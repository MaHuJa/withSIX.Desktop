// <copyright company="SIX Networks GmbH" file="CollectionLibraryItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for CollectionLibraryItemView.xaml
    /// </summary>
    public partial class CollectionLibraryItemView : UserControl, IViewFor<CollectionLibraryItemViewModel>,
        IViewFor<CustomCollectionLibraryItemViewModel>, IViewFor<SubscribedCollectionLibraryItemViewModel>,
        IViewFor<CustomRepoCollectionLibraryItemViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (CollectionLibraryItemViewModel),
                typeof (CollectionLibraryItemView),
                new PropertyMetadata(null));

        public CollectionLibraryItemView() {
            InitializeComponent();
        }

        public CollectionLibraryItemViewModel ViewModel
        {
            get { return (CollectionLibraryItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CollectionLibraryItemViewModel) value; }
        }
        CustomCollectionLibraryItemViewModel IViewFor<CustomCollectionLibraryItemViewModel>.ViewModel
        {
            get { return (CustomCollectionLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        CustomRepoCollectionLibraryItemViewModel IViewFor<CustomRepoCollectionLibraryItemViewModel>.ViewModel
        {
            get { return (CustomRepoCollectionLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        SubscribedCollectionLibraryItemViewModel IViewFor<SubscribedCollectionLibraryItemViewModel>.ViewModel
        {
            get { return (SubscribedCollectionLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
    }
}