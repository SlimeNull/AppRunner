
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
        private readonly Dictionary<Type, Page> _cachedPages = new Dictionary<Type, Page>();

        public ObservableCollection<AppNavigationItem> AppNavigationItems { get; } = new()
        {
            new AppNavigationItem("\uF1B3", Strings.PageName_Applications, typeof(ApplicationsPage))
            {
                PageFactory = () => App.Services.GetRequiredService<ApplicationsPage>(),
                PageCommandsFactory = page =>
                {
                    var appsPage = (ApplicationsPage)page;

                    return
                    [
                        new PageCommand("\uf279", Strings.Common_View, appsPage.ViewModel.ToggleGroupViewCommand),
                        new PageCommand("\uF067", Strings.Common_Add, appsPage.ViewModel.AddNewApplicationCommand),
                    ];
                }
            },

            new AppNavigationItem("\uF5FD", Strings.PageName_Environments, typeof(EnvironmentsPage))
            {
                PageFactory = () => App.Services.GetRequiredService<EnvironmentsPage>(),
                PageCommandsFactory = page =>
                {
                    var envsPage = (EnvironmentsPage)page;

                    return
                    [
                        new PageCommand("\uf279", Strings.Common_View, envsPage.ViewModel.ToggleGroupViewCommand),
                        new PageCommand("\uF067", Strings.Common_Add, envsPage.ViewModel.AddNewEnvironmentCommand),
                    ];
                }
            },

            new AppNavigationItem("\u2699", Strings.PageName_Settings, typeof(SettingsPage))
            {
                PageFactory = () => App.Services.GetRequiredService<SettingsPage>(),
            },

            new AppNavigationItem("\uf02d", Strings.PageName_About, typeof(AboutPage), false)
            {
                PageFactory = () => App.Services.GetRequiredService<AboutPage>(),
                PageCommandsFactory = page =>
                {
                    var aboutPage = (AboutPage)page;

                    return
                    [
                        new PageCommand("\uF09B", "GitHub", aboutPage.OpenGithubRepositoryCommand, PageCommand.FontAwesomeBrands)
                    ];
                }
            }
        };

        [ObservableProperty]
        private Page? _currentPage;

        [ObservableProperty]
        private string? _currentPageTitle;

        [ObservableProperty]
        private IEnumerable<PageCommand>? _currentPageCommands;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void UpdatePage(AppNavigationItem navigationItem)
        {
            CurrentPageTitle = navigationItem.Title;

            if (_cachedPages.TryGetValue(navigationItem.TargetPageType, out var page))
            {
                CurrentPage = page;
            }
            else
            {
                if (navigationItem.PageFactory is null)
                {
                    CurrentPage = null;
                    return;
                }

                CurrentPage = navigationItem.PageFactory.Invoke();

                if (navigationItem.Cache)
                {
                    _cachedPages[navigationItem.TargetPageType] = CurrentPage;
                }
            }

            if (navigationItem.PageCommandsFactory is null)
            {
                CurrentPageCommands = null;
                return;
            }

            CurrentPageCommands = navigationItem.PageCommandsFactory.Invoke(CurrentPage);
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

        private void NavigatorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems is not [AppNavigationItem navigationItem])
            {
                return;
            }

            UpdatePage(navigationItem);
        }
    }
}