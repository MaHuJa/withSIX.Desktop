// <copyright company="SIX Networks GmbH" file="GameView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Applications.Views.Main.Games;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games
{
    /// <summary>
    ///     Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl, IGameView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IGameViewModel), typeof (GameView),
                new PropertyMetadata(null));

        public GameView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Tabs.SelectedItem, v => v.ListAreaTabs.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.Tabs.Items, v => v.ListAreaTabs.ItemsSource));
                d(
                    ViewModel.WhenAnyValue(x => x.FirstTimeRunInfo, x => x.FirstTimeRunShown, (s, b) => s != null && !b)
                        .BindTo(this, v => v.FirstTimeGrid.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.FirstTimeRunInfo, v => v.FirstTimeText.Text));
                d(this.BindCommand(ViewModel, vm => vm.FirstTimeClose, v => v.FirstTimeCloseButton));
            });
        }

        public IGameViewModel ViewModel
        {
            get { return (IGameViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IGameViewModel) value; }
        }
    }
}