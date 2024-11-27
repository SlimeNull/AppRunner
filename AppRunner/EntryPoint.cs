using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AppRunner.Models;
using AppRunner.Resources;
using CommandLine;

namespace AppRunner
{
    class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<AppArguments>(args)
                .WithParsed(args =>
                {
                    if (args.Language is not null)
                    {
                        try
                        {
                            Strings.Culture = new CultureInfo(args.Language);
                            var r = new ResourceManager(typeof(Strings));
                        }
                        catch { }
                    }
                });

            // 确认程序是单例?
            if (!EnsureAppSingletion())
            {
                return;
            }

            var app = new App();
            app.InitializeComponent();
            app.Run();
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
