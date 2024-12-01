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

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage(
            ConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
            DataContext = this;
            InitializeComponent();
        }

        public ConfigurationService ConfigurationService { get; }

        private async void AnyConfigurationPropertyChanged(object sender, EventArgs e)
        {
            try
            {
                await ConfigurationService.SaveConfiguration();
            }
            catch { }
        }
    }
}
