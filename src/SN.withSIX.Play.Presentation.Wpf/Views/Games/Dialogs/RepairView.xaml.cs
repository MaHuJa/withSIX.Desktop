// <copyright company="SIX Networks GmbH" file="RepairView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Dialogs
{
    /// <summary>
    ///     Interaction logic for RepairView.xaml
    /// </summary>
    public partial class RepairView : StandardDialog, IRepairView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IRepairViewModel),
                typeof (RepairView),
                new PropertyMetadata(null));

        public RepairView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.WhenAnyObservable(x => x.ViewModel.ProcessCommand.IsExecuting)
                    .Subscribe(x => {
                        ProgressBar.IsIndeterminate = x;
                        ProgressText.Text = x ? "Processing... please wait! This might take a while.." : "All done!";
                    }));
                d(this.BindCommand(ViewModel, vm => vm.OkCommand, v => v.OkButton));
            });
        }

        public IRepairViewModel ViewModel
        {
            get { return (IRepairViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRepairViewModel) value; }
        }
    }
}