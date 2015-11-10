// <copyright company="SIX Networks GmbH" file="InstalledItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Applications;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Installed;
using SN.withSIX.Mini.Applications.Views.Main.Games.Installed;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games.Installed
{
    /// <summary>
    ///     Interaction logic for LocalItemView.xaml
    /// </summary>
    public partial class InstalledItemView : UserControl, IInstalledItemView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IInstalledItemViewModel), typeof (InstalledItemView),
                new PropertyMetadata(null));

        public InstalledItemView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.WhenAnyValue(x => x.IsMouseOver, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, v => v.ActionButton.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Image, v => v.Image.ImageUrl));
                d(this.BindCommand(ViewModel, vm => vm.Visit, v => v.Visit));
                d(this.BindCommand(ViewModel, vm => vm.Action, v => v.ActionButton));
                d(ViewModel.WhenAnyObservable(x => x.Action.CanExecuteObservable)
                    .BindTo(this, v => v.ActionButton.IsEnabled));
                d(this.OneWayBind(ViewModel, vm => vm.Actions.Items, v => v.ActionButton.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.Actions.SelectedItem, v => v.ActionButton.SelectedItem));
                d(this.Bind(ViewModel, vm => vm.IsEnabled, v => v.IsEnabled.IsChecked));
                d(this.BindCommand(ViewModel, vm => vm.SwitchFavorite, v => v.SwitchFavorite));
                d(this.OneWayBind(ViewModel, vm => vm.IsFavorite, v => v.SwitchFavorite.Content,
                    b => b ? SixIconFont.withSIX_icon_Star : SixIconFont.withSIX_icon_Star_Outline));
                d(this.WhenAnyValue(x => x.Image.IsMouseOver, x => x.SwitchFavorite.IsMouseOver,
                    (x, y) => x || y ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, v => v.SwitchFavorite.Visibility));
            });
        }

        public IInstalledItemViewModel ViewModel
        {
            get { return (IInstalledItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IInstalledItemViewModel) value; }
        }
    }
}