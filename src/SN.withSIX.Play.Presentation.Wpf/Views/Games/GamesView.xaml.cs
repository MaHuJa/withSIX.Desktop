// <copyright company="SIX Networks GmbH" file="GamesView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.Views.Games;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games
{
    [DoNotObfuscate]
    public partial class GamesView : UserControl, IGamesView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (GamesViewModel), typeof (GamesView),
                new PropertyMetadata(null));

        public GamesView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.View, v => v.LB.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedItem, v => v.LB.SelectedItem));
                d(this.WhenAnyValue(x => x.LB.SelectedItems).Cast<ObservableCollection<object>>()
                    .BindTo(ViewModel, vm => vm.SelectedItemsInternal));
            });
        }

        public GamesViewModel ViewModel
        {
            get { return (GamesViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as GamesViewModel; }
        }
    }
}