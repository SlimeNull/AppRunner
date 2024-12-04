using System;
using System.Collections.Generic;
using System.IO;
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
using AppRunner.Resources;
using AppRunner.Services;
using AppRunner.Utilities;
using Microsoft.Win32;

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private OpenFileDialog? _importConfigurationDialog;
        private SaveFileDialog? _exportConfigurationDialog;

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

        private async void ImportConfigurationButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _importConfigurationDialog = _importConfigurationDialog ?? new OpenFileDialog()
                {
                    DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    Title = Strings.Title_ImportConfiguration,
                    FileName = "AppRunner.Configuration",
                    CheckFileExists = true,
                    Filter = "AppRunner Configuration|*.json|Any|*.*"
                };

                var dialogResult = _importConfigurationDialog.ShowDialog();
                if (dialogResult != true)
                {
                    return;
                }

                var ok = await ConfigurationService.ImportFrom(_importConfigurationDialog.FileName);

                if (!ok)
                {
                    MessageUtils.ShowDialogMessage(Strings.Common_Error, Strings.Message_FailedToImportConfigurationPleaseCheckIfTheFileFormatIsCorrect);
                }
            }
            catch (Exception ex)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, $"{Strings.Message_FailedToImportConfiguration}. {ex.Message}");
            }
        }

        private async void ExportConfigurationButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _exportConfigurationDialog = _exportConfigurationDialog ?? new SaveFileDialog()
                {
                    DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    Title = Strings.Title_ExportConfiguration,
                    FileName = "AppRunner.Configuration",
                    CheckPathExists = true,
                    Filter = "AppRunner Configuration|*.json|Any|*.*"
                };

                var dialogResult = _exportConfigurationDialog.ShowDialog();
                if (dialogResult != true)
                {
                    return;
                }

                await ConfigurationService.ExportTo(_exportConfigurationDialog.FileName);
            }
            catch (IOException)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, Strings.Message_FailedToExportConfigurationPleaseCheckIfYouCurrentlyHaveAccessRightsToTargetPath);
            }
            catch (Exception ex)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, $"{Strings.Message_FailedToImportConfiguration}. {ex.Message}");
            }
        }
    }
}
