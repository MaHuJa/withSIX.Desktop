// <copyright company="SIX Networks GmbH" file="CreateCollectionView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Games.Popups;
using SN.withSIX.Play.Applications.Views.Games.Overlays;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Popups
{
    /// <summary>
    ///     Interaction logic for CreateCollectionView.xaml
    /// </summary>
    public partial class CreateCollectionView : PopupControl, ICreateCollectionView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ICreateCollectionViewModel), typeof (CreateCollectionView),
                new PropertyMetadata(null));

        public CreateCollectionView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.Bind(ViewModel, vm => vm.CollectionName, v => v.CollectionName.Text));
                d(this.BindCommand(ViewModel, vm => vm.CreateCollection, v => v.CreateButton));
                CollectionName.Focus();
            });
        }

        public ICreateCollectionViewModel ViewModel
        {
            get { return (ICreateCollectionViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ICreateCollectionViewModel) value; }
        }
    }
}