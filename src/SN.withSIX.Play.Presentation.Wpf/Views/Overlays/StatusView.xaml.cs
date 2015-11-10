// <copyright company="SIX Networks GmbH" file="StatusView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Converters;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Applications.Views.Overlays;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Overlays
{
    /// <summary>
    ///     Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : Flyout, IStatusView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IStatusViewModel), typeof (StatusView),
                new PropertyMetadata(null));
        public static readonly DependencyProperty StatusOutputProperty = DependencyProperty.Register("StatusOutput",
            typeof (string), typeof (StatusView), new PropertyMetadata(default(string)));

        public StatusView() {
            InitializeComponent();

            var cvs = (CollectionViewSource) FindResource("StatusSource");
            //collectionView.SortDescriptions.Add(new SortDescription("Progress", ListSortDirection.Ascending));
            cvs.LiveSortingProperties.Add("Progress");
            cvs.IsLiveSortingRequested = true;

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));

                d(this.Bind(ViewModel, vm => vm.DisplayName, v => v.Header));
                d(this.Bind(ViewModel, vm => vm.IsOpen, v => v.IsOpen));

                var observable = this.WhenAnyValue(x => x.ViewModel.Info)
                    .Select(GenerateInfo);
                d(observable.BindTo(this, v => v.StatusOutput));
                d(observable.BindTo(this, v => v.StatusOutputText.Text));

                var statusItemsObservable = this.WhenAnyValue(x => x.ViewModel.StatusItems);
                d(statusItemsObservable.Subscribe(x => {
                    if (x == null) {
                        cvs.Source = null;
                        return;
                    }
                    cvs.Source = x;
                    var view = (ListCollectionView) cvs.View;
                    view.CustomSort = ViewModel.Sort;
                }));
            });
        }

        public string StatusOutput
        {
            get { return (string) GetValue(StatusOutputProperty); }
            set { SetValue(StatusOutputProperty, value); }
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

        string GenerateInfo(StatusInfo statusInfo) {
            if (statusInfo == null)
                return "";
            return statusInfo.Action == RepoStatus.Downloading
                ? GenerateDownloadingInfo(statusInfo, ViewModel.ActiveStatusMod.Name)
                : GenerateOtherInfo(statusInfo, ViewModel.ActiveStatusMod.Name);
        }

        static string GenerateOtherInfo(StatusInfo statusInfo, string name) {
            return string.Format("Status: {0} {1} {2:#.00}%", statusInfo.Action, name, statusInfo.Progress);
        }

        static string GenerateDownloadingInfo(StatusInfo statusInfo, string name) {
            return string.Format("Status: {0} {1} {2:#.00}% @ {3}", statusInfo.Action, name, statusInfo.Progress,
                SpeedConverter.ConvertSpeed(statusInfo.Speed));
        }

        void dataGrid2_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var dg = (DataGrid) sender;
            var item = e.FindDataGridItem<IStatus>();
            if (item == null)
                return;

            dg.RowDetailsVisibilityMode = dg.RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.Collapsed
                ? DataGridRowDetailsVisibilityMode.Visible
                : DataGridRowDetailsVisibilityMode.Collapsed;
            e.Handled = true;
        }
    }
}