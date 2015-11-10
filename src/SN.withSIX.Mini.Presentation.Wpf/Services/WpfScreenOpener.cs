// <copyright company="SIX Networks GmbH" file="WpfScreenOpener.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Presentation.Wpf.Services
{
    public class WpfScreenOpener : IScreenOpener, IPresentationService
    {
        public void Open<T>(T viewModel) where T : class, IScreenViewModel {
            var resolvedView = ViewLocator.Current.ResolveView(viewModel);
            resolvedView.ViewModel = viewModel;
            var window = resolvedView as Window ?? CreateWindow<T>(resolvedView);
            window.WindowStartupLocation = window.Owner == null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner;
            window.Show();
            Cheat.MessageBus.SendMessage(new ScreenOpened(window));
        }

        public async Task OpenAsync<T>(T viewModel) where T : class, IScreenViewModel {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() => Open(viewModel)));
        }

        static MetroWindow CreateWindow<T>(IViewFor resolvedView) where T : class, IScreenViewModel {
            var window = new MetroWindow {Content = resolvedView};
            window.SetBinding(Window.TitleProperty, new Binding("DisplayName"));
            return window;
        }
    }

    public class ScreenOpened
    {
        public ScreenOpened(Window window) {
            Window = window;
        }

        public Window Window { get; }
    }
}