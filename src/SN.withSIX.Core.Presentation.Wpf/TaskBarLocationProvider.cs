// <copyright company="SIX Networks GmbH" file="TaskBarLocationProvider.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace SN.withSIX.Core.Presentation.Wpf
{
    public static class TaskBarLocationProvider
    {
        // P/Invoke goo:
        const int ABM_GETTASKBARPOS = 5;

        [DllImport("shell32.dll")]
        static extern IntPtr SHAppBarMessage(int msg, ref AppBarData data);

        static Rectangle GetTaskBarCoordinates(Rect rc) {
            return new Rectangle(rc.Left, rc.Top,
                rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        static AppBarData GetTaskBarLocation() {
            var data = new AppBarData();
            data.cbSize = Marshal.SizeOf(data);

            IntPtr retval = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

            if (retval == IntPtr.Zero)
                throw new Win32Exception("WinAPi Error: does'nt work api method SHAppBarMessage");

            return data;
        }

        static Screen FindScreenWithTaskBar(Rectangle taskBarCoordinates) {
            foreach (var screen in Screen.AllScreens) {
                if (screen.Bounds.Contains(taskBarCoordinates))
                    return screen;
            }

            return Screen.PrimaryScreen;
        }

        /// <summary>
        ///     Calculate wpf window position for place it near to taskbar area
        /// </summary>
        /// <param name="windowWidth">target window height</param>
        /// <param name="windowHeight">target window width</param>
        /// <param name="left">Result left coordinate <see cref="System.Windows.Window.Left" /></param>
        /// <param name="top">Result top coordinate <see cref="System.Windows.Window.Top" /></param>
        public static void CalculateWindowPositionByTaskbar(Window window, double windowWidth, double windowHeight,
            out double left,
            out double top, bool include = false) {
            var taskBarLocation = GetTaskBarLocation();
            var taskBarRectangle = GetTaskBarCoordinates(taskBarLocation.rc);
            var screen = FindScreenWithTaskBar(taskBarRectangle);

            // TODO: Get DPI from monitor that the taskbar is on!
            // http://dzimchuk.net/post/Best-way-to-get-DPI-value-in-WPF  ??
            var source = PresentationSource.FromVisual(Application.Current.MainWindow ?? window);
            var m11 = source.CompositionTarget.TransformToDevice.M11;
            var m22 = source.CompositionTarget.TransformToDevice.M22;
            var dpiX = 96.0*m11;
            var dpiY = 96.0*m22;

            windowWidth = windowWidth*m11;
            windowHeight = windowHeight*m22;

            //var source = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice;
            //var workingArea = GetScreen(window).WorkingArea;
            //var corner = RealPixelsToWpf(window, new Point(left, top));

            left = taskBarLocation.uEdge == (int) Dock.Left
                ? screen.Bounds.X + taskBarRectangle.Width
                : screen.Bounds.X + screen.WorkingArea.Width - windowWidth;

            if (include && taskBarLocation.uEdge == (int) Dock.Right)
                left = left - taskBarRectangle.Width;

            left = left*96.0/dpiX;

            top = taskBarLocation.uEdge == (int) Dock.Top
                ? screen.Bounds.Y + taskBarRectangle.Height
                : screen.Bounds.Y + screen.WorkingArea.Height - windowHeight;
            if (include && taskBarLocation.uEdge == (int) Dock.Bottom)
                top = top - taskBarRectangle.Height;
            top = top*96.0/dpiY;
        }

        /// <summary>
        ///     Where is task bar located (at top of the screen, at bottom (default), or at the one of sides)
        /// </summary>
        enum Dock
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Rect
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct AppBarData
        {
            public int cbSize;
            public readonly IntPtr hWnd;
            public readonly int uCallbackMessage;
            public readonly int uEdge;
            public readonly Rect rc;
            public readonly IntPtr lParam;
        }
    }
}