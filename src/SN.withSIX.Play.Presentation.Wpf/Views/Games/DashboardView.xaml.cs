// <copyright company="SIX Networks GmbH" file="DashboardView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.Views.Games;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games
{
    /// <summary>
    ///     Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl, IDashboardView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IDashboardViewModel), typeof (DashboardView),
                new PropertyMetadata(null));

        public DashboardView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.BindCommand(ViewModel, vm => vm.GoGames, v => v.GamesButton));
                d(this.BindCommand(ViewModel, vm => vm.GoDiscovery, v => v.DiscoveryButton));
            });
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as IDashboardViewModel; }
        }
        public IDashboardViewModel ViewModel
        {
            get { return (IDashboardViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}