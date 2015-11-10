// <copyright company="SIX Networks GmbH" file="ScreenExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Behaviors;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Presentation.Wpf.Extensions
{
    public static class ScreenExtensions
    {
        public static void SetupScreen<T>(this Window window, Action<IDisposable> d) where T : class, IScreenViewModel {
            ScreenExtensions<T>.SetupScreen(window, d);
        }

        public static void SetupScreen<T>(this Window window, Action<IDisposable> d, bool taskbarWindow = false)
            where T : class, IScreenViewModel {
            ScreenExtensions<T>.SetupScreen(window, d, taskbarWindow);
        }
    }

    public static class ScreenExtensions<T2> where T2 : class, IScreenViewModel
    {
        public static void SetupScreen(Window window, Action<IDisposable> d, bool taskbarWindow = false) {
            d(BasicUserErrorHandler.RegisterDefaultHandler(window));
            var target = (IViewFor<T2>) window;
            d(target.WhenAnyValue(x => x.ViewModel).BindTo(window, v => v.DataContext));
            d(target.WhenAnyValue(x => x.ViewModel).Where(x => x != null).Subscribe(x => {
                x.IsOpen = true;
                window.Closed += Window_Closed;
            }));

            d(window.WhenAnyValue(x => x.Visibility)
                .Where(x => x == Visibility.Visible)
                .Subscribe(x => {
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                    if (taskbarWindow)
                        window.SetWindowPos();
                }));

            d(window.Events().KeyDown.Where(x => x.Key == Key.F1).InvokeCommand(target.ViewModel.Help));

            d(target.WhenAnyObservable(x => x.ViewModel.Close)
                .Subscribe(x => window.Close()));

            d(target.WhenAnyObservable(x => x.ViewModel.Activate)
                .Subscribe(x => window.Activate()));
        }

        static void Window_Closed(object sender, EventArgs e) {
            var window = (Window) sender;
            var target = (IViewFor<T2>) window;
            target.ViewModel.IsOpen = false;
            window.Closed -= Window_Closed;
        }
    }
}