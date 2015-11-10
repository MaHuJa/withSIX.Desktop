// <copyright company="SIX Networks GmbH" file="LoginView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ReactiveUI;
using SN.withSIX.Play.Applications.Views.Dialogs;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl, ILoginView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (LoginViewModel), typeof (LoginView),
                new PropertyMetadata(null));

        public LoginView() {
            InitializeComponent();
            var ver = new WebBrowser().Version;
            if (ver.Major < 9) {
                Warning.Text =
                    "WARN: Your Windows / Internet Explorer is out of date, please install latest Windows Updates, including latest Internet Explorer";
            } else
                Warning.Visibility = Visibility.Collapsed;
            Browser.Navigating += BrowserOnNavigating;
            this.WhenActivated(d => { d(this.Bind(ViewModel, vm => vm.Uri, v => v.Browser.Source)); });
        }

        public LoginViewModel ViewModel
        {
            get { return (LoginViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (LoginViewModel) value; }
        }

        void BrowserOnNavigating(object sender, NavigatingCancelEventArgs navigatingCancelEventArgs) {
            var currentUrl = Browser.Source;
            navigatingCancelEventArgs.Cancel = ViewModel.Navigating(navigatingCancelEventArgs.Uri);
        }
    }
}