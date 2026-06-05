using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AppRunner.Helpers;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
using AppRunner.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.ViewModels
{
    public partial class EnvironmentsPageModel : ObservableObject
    {
        public CollectionViewSource ItemsViewSource { get; }
        public CollectionViewSource ItemsGroupViewSource { get; }
        public DictionaryProxy<string, bool> GroupExpandedValues { get; }

        private readonly ConfigurationService _configurationService;
        private readonly EnvironmentDeploymentService _environmentDeploymentService;
        private readonly ShortcutService _shortcutService;
        private RunEnvironment? _editingEnvironmentToPopulate;

        [ObservableProperty]
        private bool _isEditEnvironmentDialogOpen;

        [ObservableProperty]
        private bool _isCreatingNewEnvironment;

        [ObservableProperty]
        private bool _showGrouped;

        [ObservableProperty]
        private string? _editEnvironmentDialogTitle;

        [ObservableProperty]
        private RunEnvironment? _editingEnvironment;

        public ObservableCollection<RunEnvironment> Environments { get; } = new();

        public EnvironmentsPageModel(
            ConfigurationService configurationService,
            EnvironmentDeploymentService environmentDeploymentService,
            ShortcutService shortcutService)
        {
            this._configurationService = configurationService;
            this._environmentDeploymentService = environmentDeploymentService;
            this._shortcutService = shortcutService;

            ShowGrouped = _configurationService.Configuration.EnvironmentsShowGroupView;

            ItemsViewSource = new CollectionViewSource()
            {
                Source = Environments,
            };

            ItemsGroupViewSource = new CollectionViewSource()
            {
                Source = Environments,
                GroupDescriptions =
                {
                    new PropertyGroupDescription(nameof(RunApp.Group)),
                }
            };

            GroupExpandedValues = new DictionaryProxy<string, bool>(_configurationService.Configuration.EnvironmentGroupExpandedValues)
            {
                DefaultValue = true,
            };
        }


        [RelayCommand]
        public void ToggleGroupView()
        {
            ShowGrouped ^= true;

            _configurationService.Configuration.EnvironmentsShowGroupView = ShowGrouped;
            _ = _configurationService.SaveConfiguration();
        }

        [RelayCommand]
        public void ShowEnvironmentUsageHint(RunEnvironment env)
        {
            MessageUtils.ShowDialogMessage(
                env.Name,
                Strings.ResourceManager.GetString("Message.HowToUseEnvironment") ?? string.Empty);
        }

        [RelayCommand]
        public void DeployEnvironment(RunEnvironment env)
        {
            if (env is null)
            {
                return;
            }

            try
            {
                _environmentDeploymentService.DeployEnvironment(env);
            }
            catch (Exception ex)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, ex.Message);
            }
        }

        [RelayCommand]
        public void CreateDeployEnvironmentShortcut(RunEnvironment env)
        {
            var shortcutPath = SelectShortcutPath(env.Name);
            if (shortcutPath is null)
            {
                return;
            }

            try
            {
                _shortcutService.CreateShortcut(
                    shortcutPath,
                    AppRunnerCommandLine.CreateDeployEnvironmentArguments(env.Guid),
                    env.Name);
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
        public void AddNewEnvironment()
        {
            EditEnvironmentDialogTitle = Strings.Title_AddNewEnvironment;
            IsEditEnvironmentDialogOpen = true;
            EditingEnvironment = new RunEnvironment();
            IsCreatingNewEnvironment = true;
        }

        [RelayCommand]
        public void EditEnvironment(RunEnvironment env)
        {
            _editingEnvironmentToPopulate = env;

            EditEnvironmentDialogTitle = Strings.Title_EditEnvironment;
            IsEditEnvironmentDialogOpen = true;
            EditingEnvironment = env.Clone();
        }

        [RelayCommand]
        public void DuplicateEnvironment(RunEnvironment env)
        {
            var copy = env.Clone();
            copy.Name = NamingUtils.CreateCopyName(env.Name, newName => !Environments.Any(anyApp => anyApp.Name == newName));

            Environments.Add(copy);

            _configurationService.Configuration.Environments = Environments.ToArray();
            _ = _configurationService.SaveConfiguration();
        }

        [RelayCommand]
        public void CancelEditEnvironmentDialog()
        {
            EditingEnvironment = null;
            IsEditEnvironmentDialogOpen = false;
            IsCreatingNewEnvironment = false;
        }

        [RelayCommand]
        public void ConfirmEditEnvironmentDialog()
        {
            if (EditingEnvironment is not null)
            {
                if (IsCreatingNewEnvironment)
                {
                    Environments.Add(EditingEnvironment);
                }
                else if (_editingEnvironmentToPopulate is not null)
                {
                    EditingEnvironment.Populate(_editingEnvironmentToPopulate);
                }
            }

            IsEditEnvironmentDialogOpen = false;
            IsCreatingNewEnvironment = false;

            _configurationService.Configuration.Environments = Environments.ToArray();
            _ = _configurationService.SaveConfiguration();
        }


        [RelayCommand]
        public void DeleteEnvironment(RunEnvironment env)
        {
            Environments.Remove(env);

            _configurationService.Configuration.Environments = Environments.ToArray();
            _ = _configurationService.SaveConfiguration();
        }
    }
}
