// <copyright company="SIX Networks GmbH" file="SoftwareUpdateSquirrelOverlayView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Applications.Views.Overlays;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Overlays
{
    /// <summary>
    ///     Interaction logic for SoftwareUpdateSquirrelOverlayView.xaml
    /// </summary>
    public partial class SoftwareUpdateSquirrelOverlayView : UserControl, ISoftwareUpdateSquirrelOverlayView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ISoftwareUpdateSquirrelOverlayViewModel),
                typeof (SoftwareUpdateSquirrelOverlayView),
                new PropertyMetadata(null));

        public SoftwareUpdateSquirrelOverlayView() {
            InitializeComponent();

            webControl.RegisterJsObject("six_client", new DummyWc());

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.BindCommand(ViewModel, vm => vm.RestartCommand, v => v.RestartButton));
                d(this.OneWayBind(ViewModel, vm => vm.NewVersionInstalled, v => v.RestartButton.Visibility));
                d(this.BindCommand(ViewModel, vm => vm.ApplyUpdateCommand, v => v.InstallButton));
                d(this.OneWayBind(ViewModel, vm => vm.NewVersionAvailable, v => v.InstallButton.Visibility));
            });
        }

        public ISoftwareUpdateSquirrelOverlayViewModel ViewModel
        {
            get { return (ISoftwareUpdateSquirrelOverlayViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ISoftwareUpdateSquirrelOverlayViewModel) value; }
        }
    }
}