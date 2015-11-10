// <copyright company="SIX Networks GmbH" file="LoginView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Login;
using SN.withSIX.Mini.Applications.Views.Login;
using SN.withSIX.Mini.Presentation.Wpf.Extensions;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Login
{
    /// <summary>
    ///     Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : MetroWindow, ILoginView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ILoginViewModel), typeof (LoginView),
                new PropertyMetadata(null));

        public LoginView() {
            InitializeComponent();
            var ver = new WebBrowser().Version;
            if (ver.Major < 9) {
                Warning.Text =
                    "WARN: Your Windows / Internet Explorer is out of date, please install latest Windows Updates, including latest Internet Explorer";
            } else
                Warning.Visibility = Visibility.Collapsed;

            this.WhenActivated(d => {
                this.SetupScreen<ILoginViewModel>(d);
                //d(Observable.FromEventPattern<NavigatingCancelEventHandler, NavigationEventArgs>
                //  (x => Browser.Navigating += x, x => Browser.Navigating -= x)
                //.Subscribe(x => ViewModel.Nav.Execute(x.EventArgs.Uri)));
                Browser.Navigating += BrowserOnNavigating;
                d(this.Bind(ViewModel, vm => vm.Uri, v => v.Browser.Source));
            });
        }

        public ILoginViewModel ViewModel
        {
            get { return (ILoginViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILoginViewModel) value; }
        }

        void BrowserOnNavigating(object sender, NavigatingCancelEventArgs navigatingCancelEventArgs) {
            var currentUrl = Browser.Source;
            navigatingCancelEventArgs.Cancel = ViewModel.Navigating(navigatingCancelEventArgs.Uri);
        }
    }
}