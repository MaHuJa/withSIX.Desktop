// <copyright company="SIX Networks GmbH" file="LicenseDialogView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Dialogs;
using SN.withSIX.Play.Applications.Views.Dialogs;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Dialogs
{
    [DoNotObfuscate]
    public partial class LicenseDialogView : StandardDialog, ILicenseDialogView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ILicenseDialogViewModel), typeof (LicenseDialogView),
                new PropertyMetadata(null));

        public LicenseDialogView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ILicenseDialogViewModel; }
        }
        public ILicenseDialogViewModel ViewModel
        {
            get { return (ILicenseDialogViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        [Obsolete("Click handlers not MVVM")]
        void AcceptButtonClick(object sender, RoutedEventArgs e) {
            var dc = (LicenseDialogViewModel) DataContext;
            dc.DialogResult = LicenseResult.LicensesAccepted;
            dc.Close(LicenseResult.LicensesAccepted);
        }

        [Obsolete("Click handlers not MVVM")]
        void DeclineButtonClick(object sender, RoutedEventArgs e) {
            var dc = (LicenseDialogViewModel) DataContext;
            dc.Close(LicenseResult.LicensesDeclined);
        }
    }
}