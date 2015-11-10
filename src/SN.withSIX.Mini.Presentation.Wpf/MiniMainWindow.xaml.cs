// <copyright company="SIX Networks GmbH" file="MiniMainWindow.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels;
using SN.withSIX.Mini.Applications.ViewModels.Main;
using SN.withSIX.Mini.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Presentation.Wpf.Services;
using SN.withSIX.Mini.Presentation.Wpf.Views;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MiniMainWindow : MetroWindow, IViewFor<IMiniMainWindowViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IMiniMainWindowViewModel), typeof (MiniMainWindow),
                new PropertyMetadata(null));

        public MiniMainWindow() {
            InitializeComponent();
            if (!Consts.FirstRun)
                Visibility = Visibility.Hidden;
            Closing += OnClosing;

            var img = new BitmapImage(new Uri("pack://application:,,,/Sync;component/app.ico"));
            var busyimg = new BitmapImage(new Uri("pack://application:,,,/Sync;component/app-busy.ico"));

            this.WhenActivated(d => {
                this.SetupScreen<IMiniMainWindowViewModel>(d, true);
                d(UserError.RegisterHandler<CanceledUserError>(x => Observable.Return(RecoveryOptionResult.CancelOperation)));
                d(ViewModel.OpenPopup
                    .ObserveOnMainThread()
                    .Subscribe(x => ShowAndActivate()));
                d(this.OneWayBind(ViewModel, vm => vm.TaskbarToolTip, v => v.tbInfo.Description));
                d(this.OneWayBind(ViewModel, vm => vm.TrayViewModel.Status.Status.Progress, v => v.tbInfo.ProgressValue,
                    d1 => d1/100.0));
                d(this.OneWayBind(ViewModel, vm => vm.TrayViewModel.Status.Status, v => v.tbInfo.ProgressState,
                    ToProgressState));
                d(ViewModel.WhenAnyObservable(x => x.ShowNotification).Subscribe(Notify));
                d(this.OneWayBind(ViewModel, vm => vm.DisplayName, v => v.Title));
                d(this.OneWayBind(ViewModel, vm => vm.TrayViewModel, v => v.ViewModelHost.ViewModel));
                //d(this.Bind(ViewModel, vm => vm.TrayViewModel, v => v.TrayMainWindow.ViewModel));
                d(this.OneWayBind(ViewModel, vm => vm.TaskbarToolTip, v => v.TaskbarIcon.ToolTipText));
                d(this.OneWayBind(ViewModel, vm => vm.TrayViewModel.Status.Status.Acting, v => v.TaskbarIcon.IconSource,
                    b => b ? busyimg : img));
                d(Cheat.MessageBus.Listen<ScreenOpened>().InvokeCommand(ViewModel, vm => vm.Deactivate));
                d(this.Events().Deactivated.InvokeCommand(ViewModel, vm => vm.Deactivate));
            });
            TaskbarIcon.TrayLeftMouseUp += (sender, args) => ViewModel.OpenPopup.Execute(null);
            TaskbarIcon.TrayRightMouseUp += (sender, args) => ViewModel.OpenPopup.Execute(null);
            TaskbarIcon.TrayMiddleMouseUp += (sender, args) => ViewModel.OpenPopup.Execute(null);
            //TaskbarIcon.PopupActivation = PopupActivationMode.All;
        }

        public IMiniMainWindowViewModel ViewModel
        {
            get { return (IMiniMainWindowViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IMiniMainWindowViewModel) value; }
        }

        void ShowAndActivate() {
            Show();
            Activate();
        }

        static TaskbarItemProgressState ToProgressState(StatusModel b) {
            if (b == null || !b.Acting)
                return TaskbarItemProgressState.None;
            switch (b.State) {
            case State.Error:
                return TaskbarItemProgressState.Error;
            case State.Paused:
                return TaskbarItemProgressState.Paused;
            default:
                return TaskbarItemProgressState.Normal;
            }
        }

        void Notify(ITrayNotificationViewModel notification) {
            Dispatcher.InvokeAsync(
                () => TaskbarIcon.ShowCustomBalloon(
                    new TrayNotification {
                        ViewModel = notification
                    },
                    PopupAnimation.Fade,
                    notification.CloseIn == null ? null : (int?) notification.CloseIn.Value.TotalMilliseconds));
            // TODO: configurable delay etc
        }

        void OnClosing(object sender, CancelEventArgs e) {
            if (Cheat.IsShuttingDown) {
                ViewModel.IsOpen = false;
                return;
            }
            e.Cancel = true;
            Hide();
        }
    }
}