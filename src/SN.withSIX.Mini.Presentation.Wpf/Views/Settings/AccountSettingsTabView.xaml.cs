// <copyright company="SIX Networks GmbH" file="AccountSettingsTabView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Applications.Views.Settings;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Settings
{
    /// <summary>
    ///     Interaction logic for AccountSettingsTabView.xaml
    /// </summary>
    public partial class AccountSettingsTabView : UserControl, IAccountSettingsTabView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IAccountSettingsTabViewModel),
                typeof (AccountSettingsTabView),
                new PropertyMetadata(null));

        public AccountSettingsTabView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.DataContext));

                d(this.BindCommand(ViewModel, vm => vm.Login, v => v.Login));
                d(this.BindCommand(ViewModel, vm => vm.Logout, v => v.Logout));
                d(this.BindCommand(ViewModel, vm => vm.GoPremium, v => v.GoPremium));
                d(ViewModel.WhenAnyValue(x => x.LoginInfo.IsLoggedIn, x => x.LoginInfo.IsPremium,
                    (b, b1) => b && !b1)
                    .BindTo(this, x => x.GoPremium.Visibility));
                //d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsLoggedIn, v => v.GoPremium.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsPremium, v => v.GoPremiumRun.Text,
                    b => b ? "Premium user" : "Go Premium"));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsPremium, v => v.GoPremium.Foreground,
                    b => (SolidColorBrush) Application.Current.FindResource(b ? "SixOrange" : "SixBlue")));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsLoggedIn, v => v.Logout.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.IsLoggedIn, v => v.Login.Visibility,
                    Converters.ReverseVisibility));
                d(this.OneWayBind(ViewModel, vm => vm.LoginInfo.Account.UserName, v => v.UserName.Text));
            });
        }

        public IAccountSettingsTabViewModel ViewModel
        {
            get { return (IAccountSettingsTabViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IAccountSettingsTabViewModel) value; }
        }
    }
}