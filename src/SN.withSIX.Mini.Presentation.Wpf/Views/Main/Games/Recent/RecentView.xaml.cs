// <copyright company="SIX Networks GmbH" file="RecentView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;
using SN.withSIX.Mini.Applications.Views.Main.Games.Recent;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games.Recent
{
    /// <summary>
    ///     Interaction logic for RecentView.xaml
    /// </summary>
    public partial class RecentView : UserControl, IRecentView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IRecentViewModel), typeof (RecentView),
                new PropertyMetadata(null));

        public RecentView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));

                d(this.OneWayBind(ViewModel, vm => vm.RecentItems, v => v.RecentList.ItemsSource));

                d(this.OneWayBind(ViewModel, x => x.RecentItems.Count, v => v.RecentList.Visibility,
                    Converters.VisibilityNormal));
                d(this.OneWayBind(ViewModel, x => x.RecentItems.Count, v => v.AddSomeContent.Visibility,
                    Converters.ReverseVisibility));

                d(this.BindCommand(ViewModel, vm => vm.AddContent, v => v.AddSomeContent));
            });
        }

        public IRecentViewModel ViewModel
        {
            get { return (IRecentViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRecentViewModel) value; }
        }
    }
}