// <copyright company="SIX Networks GmbH" file="UserErrorView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Data;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Dialogs
{
    [DoNotObfuscate]
    public partial class UserErrorView : StandardDialog, IViewFor<UserErrorViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (UserErrorViewModel), typeof (UserErrorView),
                new PropertyMetadata(null));

        public UserErrorView() {
            InitializeComponent();
            BindingOperations.ClearBinding(this, TitleProperty);

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.UserError.ErrorMessage, v => v.Title));
                d(this.OneWayBind(ViewModel, vm => vm.UserError.ErrorCauseOrResolution, v => v.Message.Text));
                d(this.OneWayBind(ViewModel, vm => vm.UserError.RecoveryOptions, v => v.Buttons.ItemsSource));
            });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as UserErrorViewModel; }
        }
        public UserErrorViewModel ViewModel
        {
            get { return (UserErrorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}