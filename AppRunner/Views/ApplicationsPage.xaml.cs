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
using AppRunner.Resources;
using AppRunner.Services;
using AppRunner.ViewModels;
using AppRunner.Utilities;
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
        private readonly ApplicationFileImportService _applicationFileImportService;

        public ApplicationsPage(
            ApplicationsPageModel viewModel,
            ConfigurationService configurationService,
            ApplicationFileImportService applicationFileImportService)
        {
            ConfigurationService = configurationService;
            ViewModel = viewModel;
            _applicationFileImportService = applicationFileImportService;
            DataContext = this;

            InitializeComponent();
            LoadFromConfiguration();
        }

        public void LoadFromConfiguration()
        {
            ViewModel.Applications.Clear();
            if (ConfigurationService.Configuration.Applications is not null)
            {
                foreach (var app in ConfigurationService.Configuration.Applications)
                {
                    ViewModel.Applications.Add(app);
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
            if (!ConfigurationService.Configuration.UpdateApplicationGroupExpandedValue((string)groupItem.Name, true))
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
            if (!ConfigurationService.Configuration.UpdateApplicationGroupExpandedValue((string)groupItem.Name, false))
            {
                return;
            }

            ConfigurationService.Configuration.TrimGroupExpandedValues();
            _ = ConfigurationService.SaveConfiguration();
        }

        private void Page_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = CanImportDroppedFile(e)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void Page_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = CanImportDroppedFile(e)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void Page_PreviewDrop(object sender, DragEventArgs e)
        {
            try
            {
                var filePath = GetDroppedFilePath(e);
                if (filePath is null)
                {
                    return;
                }

                ViewModel.AddNewApplication(
                    _applicationFileImportService.CreateApplicationFromFile(filePath));
            }
            catch (Exception ex)
            {
                MessageUtils.ShowDialogMessage(Strings.Common_Error, ex.Message);
            }
            finally
            {
                e.Handled = true;
            }
        }

        private static bool CanImportDroppedFile(DragEventArgs e)
        {
            return GetDroppedFilePath(e) is not null;
        }

        private static string? GetDroppedFilePath(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return null;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
            {
                return null;
            }

            return files.FirstOrDefault(System.IO.File.Exists);
        }
    }
}
