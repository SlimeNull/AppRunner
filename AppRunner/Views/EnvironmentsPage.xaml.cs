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
    /// Interaction logic for EnvironmentsPage.xaml
    /// </summary>
    public partial class EnvironmentsPage : Page
    {
        public EnvironmentsPageModel ViewModel { get; }
        public ConfigurationService ConfigurationService { get; }

        public EnvironmentsPage(
            EnvironmentsPageModel viewModel,
            ConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            LoadFromConfiguration();
        }

        public void LoadFromConfiguration()
        {
            ViewModel.Environments.Clear();
            if (ConfigurationService.Configuration.Environments is not null)
            {
                foreach (var env in ConfigurationService.Configuration.Environments)
                {
                    ViewModel.Environments.Add(env);
                }
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement frameworkElement ||
                frameworkElement.DataContext is not CollectionViewGroup groupItem)
            {
                return;
            }

            ViewModel.GroupExpandedValues[(string)groupItem.Name] = true;
            if (!ConfigurationService.Configuration.UpdateEnvironmentGroupExpandedValue((string)groupItem.Name, true))
            {
                return;
            }

            ConfigurationService.Configuration.TrimGroupExpandedValues();
            _ = ConfigurationService.SaveConfiguration();
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement frameworkElement ||
                frameworkElement.DataContext is not CollectionViewGroup groupItem)
            {
                return;
            }

            ViewModel.GroupExpandedValues[(string)groupItem.Name] = false;
            if (!ConfigurationService.Configuration.UpdateEnvironmentGroupExpandedValue((string)groupItem.Name, false))
            {
                return;
            }

            ConfigurationService.Configuration.TrimGroupExpandedValues();
            _ = ConfigurationService.SaveConfiguration();
        }
    }
}
