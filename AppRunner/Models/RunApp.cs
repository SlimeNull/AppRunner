using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AppRunner.Controls;
using AppRunner.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EleCho.WpfSuite.Controls;

namespace AppRunner.Models
{
    public partial class RunApp : ObservableObject
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _commandLineArguments = string.Empty;

        [ObservableProperty]
        private Guid _environmentGuid;

        [ObservableProperty]
        private bool _runAsAdministrator;

        [ObservableProperty]
        private bool _useShellExecute;

        [ObservableProperty]
        private bool _createNoWindow;

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

        public RunApp Clone()
        {
            var newApp = new RunApp();
            Populate(newApp);

            return newApp;
        }

        public void Populate(RunApp runApp)
        {
            runApp.Name = Name;
            runApp.FileName = FileName;
            runApp.Description = Description;
            runApp.CommandLineArguments = CommandLineArguments;
            runApp.RunAsAdministrator = RunAsAdministrator;
            runApp.CreateNoWindow = CreateNoWindow;
            runApp.UseShellExecute = UseShellExecute;
        }


        [RelayCommand]
        public async Task Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = FileName,
                Arguments = CommandLineArguments,
                CreateNoWindow = CreateNoWindow,
                UseShellExecute = UseShellExecute,
            };

            if (RunAsAdministrator)
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            try
            {
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
        public async Task StartAsAdministrator()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = FileName,
                Arguments = CommandLineArguments,
                CreateNoWindow = CreateNoWindow,
                UseShellExecute = true,
                Verb = "RunAs"
            };

            try
            {
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
