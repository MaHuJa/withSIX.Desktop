// <copyright company="SIX Networks GmbH" file="GameItemView.xaml.cs">
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
    ///     Interaction logic for GameItemView.xaml
    /// </summary>
    public partial class GameItemView : UserControl, IGameItemView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IGameItemViewModel), typeof (GameItemView),
                new PropertyMetadata(null));

        public GameItemView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Image, v => v.Image.ImageUrl));
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.Image.ToolTip));
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameText.Text));
            });
        }

        public IGameItemViewModel ViewModel
        {
            get { return (IGameItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IGameItemViewModel) value; }
        }
    }
}