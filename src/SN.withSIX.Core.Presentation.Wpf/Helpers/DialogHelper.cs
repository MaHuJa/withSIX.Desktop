// <copyright company="SIX Networks GmbH" file="DialogHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.SA;

namespace SN.withSIX.Core.Presentation.Wpf.Helpers
{
    public static class DialogHelper
    {
        static bool _mainWindowLoaded;
        public static bool MainWindowLoaded
        {
            get { return _mainWindowLoaded; }
            set
            {
                _mainWindowLoaded = value;
                KnownExceptions.MainWindowShown = value;
            }
        }

        public static void ActivateWindows(bool activate = true, bool allWindows = true) {
            var app = Application.Current;
            var mainWindow = app.MainWindow;

            if (mainWindow != null)
                ActivateMainWindow(activate, mainWindow);

            if (allWindows)
                ActivateAllWindows(activate, app, mainWindow);
        }

        static void ActivateMainWindow(bool activate, Window mainWindow) {
            if (!activate) {
                mainWindow.Hide();
                return;
            }

            SetToNormalIfMinimized(mainWindow);

            TryActivateMainWindow(mainWindow);
        }

        static void SetToNormalIfMinimized(Window mainWindow) {
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
        }

        static void TryActivateMainWindow(Window mainWindow) {
            try {
                mainWindow.Show();
                mainWindow.Activate();
            } catch (InvalidOperationException e) {
                MainLog.Logger.FormattedDebugException(e);
            }
        }

        static void ActivateAllWindows(bool activate, Application app, Window mainWindow) {
            foreach (var window in app.Windows
                .Cast<Window>()
                .Where(x => x != null && x != mainWindow)
                .ToArray()) {
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;

                if (!activate) {
                    window.Hide();
                    return;
                }

                TryShowWindow(window);
            }
        }

        static void TryShowWindow(Window window) {
            try {
                window.Show();
            } catch (InvalidOperationException e) {
                MainLog.Logger.FormattedDebugException(e);
            }
        }

        public static bool TryIsDesktopCompositionEnabled() {
            try {
                bool glassEnabled;
                NativeMethods.DwmIsCompositionEnabled(out glassEnabled);
                return glassEnabled;
            } catch {
                return false;
            }
        }

        static Window GetMainWindow() {
            var app = Application.Current;
            return app == null || !MainWindowLoaded ? null : app.MainWindow;
        }

        public static void SetMainWindowOwner(Window window) {
            var mw = GetMainWindow();
            if (mw == window)
                return;
            window.Owner = mw;
        }

        public class NoSynchronizationContextScope : IDisposable
        {
            readonly SynchronizationContext _synchronizationContext;

            public NoSynchronizationContextScope() {
                _synchronizationContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
            }

            public void Dispose() {
                SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
            }
        }

        class NativeMethods
        {
            [DllImport("dwmapi.dll")]
            public static extern IntPtr DwmIsCompositionEnabled(out bool pfEnabled);

            [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern int GetDoubleClickTime();

            public static void RestoreWindow(Process process) {
                Contract.Requires<ArgumentNullException>(process != null);

                Tools.ProcessesTools.NativeMethods.ShowWindow(process.MainWindowHandle,
                    Tools.ProcessesTools.NativeMethods.SW_RESTORE);
            }

            public static void RestoreWindow(Window window) {
                Contract.Requires<ArgumentNullException>(window != null);
                Tools.ProcessesTools.NativeMethods.ShowWindow(
                    (PresentationSource.FromVisual(window) as HwndSource).Handle,
                    Tools.ProcessesTools.NativeMethods.SW_RESTORE);
            }

            public static void RestoreWindow() {
                RestoreWindow(Application.Current.MainWindow);
            }
        }
    }
}