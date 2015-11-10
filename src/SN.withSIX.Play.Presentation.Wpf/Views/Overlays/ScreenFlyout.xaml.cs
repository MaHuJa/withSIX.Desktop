// <copyright company="SIX Networks GmbH" file="ScreenFlyout.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Overlays;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Overlays
{
    public interface IScreenFlyout : IViewFor<OverlayViewModelBase> {}

    /// <summary>
    ///     Interaction logic for ScreenFlyout.xaml
    /// </summary>
    public partial class ScreenFlyout : Flyout, IScreenFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (OverlayViewModelBase), typeof (ScreenFlyout),
                new PropertyMetadata(null));

        public ScreenFlyout() {
            InitializeComponent();

            // Because we are using ViewFirst in this case, there is a Cycle occuring because the View is already part of the VisualTree perhaps
            // We workaround this when we use the View, by setting the DataContext to null in the Parent View...
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public OverlayViewModelBase ViewModel
        {
            get { return (OverlayViewModelBase) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (OverlayViewModelBase) value; }
        }
    }
}