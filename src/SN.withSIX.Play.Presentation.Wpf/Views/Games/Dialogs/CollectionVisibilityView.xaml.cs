// <copyright company="SIX Networks GmbH" file="CollectionVisibilityView.xaml.cs">
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
    ///     Interaction logic for CollectionVisibilityView.xaml
    /// </summary>
    [DoNotObfuscate]
    public partial class CollectionVisibilityView : StandardDialog, ICollectionVisibilityView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ICollectionVisibilityViewModel),
                typeof (CollectionVisibilityView),
                new PropertyMetadata(null));

        public CollectionVisibilityView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ICollectionVisibilityViewModel; }
        }
        public ICollectionVisibilityViewModel ViewModel
        {
            get { return (ICollectionVisibilityViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}