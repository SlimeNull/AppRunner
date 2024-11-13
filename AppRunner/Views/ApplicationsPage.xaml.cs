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
using AppRunner.Services;
using AppRunner.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for ApplicationsPage.xaml
    /// </summary>
    public partial class ApplicationsPage : Page
    {
        public ConfigurationService ConfigurationService { get; }
        public ApplicationsPageModel ViewModel { get; }

        public ApplicationsPage(
            ApplicationsPageModel viewModel,
            ConfigurationService configurationService)
        {
            ConfigurationService = configurationService;

            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            if (ConfigurationService.Configuration.Applications is not null)
            {
                foreach (var app in ConfigurationService.Configuration.Applications)
                {
                    ViewModel.Applications.Add(app);
                }
            }
        }
    }
}
