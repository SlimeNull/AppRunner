using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AppRunner.Models;
using AppRunner.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for ApplicationsPage.xaml
    /// </summary>
    public partial class ApplicationsPage : Page
    {
        public ApplicationsPageModel ViewModel { get; }

        public ApplicationsPage(
            ApplicationsPageModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }


        [RelayCommand]
        public void AddNewApplication()
        {
            ViewModel.IsAddApplicationDialogOpen = true;
            ViewModel.CreatingApplication = new RunApp();
        }

        [RelayCommand]
        public void CancelAddApplicationDialog()
        {
            ViewModel.IsAddApplicationDialogOpen = false;
        }


        [RelayCommand]
        public void ConfirmAddApplicationDialog()
        {
            ViewModel.IsAddApplicationDialogOpen = false;

            if (ViewModel.CreatingApplication is not null)
            {
                ViewModel.Applications.Add(ViewModel.CreatingApplication);
            }
        }
    }
}
