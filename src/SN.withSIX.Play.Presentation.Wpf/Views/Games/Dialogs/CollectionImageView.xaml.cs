// <copyright company="SIX Networks GmbH" file="CollectionImageView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;
using SN.withSIX.Play.Applications.Views.Games.Dialogs;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Dialogs
{
    /// <summary>
    ///     Interaction logic for CollectionImageView.xaml
    /// </summary>
    public partial class CollectionImageView : StandardDialog, ICollectionImageView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ICollectionImageViewModel), typeof (CollectionImageView),
                new PropertyMetadata(null));

        public CollectionImageView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ICollectionImageViewModel; }
        }
        public ICollectionImageViewModel ViewModel
        {
            get { return (ICollectionImageViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}