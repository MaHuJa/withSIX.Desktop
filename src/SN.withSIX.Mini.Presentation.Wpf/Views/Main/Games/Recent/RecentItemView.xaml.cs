// <copyright company="SIX Networks GmbH" file="RecentItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;
using SN.withSIX.Mini.Applications.Views.Main.Games.Recent;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games.Recent
{
    /// <summary>
    ///     Interaction logic for RecentItemView.xaml
    /// </summary>
    public partial class RecentItemView : UserControl, IRecentItemView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IRecentItemViewModel), typeof (RecentItemView),
                new PropertyMetadata(null));

        public RecentItemView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));

                d(this.BindCommand(ViewModel, vm => vm.Visit, v => v.Visit));
                d(this.BindCommand(ViewModel, vm => vm.Action, v => v.ActionButton));
                d(this.BindCommand(ViewModel, vm => vm.Abort, v => v.Abort));
                var playIsExecuting = ViewModel.WhenAnyObservable(x => x.Action.IsExecuting);
                d(this.WhenAnyValue(x => x.IsMouseOver)
                    .CombineLatest(playIsExecuting, (mo, executing) => mo && !executing)
                    .DistinctUntilChanged()
                    .BindTo(this, v => v.ActionButton.Visibility));
                d(playIsExecuting.BindTo(this, v => v.Abort.Visibility));
                d(playIsExecuting.BindTo(this, v => v.ProcessingText.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Actions.Items, v => v.ActionButton.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.Actions.SelectedItem, v => v.ActionButton.SelectedItem));
                d(ViewModel.WhenAnyObservable(x => x.Action.CanExecuteObservable)
                    .BindTo(this, v => v.ActionButton.IsEnabled));

                d(this.OneWayBind(ViewModel, vm => vm.ContentCount, v => v.ItemCountText.Text,
                    c => c + " " + "item".PluralizeIfNeeded(c)));
                d(this.OneWayBind(ViewModel, vm => vm.ContentNames, v => v.ItemCountText.ToolTip,
                    c => string.Join(", ", c)));
                d(this.OneWayBind(ViewModel, vm => vm.Image, v => v.Image.ImageUrl));
                d(this.OneWayBind(ViewModel, vm => vm.LastUsed, v => v.LastPlayedText.ToolTip));
                d(this.OneWayBind(ViewModel, vm => vm.LastUsed, v => v.LastPlayedText.Text,
                    Converters.TimeAgoConverter));
                d(this.BindCommand(ViewModel, vm => vm.SwitchFavorite, v => v.SwitchFavorite));
                d(this.OneWayBind(ViewModel, vm => vm.IsFavorite, v => v.SwitchFavorite.Content,
                    b => b ? SixIconFont.withSIX_icon_Star : SixIconFont.withSIX_icon_Star_Outline));
                d(this.WhenAnyValue(x => x.Image.IsMouseOver, x => x.SwitchFavorite.IsMouseOver,
                    (x, y) => x || y ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, v => v.SwitchFavorite.Visibility));
            });
        }

        public IRecentItemViewModel ViewModel
        {
            get { return (IRecentItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRecentItemViewModel) value; }
        }
    }
}