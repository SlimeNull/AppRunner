using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AppRunner.Controls;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
using AppRunner.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EleCho.WpfSuite.Controls;

namespace AppRunner.ViewModels
{
    public partial class ApplicationsPageModel : ObservableObject
    {
        private readonly ConfigurationService _configurationService;
        private RunApp? _editingApplicationToPopulate;

        [ObservableProperty]
        private bool _isEditApplicationDialogOpen;

        [ObservableProperty]
        private bool _isCreatingNewApplication;

        [ObservableProperty]
        private string? _editApplicationDialogTitle;

        [ObservableProperty]
        private RunApp? _editingApplication;

        public ObservableCollection<RunApp> Applications { get; } = new();

        public ApplicationsPageModel(ConfigurationService configurationService)
        {
            this._configurationService = configurationService;
        }


        private void HandleStartException(Exception ex)
        {
            MessageUtils.ShowDialogMessage(Strings.Common_Error, ex.Message);
        }

        private void ApplyEnvironementBeforeApplicationStart(
            ProcessStartInfo processStartInfo,
            RunEnvironment? env)
        {
            if (env is null)
            {
                // do nothing
                return;
            }

            processStartInfo.WorkingDirectory = Environment.ExpandEnvironmentVariables(env.WorkingDirectory);

            if (env.EnvironmentVariables is not null)
            {
                foreach (var var in env.EnvironmentVariables)
                {
                    if (string.IsNullOrWhiteSpace(var.Key))
                    {
                        continue;
                    }

                    Environment.SetEnvironmentVariable(var.Key, var.Value);
                }
            }
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
        public void EditApplication(RunApp app)
        {
            _editingApplicationToPopulate = app;

            EditApplicationDialogTitle = Strings.Title_EditApplication;
            IsEditApplicationDialogOpen = true;
            EditingApplication = app.Clone();
        }

        [RelayCommand]
        public void CancelEditApplicationDialog()
        {
            IsEditApplicationDialogOpen = false;
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
                    EditingApplication.Populate(_editingApplicationToPopulate);
                }
            }

            IsEditApplicationDialogOpen = false;
            IsCreatingNewApplication = false;

            _configurationService.Configuration.Applications = Applications.ToArray();
            await _configurationService.SaveConfiguration();
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
            if (string.IsNullOrWhiteSpace(app.FileName))
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, Strings.Message_AppFileNameCanNotBeEmpty);
            }

            var trimmedAppFileName = app.FileName.Trim('"');

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables(trimmedAppFileName),
                Arguments = Environment.ExpandEnvironmentVariables(app.CommandLineArguments),
                CreateNoWindow = app.CreateNoWindow,
                UseShellExecute = app.UseShellExecute,
            };

            if (runAsAdministratorOverride ?? app.RunAsAdministrator)
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            var appEnvironemnt = _configurationService.Configuration.Environments?.FirstOrDefault(env => env.Guid == app.EnvironmentGuid);

            try
            {
                ApplyEnvironementBeforeApplicationStart(startInfo, environmentOverride ?? appEnvironemnt);

                var process = Process.Start(startInfo);

                if (process is not null)
                {
                    // wait for start

                    await Task.Run(() =>
                    {
                        try
                        {
                            process.WaitForInputIdle();
                        }
                        catch { }
                    });
                }
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
        public async Task DeleteApplication(RunApp app)
        {
            Applications.Remove(app);

            _configurationService.Configuration.Applications = Applications.ToArray();
            await _configurationService.SaveConfiguration();
        }
    }
}
