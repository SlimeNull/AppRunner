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
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _workingDirectory = string.Empty;

        [ObservableProperty]
        private string _commandLineArguments = string.Empty;

        [ObservableProperty]
        private bool _runAsAdministrator;

        [ObservableProperty]
        private bool _useShellExecute;

        [ObservableProperty]
        private bool _createNoWindow;


        [RelayCommand]
        public async Task Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = FileName,
                WorkingDirectory = WorkingDirectory,
                Arguments = CommandLineArguments,
                CreateNoWindow = CreateNoWindow,
            };

            if (RunAsAdministrator)
            {
                startInfo.Verb = "RunAs";
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
            catch  (Exception ex)
            {
                // TODO: show error message
            }
        }
    }
}
