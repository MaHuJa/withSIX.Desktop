// <copyright company="SIX Networks GmbH" file="GamesView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Applications.Views.Main.Games;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games
{
    /// <summary>
    ///     Interaction logic for GamesView.xaml
    /// </summary>
    public partial class GamesView : UserControl, IGamesView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IGamesViewModel), typeof (GamesView),
                new PropertyMetadata(null));

        public GamesView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem.BackgroundImage,
                    v => v.GameBackgroundImage.ImageUrl));
                d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem.Image, v => v.SelectedGameImage.ImageUrl));
                d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem, v => v.SelectAGame.Visibility,
                    Converters.ReverseVisibility));
                d(this.OneWayBind(ViewModel, vm => vm.Games.Items.Count, v => v.AddGames.Visibility,
                    Converters.ReverseVisibility));
                d(this.OneWayBind(ViewModel, vm => vm.Games.Items.Count, v => v.Games.Visibility,
                    Converters.VisibilityNormal));

                d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem.LaunchTypes.Items,
                    v => v.LaunchButton.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.Games.SelectedItem.LaunchTypes.SelectedItem,
                    v => v.LaunchButton.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem.IsInstalled, v => v.LaunchButton.Visibility));

                d(this.OneWayBind(ViewModel, vm => vm.Games.Items, v => v.Games.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.Games.SelectedItem, v => v.Games.SelectedItem));
                d(ViewModel.WhenAnyObservable(x => x.ChangeGame.IsExecuting)
                    .Select(x => !x)
                    .BindTo(this, v => v.Games.IsEnabled));

                //d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem.IsInstalled, v => v.Game.Visibility));
                //d(this.OneWayBind(ViewModel, vm => vm.Games.SelectedItem.IsInstalled, v => v.ConfigureGameDirectory.Visibility,
                //  Converters.ReverseVisibility));
                //d(this.BindCommand(ViewModel, vm => vm.ConfigureGameDirectory, v => v.ConfigureGameDirectory));

                d(this.OneWayBind(ViewModel, vm => vm.Game, v => v.Game.ViewModel));

                d(ViewModel.WhenAnyObservable(x => x.ChangeGame.IsExecuting)
                    .BindTo(this, v => v.Loading.Visibility));
                d(ViewModel.WhenAnyObservable(x => x.ChangeGame.IsExecuting)
                    .Select(x => !x)
                    .BindTo(this, v => v.Game.Visibility));


                d(this.BindCommand(ViewModel, vm => vm.Games.SelectedItem.Launch, v => v.LaunchButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenBrowse, v => v.OpenBrowse));
                d(this.BindCommand(ViewModel, vm => vm.AddGames, v => v.AddGames));
            });
        }

        public IGamesViewModel ViewModel
        {
            get { return (IGamesViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IGamesViewModel) value; }
        }
    }
}