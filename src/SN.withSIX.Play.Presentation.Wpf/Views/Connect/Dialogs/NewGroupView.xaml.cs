// <copyright company="SIX Networks GmbH" file="NewGroupView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Connect.Dialogs;
using SN.withSIX.Play.Applications.Views.Connect.Dialogs;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Connect.Dialogs
{
    [DoNotObfuscate]
    public partial class NewGroupView : StandardDialog, INewGroupView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (INewGroupViewModel), typeof (NewGroupView),
                new PropertyMetadata(null));

        public NewGroupView() {
            InitializeComponent();
            Name.Focus();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as INewGroupViewModel; }
        }
        public INewGroupViewModel ViewModel
        {
            get { return (INewGroupViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}