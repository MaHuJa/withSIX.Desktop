// <copyright company="SIX Networks GmbH" file="FavoriteView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Favorite;
using SN.withSIX.Mini.Applications.Views.Main.Games.Favorite;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games.Favorite
{
    /// <summary>
    ///     Interaction logic for FavoriteView.xaml
    /// </summary>
    public partial class FavoriteView : UserControl, IFavoriteView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IFavoriteViewModel), typeof (FavoriteView),
                new PropertyMetadata(null));

        public FavoriteView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.FavoriteItems, v => v.FavoriteItems.ItemsSource));
                d(this.OneWayBind(ViewModel, x => x.FavoriteItems.Count, v => v.AddFavoritesText.Visibility,
                    Converters.ReverseVisibility));
            });
        }

        public IFavoriteViewModel ViewModel
        {
            get { return (IFavoriteViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IFavoriteViewModel) value; }
        }
    }
}