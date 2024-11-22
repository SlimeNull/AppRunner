using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
using AppRunner.ViewModels;
using AppRunner.Views;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AppRunner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; } = BuildServiceProvider();

        private static IServiceProvider BuildServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<MainWindow>();

            serviceCollection.AddSingleton<ApplicationsPage>();
            serviceCollection.AddSingleton<EnvironmentsPage>();
            serviceCollection.AddSingleton<SettingsPage>();
            serviceCollection.AddSingleton<AboutPage>();

            serviceCollection.AddSingleton<ApplicationsPageModel>();
            serviceCollection.AddSingleton<EnvironmentsPageModel>();
            serviceCollection.AddSingleton<SettingsPageModel>();
            serviceCollection.AddSingleton<AboutPageModel>();

            serviceCollection.AddSingleton<ConfigurationService>();


            return serviceCollection.BuildServiceProvider();
        }



        protected override async void OnStartup(StartupEventArgs e)
        {
            // load configuration 
            await Services
                .GetRequiredService<ConfigurationService>()
                .LoadConfiguration();

            // 

            // show main window
            Services
                .GetRequiredService<MainWindow>()
                .Show();

            base.OnStartup(e);
        }
    }

}
