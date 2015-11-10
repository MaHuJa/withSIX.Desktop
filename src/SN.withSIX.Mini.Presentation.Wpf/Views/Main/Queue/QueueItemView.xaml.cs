// <copyright company="SIX Networks GmbH" file="QueueItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Queue;
using SN.withSIX.Mini.Applications.Views.Main.Queue;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Queue
{
    /// <summary>
    ///     Interaction logic for QueueItemView.xaml
    /// </summary>
    public partial class QueueItemView : UserControl, IQueueItemView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IQueueItemViewModel), typeof (QueueItemView),
                new PropertyMetadata(null));

        public QueueItemView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.State, v => v.State.Visibility,
                    state => state != QueueItemState.Downloading ? Visibility.Visible : Visibility.Collapsed));
                d(this.OneWayBind(ViewModel, vm => vm.Progress, v => v.ProgressBar.Value));
                d(this.OneWayBind(ViewModel, vm => vm.Progress, v => v.ProgressText.Text, Converters.Progress));
                d(this.OneWayBind(ViewModel, vm => vm.State, v => v.DownloadGrid.Visibility,
                    state => state == QueueItemState.Downloading ? Visibility.Visible : Visibility.Collapsed));
                d(this.OneWayBind(ViewModel, vm => vm.State, v => v.State.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Speed, v => v.ToolTip, Converters.Speed));
                d(this.OneWayBind(ViewModel, vm => vm.Size, v => v.Size.Text, Converters.Size));
                d(this.BindCommand(ViewModel, vm => vm.Pause, v => v.Pause));
                d(this.BindCommand(ViewModel, vm => vm.Abort, v => v.Abort));
            });
        }

        public IQueueItemViewModel ViewModel
        {
            get { return (IQueueItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IQueueItemViewModel) value; }
        }
    }
}