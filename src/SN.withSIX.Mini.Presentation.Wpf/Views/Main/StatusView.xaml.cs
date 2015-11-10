// <copyright company="SIX Networks GmbH" file="StatusView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.ViewModels.Main;
using SN.withSIX.Mini.Applications.Views.Main;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main
{
    /// <summary>
    ///     Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : UserControl, IStatusView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IStatusViewModel), typeof (StatusView),
                new PropertyMetadata(null));

        public StatusView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Icon, v => v.Icon.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Color, v => v.Icon.Foreground));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Text, v => v.Text.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Progress, v => v.Progress.Value));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Progress, v => v.Status.Text,
                    d1 => {
                        var statusModel = ViewModel.Status;
                        return statusModel.Acting
                            ? statusModel.ToProgressText()
                            : string.Empty;
                    }));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Acting, v => v.Progress.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Status.Acting, v => v.Abort.Visibility));
                d(this.BindCommand(ViewModel, vm => vm.Abort, v => v.Abort));
                d(this.BindCommand(ViewModel, vm => vm.SwitchQueue, v => v.Switch));
                if (!Consts.Features.Queue)
                    Switch.Visibility = Visibility.Collapsed;
            });
        }

        public IStatusViewModel ViewModel
        {
            get { return (IStatusViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IStatusViewModel) value; }
        }
    }
}