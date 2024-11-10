
using System.Collections.ObjectModel;
using System.Text;
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
using AppRunner.Resources;
using AppRunner.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EleCho.WpfSuite.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace AppRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        public ObservableCollection<AppNavigationItem> AppNavigationItems { get; } = new()
        {
            new AppNavigationItem("\uF1B3", Strings.PageName_Applications, App.Services.GetRequiredService<ApplicationsPage>())
            {
                PageCommands =
                {
                    new PageCommand("\uF067", Strings.Common_Add, App.Services.GetRequiredService<ApplicationsPage>().AddNewApplicationCommand)
                }
            },

            new AppNavigationItem("\uF5FD", Strings.PageName_Environments, App.Services.GetRequiredService<EnvironmentsPage>()),

            new AppNavigationItem("\u2699", Strings.PageName_Settings, App.Services.GetRequiredService<SettingsPage>()),

            new AppNavigationItem("\uF1B3", Strings.PageName_About, App.Services.GetRequiredService<AboutPage>())
            {
                PageCommands =
                {
                    new PageCommand("\uF09B", "GitHub", App.Services.GetRequiredService<AboutPage>().OpenGithubRepositoryCommand)

                }
            }
        };

        [ObservableProperty]
        private AppNavigationItem _currentNavigationItem;

        public MainWindow()
        {
            _currentNavigationItem = AppNavigationItems[0];

            DataContext = this;
            InitializeComponent();
        }


        [RelayCommand]
        public void MinimizeSelf()
        {
            WindowState = WindowState.Minimized;
        }

        [RelayCommand]
        public void MaximizeSelf()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        [RelayCommand]
        public void CloseSelf()
        {
            Close();
        }

        private void AppFrame_Navigated(object sender, NavigationEventArgs e)
        {
            while (AppFrame.CanGoBack)
            {
                AppFrame.RemoveBackEntry();
            }
        }
    }
}