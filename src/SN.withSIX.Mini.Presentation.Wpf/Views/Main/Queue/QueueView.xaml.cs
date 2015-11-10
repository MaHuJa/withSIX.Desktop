// <copyright company="SIX Networks GmbH" file="QueueView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Main.Queue;
using SN.withSIX.Mini.Applications.Views.Main.Queue;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Queue
{
    /// <summary>
    ///     Interaction logic for QueueView.xaml
    /// </summary>
    public partial class QueueView : UserControl, IQueueView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IQueueViewModel), typeof (QueueView),
                new PropertyMetadata(null));

        public QueueView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.DisplayName, v => v.Title.Text));
                d(this.OneWayBind(ViewModel, vm => vm.QueueItems, v => v.Items.ItemsSource));
                d(this.BindCommand(ViewModel, vm => vm.ClearCompleted, v => v.ClearCompleted));
                d(this.BindCommand(ViewModel, vm => vm.PauseAll, v => v.PauseAll));
            });
        }

        public IQueueViewModel ViewModel
        {
            get { return (IQueueViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IQueueViewModel) value; }
        }
    }
}