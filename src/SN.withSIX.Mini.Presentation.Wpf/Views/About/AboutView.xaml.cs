// <copyright company="SIX Networks GmbH" file="AboutView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.About;
using SN.withSIX.Mini.Applications.Views.About;
using SN.withSIX.Mini.Presentation.Wpf.Extensions;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.About
{
    /// <summary>
    ///     Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : MetroWindow, IAboutView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IAboutViewModel), typeof (AboutView),
                new PropertyMetadata(null));

        public AboutView() {
            InitializeComponent();

            this.WhenActivated(d => {
                this.SetupScreen<IAboutViewModel>(d);
                d(this.OneWayBind(ViewModel, vm => vm.DisplayName, v => v.Title));
                d(this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionText.Text));
                d(this.BindCommand(ViewModel, vm => vm.ViewLicense, v => v.LicenseLink));
            });
        }

        public IAboutViewModel ViewModel
        {
            get { return (IAboutViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IAboutViewModel) value; }
        }
    }
}