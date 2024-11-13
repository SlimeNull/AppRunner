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
            if (DialogLayer.GetDialogLayer(Application.Current.MainWindow) is DialogLayer dialogLayer)
            {
                dialogLayer.Push(new Dialog()
                {
                    Content = new SimpleMessageToast()
                    {
                        MaxWidth = 450,
                        Title = Strings.Common_Error,
                        Message = ex.Message,
                    }
                });
            }
        }

        private void ApplyEnvironementBeforeApplicationStart(
            ProcessStartInfo processStartInfo,
            Guid environmentGuid)
        {
            if (_configurationService.Configuration.Environments?.FirstOrDefault(env => env.Guid == environmentGuid) is not RunEnvironment env)
            {
                return;
            }

            processStartInfo.WorkingDirectory = Environment.ExpandEnvironmentVariables(env.WorkingDirectory);

            foreach (var var in env.EnvironmentVariables)
            {
                if (string.IsNullOrWhiteSpace(var.Key))
                {
                    continue;
                }

                Environment.SetEnvironmentVariable(var.Key, var.Value);
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
        public void ConfirmEditApplicationDialog()
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
            _configurationService.SaveConfiguration();
        }

        [RelayCommand]
        public async Task ApplyEnvironmentVariables(RunEnvironment env)
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

        [RelayCommand]
        public async Task RunApplication(RunApp app)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables(app.FileName),
                Arguments = Environment.ExpandEnvironmentVariables(app.CommandLineArguments),
                CreateNoWindow = app.CreateNoWindow,
                UseShellExecute = app.UseShellExecute,
            };

            if (app.RunAsAdministrator)
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            try
            {
                ApplyEnvironementBeforeApplicationStart(startInfo, app.EnvironmentGuid);

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
        public async Task RunApplicationAsAdministrator(RunApp app)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables(app.FileName),
                Arguments = Environment.ExpandEnvironmentVariables(app.CommandLineArguments),
                CreateNoWindow = app.CreateNoWindow,
                UseShellExecute = true,
                Verb = "RunAs"
            };

            try
            {
                ApplyEnvironementBeforeApplicationStart(startInfo, app.EnvironmentGuid);

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
    }
}
