using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    [ObservableObject]
    public partial class AboutPage : Page
    {
        [ObservableProperty]
        private bool _isLicenseDialogOpen;

        [ObservableProperty]
        private string? _currentLicense;

        public AboutPage()
        {
            DataContext = this;
            InitializeComponent();
        }


        [RelayCommand]
        public void OpenGithubRepository()
        {
            Process.Start(
                new ProcessStartInfo()
                {
                    FileName = @"https://github.com/SlimeNull/AppRunner",
                    UseShellExecute = true,
                });
        }

        [RelayCommand]
        public void WhoIsSlimeNull()
        {
            Process.Start(
                new ProcessStartInfo()
                {
                    FileName = @"https://slimenull.com",
                    UseShellExecute = true,
                });
        }

        [RelayCommand]
        public void ViewLicense(string license)
        {
            CurrentLicense = license;
            IsLicenseDialogOpen = true;
        }

        [RelayCommand]
        public void CloseLicense()
        {
            IsLicenseDialogOpen = false;
        }

        [RelayCommand]
        public void OpenUrl(string url)
        {
            Process.Start(
                new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true,
                });
        }
    }
}
