// <copyright company="SIX Networks GmbH" file="CustomRepoAvailabilityWarningView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;
using SN.withSIX.Play.Applications.Views.Games.Dialogs;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Dialogs
{
    /// <summary>
    ///     Interaction logic for CollectionCreatedDialog.xaml
    /// </summary>
    [DoNotObfuscate]
    public partial class CustomRepoAvailabilityWarningView : StandardDialog, ICustomRepoAvailabilityWarningView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ICustomRepoAvailabilityWarningViewModel),
                typeof (CustomRepoAvailabilityWarningView),
                new PropertyMetadata(null));

        public CustomRepoAvailabilityWarningView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public ICustomRepoAvailabilityWarningViewModel ViewModel
        {
            get { return (ICustomRepoAvailabilityWarningViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ICustomRepoAvailabilityWarningViewModel) value; }
        }
    }
}