// <copyright company="SIX Networks GmbH" file="EnterUserNamePasswordView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs;
using SN.withSIX.Core.Applications.MVVM.Views.Dialogs;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Dialogs
{
    [DoNotObfuscate]
    public partial class EnterUserNamePasswordView : StandardDialog, IEnterUserNamePasswordView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IEnterUserNamePasswordViewModel),
                typeof (EnterUserNamePasswordView),
                new PropertyMetadata(null));

        public EnterUserNamePasswordView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as IEnterUserNamePasswordViewModel; }
        }
        public IEnterUserNamePasswordViewModel ViewModel
        {
            get { return (IEnterUserNamePasswordViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}