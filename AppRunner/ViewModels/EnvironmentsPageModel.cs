using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
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
            EnvironmentDeploymentService environmentDeploymentService)
        {
            this._configurationService = configurationService;
            this._environmentDeploymentService = environmentDeploymentService;

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
        public void DeployEnvironment(RunEnvironment env)
        {
            if (env is null)
            {
                return;
            }

            if (!IsRunningAsAdministrator())
            {
                StartElevatedDeployEnvironment(env);
                return;
            }

            _environmentDeploymentService.DeployEnvironment(env);
        }

        private static bool IsRunningAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void StartElevatedDeployEnvironment(RunEnvironment env)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "apprunner.exe",
                    Arguments = $"deploy \"{env.Guid}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                });
            }
            catch (Exception ex)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, ex.Message);
            }
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
