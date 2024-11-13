using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.ViewModels
{
    public partial class EnvironmentsPageModel : ObservableObject
    {
        private readonly ConfigurationService _configurationService;
        private RunEnvironment? _editingEnvironmentToPopulate;

        [ObservableProperty]
        private bool _isEditEnvironmentDialogOpen;

        [ObservableProperty]
        private bool _isCreatingNewEnvironment;

        [ObservableProperty]
        private string? _editEnvironmentDialogTitle;

        [ObservableProperty]
        private RunEnvironment? _editingEnvironment;

        public ObservableCollection<RunEnvironment> Environments { get; } = new();

        public EnvironmentsPageModel(ConfigurationService configurationService)
        {
            this._configurationService = configurationService;
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
            _configurationService.SaveConfiguration();
        }
    }
}
