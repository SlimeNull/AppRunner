using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AppRunner.Models;
using AppRunner.Resources;
using AppRunner.Services;
using CommandLine;

namespace AppRunner
{
    class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            var exitCode = Parser.Default
                .ParseArguments<AppArguments, DeployEnvironmentArguments, RunApplicationArguments>(args)
                .MapResult(
                    (AppArguments args) =>
                    {
                        ApplyCulture(args);
                        StartApp();
                        return 0;
                    },
                    (DeployEnvironmentArguments args) =>
                    {
                        ApplyCulture(args);
                        return DeployEnvironment(args);
                    },
                    (RunApplicationArguments args) =>
                    {
                        ApplyCulture(args);
                        return RunApplication(args);
                    },
                    _ => 1);

            Environment.ExitCode = exitCode;
        }

        static void ApplyCulture(AppArgumentsBase args)
        {
            if (args.Language is null)
            {
                return;
            }

            try
            {
                Strings.Culture = new CultureInfo(args.Language);
                var r = new ResourceManager(typeof(Strings));
            }
            catch { }
        }

        static void StartApp()
        {
            // 确认程序是单例?
            if (!EnsureAppSingletion())
            {
                return;
            }

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        static int DeployEnvironment(DeployEnvironmentArguments args)
        {
            try
            {
                var configurationService = new ConfigurationService();
                configurationService.LoadConfiguration().GetAwaiter().GetResult();

                var environment = configurationService.Configuration.Environments?
                    .FirstOrDefault(env => env.Guid == args.EnvironmentGuid);

                if (environment is null)
                {
                    return 2;
                }

                new EnvironmentDeploymentService(
                    new MachineEnvironmentService(),
                    new ElevationService())
                    .DeployEnvironment(environment);
                return 0;
            }
            catch
            {
                return 3;
            }
        }

        static int RunApplication(RunApplicationArguments args)
        {
            try
            {
                var configurationService = new ConfigurationService();
                configurationService.LoadConfiguration().GetAwaiter().GetResult();

                var app = configurationService.Configuration.Applications?
                    .FirstOrDefault(app => app.Guid == args.ApplicationGuid);

                if (app is null)
                {
                    return 2;
                }

                var environment = args.EnvironmentGuid is null
                    ? null
                    : configurationService.Configuration.Environments?
                        .FirstOrDefault(env => env.Guid == args.EnvironmentGuid.Value);

                if (args.EnvironmentGuid is not null && environment is null)
                {
                    return 2;
                }

                var applicationLaunchService = new ApplicationLaunchService(
                    configurationService,
                    new InjectionService(),
                    new ElevationService(),
                    new ApplicationManifestService());

                applicationLaunchService
                    .RunApplication(
                        app,
                        environment,
                        args.RunAsAdministrator ? true : null)
                    .GetAwaiter()
                    .GetResult();
                return 0;
            }
            catch
            {
                return 3;
            }
        }

        static void ShowApp()
        {
            Window mainWindow = Application.Current.MainWindow;
            if (mainWindow == null)
                return;

            mainWindow.Show();

            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;

            if (!mainWindow.IsActive)
                mainWindow.Activate();
        }

        /// <summary>
        /// 确认程序是单例运行的 / Confirm that the program is running as a singleton.
        /// </summary>
        /// <returns>当前程序是否是单例, 如果 false, 那么应该立即中止程序</returns>
        static bool EnsureAppSingletion()
        {
            var packageName = Windows.ApplicationModel.Package.Current.Id.FamilyName;

            EventWaitHandle singletonEvent = new EventWaitHandle(false, EventResetMode.AutoReset, packageName, out bool createdNew);

            if (createdNew)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        // wait for the second instance of OpenGptChat
                        singletonEvent.WaitOne();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowApp();
                        });
                    }
                });

                return true;
            }
            else
            {
                singletonEvent.Set();
                return false;
            }
        }
    }
}
