// <copyright company="SIX Networks GmbH" file="FavoriteItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Favorite;
using SN.withSIX.Mini.Applications.Views.Main.Games.Favorite;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games.Favorite
{
    /// <summary>
    ///     Interaction logic for FavoriteItemView.xaml
    /// </summary>
    public partial class FavoriteItemView : UserControl, IFavoriteItemView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IFavoriteItemViewModel), typeof (FavoriteItemView),
                new PropertyMetadata(null));

        public FavoriteItemView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Image, v => v.Image.ImageUrl));
                d(this.OneWayBind(ViewModel, vm => vm.Actions.Items, v => v.ActionButton.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.Actions.SelectedItem, v => v.ActionButton.SelectedItem));
                d(this.BindCommand(ViewModel, vm => vm.Unfavorite, v => v.Unfavorite));
                d(this.BindCommand(ViewModel, vm => vm.Action, v => v.ActionButton));
                d(this.BindCommand(ViewModel, vm => vm.Visit, v => v.Visit));
                d(ViewModel.WhenAnyObservable(x => x.Action.CanExecuteObservable).BindTo(this, v => v.ActionButton.IsEnabled));
                d(this.WhenAnyValue(x => x.Image.IsMouseOver, x => x.Unfavorite.IsMouseOver, Converters.OrVisibility)
                    .BindTo(this, v => v.Unfavorite.Visibility));

                // TODO: Abort
                d(this.WhenAnyValue(x => x.IsMouseOver)
                    .CombineLatest(ViewModel.WhenAnyObservable(x => x.Action.IsExecuting), (mo, executing) => mo && !executing)
                    .DistinctUntilChanged()
                    .BindTo(this, v => v.ActionButton.Visibility));
            });
        }

        public IFavoriteItemViewModel ViewModel
        {
            get { return (IFavoriteItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IFavoriteItemViewModel) value; }
        }
    }
}