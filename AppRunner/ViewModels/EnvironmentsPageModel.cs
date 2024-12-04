using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AppRunner.Helpers;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
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

        public EnvironmentsPageModel(ConfigurationService configurationService)
        {
            this._configurationService = configurationService;

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
            Environment.CurrentDirectory = env.WorkingDirectory;

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
        public void CancelEditEnvironmentDialog()
        {
            IsEditEnvironmentDialogOpen = false;
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
