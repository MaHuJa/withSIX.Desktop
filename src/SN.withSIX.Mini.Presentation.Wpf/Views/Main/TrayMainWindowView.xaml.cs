// <copyright company="SIX Networks GmbH" file="TrayMainWindowView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Media;
using ReactiveUI;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.ViewModels.Main;
using SN.withSIX.Mini.Applications.Views.Main;
using SN.withSIX.Mini.Presentation.Wpf.Controls;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main
{
    /// <summary>
    ///     Interaction logic for TrayMainWindow.xaml
    /// </summary>
    public partial class TrayMainWindow : TrayNotificationControl, ITrayMainWindowView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ITrayMainWindowViewModel), typeof (TrayMainWindow),
                new PropertyMetadata(null));

        public TrayMainWindow() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));

                d(this.Bind(ViewModel, vm => vm.Menu, v => v.Menu.DataContext));
                d(this.BindCommand(ViewModel, vm => vm.InstallUpdate, v => v.InstallUpdate));
                d(this.OneWayBind(ViewModel, vm => vm.UpdateState, v => v.InstallUpdate.Visibility,
                    state => state == AppUpdateState.UpdateAvailable ? Visibility.Visible : Visibility.Collapsed));
                d(this.OneWayBind(ViewModel, vm => vm.UpdateState, v => v.UpdateStatusText.Visibility,
                    state =>
                        state == AppUpdateState.UpdateInstalled || state == AppUpdateState.Updating
                            ? Visibility.Visible
                            : Visibility.Collapsed));
                d(this.OneWayBind(ViewModel, vm => vm.UpdateState, v => v.UpdateStatusText.Text,
                    state => state == AppUpdateState.UpdateInstalled ? "Update installed" : "Updating..."));
                d(this.OneWayBind(ViewModel, vm => vm.AvatarUrl, v => v.Avatar.ImageUrl));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.Account.UserName, v => v.Avatar.ToolTip));
                d(this.BindCommand(ViewModel, vm => vm.GoAccount, v => v.GoAccount));
                d(this.OneWayBind(ViewModel, vm => vm.MainArea, v => v.MainArea.ViewModel));
                d(this.OneWayBind(ViewModel, vm => vm.Status, v => v.Status.ViewModel));

                d(this.BindCommand(ViewModel, vm => vm.GoPremium, v => v.GoPremium));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsPremium, v => v.GoPremiumRun.Text,
                    b => b ? "Premium user" : "Go Premium"));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsPremium, v => v.GoPremium.Foreground,
                    b => (SolidColorBrush) Application.Current.FindResource(b ? "SixOrange" : "SixBlue")));
            });
        }

        public ITrayMainWindowViewModel ViewModel
        {
            get { return (ITrayMainWindowViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ITrayMainWindowViewModel) value; }
        }
    }
}