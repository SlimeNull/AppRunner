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
using AppRunner.Services;
using AppRunner.ViewModels;

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for EnvironmentsPage.xaml
    /// </summary>
    public partial class EnvironmentsPage : Page
    {
        public EnvironmentsPage(
            EnvironmentsPageModel viewModel,
            ConfigurationService configurationService)
        {
            ConfigurationService = configurationService;

            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            if (ConfigurationService.Configuration.Environments is not null)
            {
                foreach (var env in ConfigurationService.Configuration.Environments)
                {
                    ViewModel.Environments.Add(env);
                }
            }
        }

        public EnvironmentsPageModel ViewModel { get; }
        public ConfigurationService ConfigurationService { get; }
    }
}
