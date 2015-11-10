// <copyright company="SIX Networks GmbH" file="GamesSettingsTabView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Applications.Views.Settings;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Settings
{
    /// <summary>
    ///     Interaction logic for GamesSettingsTabView.xaml
    /// </summary>
    public partial class GamesSettingsTabView : UserControl, IGamesSettingsTabView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IGamesSettingsTabViewModel),
                typeof (GamesSettingsTabView),
                new PropertyMetadata(null));

        public GamesSettingsTabView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.DataContext));
                d(this.Bind(ViewModel, vm => vm.SelectedGame, v => v.Games.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.DetectedGames, v => v.Games.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.Game, v => v.Game.ViewModel));
            });
        }

        public IGamesSettingsTabViewModel ViewModel
        {
            get { return (IGamesSettingsTabViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IGamesSettingsTabViewModel) value; }
        }
    }
}