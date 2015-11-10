// <copyright company="SIX Networks GmbH" file="App.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using SimpleInjector;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Presentation.SA;
using SN.withSIX.Core.Presentation.SA.ViewModels;
using SN.withSIX.Core.Presentation.SA.Views;
using SN.withSIX.Core.Presentation.Wpf;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Core;
using SN.withSIX.Mini.Infra.Api;
using Splat;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : SingleInstanceApp
    {
        AppBootstrapper _bootstrapper;

        public App() {
            AppEvent += OnAppEvent;
#if !DEBUG
            SetupExceptionHandler();
#endif
        }

        void OnAppEvent(object sender, IList<string> list) {
            var mainWindow = MainWindow;
            if (mainWindow != null) {
                mainWindow.Show();
                mainWindow.Activate();
            }
            HandleNewVersion(list);
        }

        void HandleNewVersion(IEnumerable<string> list) {
            if (list.Select(a => new {a, newversion = "-NewVersion="})
                .Where(@t => @t.a.StartsWith(@t.newversion))
                .Select(@t => @t.a.Replace(@t.newversion, ""))
                .Any(newVer => newVer != Consts.ProductVersion)) {
                var mainWindow = MainWindow;
                var text = "You tried to start a different version, please exit the current version and try again";
                var title = "Started another version";
                if (mainWindow == null)
                    MessageBox.Show(text, title);
                else
                    MessageBox.Show(mainWindow, text, title);
            }
        }

        void SetupExceptionHandler() {
            DispatcherUnhandledException += ExDialog.OnDispatcherUnhandledException;
            //AppDomain.CurrentDomain.UnhandledException += ExDialog.CurrentDomainOnUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            _bootstrapper = new AppBootstrapper(new Container(), Locator.CurrentMutable);
            // TODO: In command mode perhaps we shouldnt even run a WpfApp instance or ??
            if (Entrypoint.CommandMode)
                RunCommands();
            else {
                _bootstrapper.Startup();
                CreateMainWindow();
                _bootstrapper.AfterWindow();
            }
        }

        void RunCommands() {
            Environment.Exit(
                _bootstrapper.GetCommandMode().RunCommandsAndLog(Environment.GetCommandLineArgs().Skip(1).ToArray()));
        }

        void CreateMainWindow() {
            var miniMainWindow = new MiniMainWindow {ViewModel = _bootstrapper.GetMainWindowViewModel()};
            miniMainWindow.Show();
            //MainWindow = miniMainWindow;
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
            try {
                _bootstrapper?.Dispose();
            } finally {
                SingleInstance<App>.Cleanup();
            }
        }

        class ExDialog
        {
            public static void CurrentDomainOnUnhandledException(object sender,
                UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
                HandleEx((Exception) unhandledExceptionEventArgs.ExceptionObject);
            }

            public static void OnDispatcherUnhandledException(object sender,
                DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs) {
                HandleEx(dispatcherUnhandledExceptionEventArgs.Exception);
            }

            static void HandleEx(Exception ex) {
                MainLog.Logger.Write(ex.Format(), LogLevel.Fatal);
                if (ex is CannotOpenApiPortException) {
                    MessageBox.Show(
                        ex.Message + "\n\n"
                        +
                        "The application will exit now. We've been notified about the problem. Sorry for the inconvenience\n\nIf the problem persists, please contact Support: http://community.withsix.com",
                        "Unrecoverable error occurred");
                } else {
                    MessageBox.Show(
                        "The application will exit now. We've been notified about the problem. Sorry for the inconvenience\n\nIf the problem persists, please contact Support: http://community.withsix.com",
                        "Unrecoverable error occurred");
                }
                Environment.Exit(1);
                //ShowMessageboxEx(ex);
            }

            static void ShowMessageboxEx(Exception ex) {
                var exInfo = KnownExceptions.FormatException(ex);
                ShowDialog(ex, exInfo);
                ShowDialog(ex, exInfo);
            }

            static void ShowDialog(Exception ex, string exInfo) {
                new ExceptionDialogView {
                    DataContext =
                        new ExceptionDialogViewModel(exInfo) {
                            Exception = ex,
                            Message = ex.Message,
                            Title = "An unhandled exception occurred"
                        }
                }.ShowDialog();
            }
        }
    }
}