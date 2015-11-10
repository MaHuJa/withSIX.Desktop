// <copyright company="SIX Networks GmbH" file="TrayNotification.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels;
using SN.withSIX.Mini.Applications.Views;
using SN.withSIX.Mini.Presentation.Wpf.Controls;

namespace SN.withSIX.Mini.Presentation.Wpf.Views
{
    /// <summary>
    ///     Interaction logic for TrayNotification.xaml
    /// </summary>
    public partial class TrayNotification : TrayNotificationControl, ITrayNotification
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ITrayNotificationViewModel), typeof (TrayNotification),
                new PropertyMetadata(null));
        bool _isClosing;

        public TrayNotification() {
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Actions, v => v.Actions.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.Title, v => v.Title.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Text, v => v.Text.Text));
                d(ViewModel.WhenAnyObservable(x => x.Close)
                    .Subscribe(x => Close()));
            });
        }

        public ITrayNotificationViewModel ViewModel
        {
            get { return (ITrayNotificationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ITrayNotificationViewModel) value; }
        }

        /// <summary>
        ///     By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent" />
        ///     and setting the "Handled" property to true, we suppress the popup
        ///     from being closed in order to display the custom fade-out animation.
        /// </summary>
        void OnBalloonClosing(object sender, RoutedEventArgs e) {
            e.Handled = true; //suppresses the popup from being closed immediately
            _isClosing = true;
        }

        /// <summary>
        ///     Resolves the <see cref="TaskbarIcon" /> that displayed
        ///     the balloon and requests a close action.
        /// </summary>
        void imgClose_MouseDown(object sender, MouseButtonEventArgs e) {
            Close();
        }

        void Close() {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        /// <summary>
        ///     If the users hovers over the balloon, we don't close it.
        /// </summary>
        void grid_MouseEnter(object sender, MouseEventArgs e) {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (_isClosing)
                return;

            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }

        /// <summary>
        ///     Closes the popup once the fade-out animation completed.
        ///     The animation was triggered in XAML through the attached
        ///     BalloonClosing event.
        /// </summary>
        void OnFadeOutCompleted(object sender, EventArgs e) {
            Popup pp = (Popup) Parent;
            pp.IsOpen = false;
        }
    }
}