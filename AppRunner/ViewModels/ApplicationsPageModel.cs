using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using AppRunner.Controls;
using AppRunner.Helpers;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
using AppRunner.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.ViewModels
{
    public partial class ApplicationsPageModel : ObservableObject
    {
        public CollectionViewSource ItemsViewSource { get; } 
        public CollectionViewSource ItemsGroupViewSource { get; }
        public DictionaryProxy<string, bool> GroupExpandedValues { get; }

        private readonly ConfigurationService _configurationService;
        private readonly ApplicationLaunchService _applicationLaunchService;
        private readonly ShortcutService _shortcutService;
        private RunApp? _editingApplicationToPopulate;

        [ObservableProperty]
        private bool _isEditApplicationDialogOpen;

        [ObservableProperty]
        private bool _isSelectGroupDialogOpen;

        [ObservableProperty]
        private bool _isCreatingNewApplication;

        [ObservableProperty]
        private bool _showGrouped = true;

        [ObservableProperty]
        private string? _editApplicationDialogTitle;

        [ObservableProperty]
        private RunApp? _editingApplication;

        public ObservableCollection<RunApp> Applications { get; } = new();

        public ApplicationsPageModel(
            ConfigurationService configurationService,
            ApplicationLaunchService applicationLaunchService,
            ShortcutService shortcutService)
        {
            this._configurationService = configurationService;
            this._applicationLaunchService = applicationLaunchService;
            this._shortcutService = shortcutService;

            ShowGrouped = _configurationService.Configuration.ApplicationsShowGroupView;

            ItemsViewSource = new CollectionViewSource()
            {
                Source = Applications,
            };

            ItemsGroupViewSource = new CollectionViewSource()
            {
                Source = Applications,
                GroupDescriptions =
                {
                    new PropertyGroupDescription(nameof(RunApp.Group)),
                }
            };

            GroupExpandedValues = new DictionaryProxy<string, bool>(_configurationService.Configuration.ApplicationGroupExpandedValues)
            {
                DefaultValue = true,
            };
        }


        private void HandleStartException(Exception ex)
        {
            MessageUtils.ShowDialogMessage(Strings.Common_Error, ex.Message);
        }

        [RelayCommand]
        public void AddNewApplication()
        {
            EditApplicationDialogTitle = Strings.Title_AddNewApplication;
            IsEditApplicationDialogOpen = true;
            EditingApplication = new RunApp();
            IsCreatingNewApplication = true;
        }

        [RelayCommand]
        public void ToggleGroupView()
        {
            ShowGrouped ^= true;

            _configurationService.Configuration.ApplicationsShowGroupView = ShowGrouped;
            _ = _configurationService.SaveConfiguration();
        }

        [RelayCommand]
        public void EditApplication(RunApp app)
        {
            _editingApplicationToPopulate = app;

            EditApplicationDialogTitle = Strings.Title_EditApplication;
            IsEditApplicationDialogOpen = true;
            EditingApplication = app.Clone();
        }

        [RelayCommand]
        public void DuplicateApplication(RunApp app)
        {
            var copy = app.Clone();
            copy.Name = NamingUtils.CreateCopyName(app.Name, newName => !Applications.Any(anyApp => anyApp.Name == newName));

            Applications.Add(copy);

            _configurationService.Configuration.Applications = Applications.ToArray();
            _ = _configurationService.SaveConfiguration();
        }

        [RelayCommand]
        public void CancelEditApplicationDialog()
        {
            EditingApplication = null;
            IsEditApplicationDialogOpen = false;
            IsCreatingNewApplication = false;
        }

        [RelayCommand]
        public async Task ConfirmEditApplicationDialog()
        {
            if (EditingApplication is not null)
            {
                if (IsCreatingNewApplication)
                {
                    Applications.Add(EditingApplication);
                }
                else if (_editingApplicationToPopulate is not null)
                {
                    _editingApplicationToPopulate.Group = _editingApplicationToPopulate.Group.Trim();
                    EditingApplication.Populate(_editingApplicationToPopulate);
                }
            }

            IsEditApplicationDialogOpen = false;
            IsCreatingNewApplication = false;

            _configurationService.Configuration.Applications = Applications.ToArray();
            await _configurationService.SaveConfiguration();

            ItemsGroupViewSource.View.Refresh();
        }

        [RelayCommand]
        public void SelectGroupForEditingApplication()
        {
            IsSelectGroupDialogOpen = true;
        }

        [RelayCommand]
        public void ApplyEnvironmentVariables(RunEnvironment env)
        {
            if (env.EnvironmentVariables is null)
            {
                return;
            }

            foreach (var var in env.EnvironmentVariables)
            {
                if (string.IsNullOrWhiteSpace(var.Key))
                {
                    continue;
                }

                Environment.SetEnvironmentVariable(var.Key, var.Value);
            }
        }

        public async Task RunApplication(RunApp app, RunEnvironment? environmentOverride, bool? runAsAdministratorOverride)
        {
            try
            {
                await _applicationLaunchService.RunApplication(app, environmentOverride, runAsAdministratorOverride);
            }
            catch (Exception ex)
            {
                HandleStartException(ex);
            }
        }

        [RelayCommand]
        public Task RunApplication(RunApp app)
        {
            return RunApplication(app, null, null);
        }

        [RelayCommand]
        public Task RunApplicationWithEnvironment(RunAppAndEnvironmentGuid appAndEnvironment)
        {
            var app = appAndEnvironment.App;
            var envGuid = appAndEnvironment.EnvironmentGuid;

            if (app is null)
            {
                throw new InvalidOperationException("This would never happen");
            }

            var specifiedEnvironment =
                _configurationService.Configuration.Environments?.FirstOrDefault(env => env.Guid == envGuid);

            return RunApplication(app, specifiedEnvironment, null);
        }

        [RelayCommand]
        public Task RunApplicationAsAdministrator(RunApp app)
        {
            return RunApplication(app, null, true);
        }

        [RelayCommand]
        public void CreateRunApplicationShortcut(RunApp app)
        {
            CreateRunApplicationShortcut(app, null, false);
        }

        [RelayCommand]
        public void CreateRunApplicationAsAdministratorShortcut(RunApp app)
        {
            CreateRunApplicationShortcut(app, null, true);
        }

        [RelayCommand]
        public void CreateRunApplicationWithEnvironmentShortcut(RunAppAndEnvironmentGuid appAndEnvironment)
        {
            if (appAndEnvironment.App is null)
            {
                return;
            }

            CreateRunApplicationShortcut(
                appAndEnvironment.App,
                appAndEnvironment.EnvironmentGuid,
                false);
        }

        private void CreateRunApplicationShortcut(
            RunApp app,
            Guid? environmentGuid,
            bool runAsAdministrator)
        {
            var shortcutPath = SelectShortcutPath(app.Name);
            if (shortcutPath is null)
            {
                return;
            }

            try
            {
                _shortcutService.CreateShortcut(
                    shortcutPath,
                    AppRunnerCommandLine.CreateRunApplicationArguments(
                        app.Guid,
                        environmentGuid,
                        runAsAdministrator),
                    app.Name);
            }
            catch (Exception ex)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, ex.Message);
            }
        }

        private static string? SelectShortcutPath(string name)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = ".lnk",
                FileName = ShortcutService.GetSafeShortcutFileName(name) + ".lnk",
                Filter = "Shortcut (*.lnk)|*.lnk",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                OverwritePrompt = true,
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        [RelayCommand]
        public async Task DeleteApplication(RunApp app)
        {
            Applications.Remove(app);

            _configurationService.Configuration.Applications = Applications.ToArray();
            await _configurationService.SaveConfiguration();
        }
    }
}
