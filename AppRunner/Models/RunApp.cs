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
            runApp.EnvironmentGuid = EnvironmentGuid;
        }


        [RelayCommand]
        public async Task Start()
        {
        }

        [RelayCommand]
        public async Task StartAsAdministrator()
        {
        }
    }
}
