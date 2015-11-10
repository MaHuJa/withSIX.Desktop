// <copyright company="SIX Networks GmbH" file="StatusViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Applications.ViewModels.Overlays
{
    public interface IStatusViewModel : IViewModel
    {
        IReactiveDerivedList<IStatus> StatusItems { get; }
        bool IsOpen { get; set; }
        StatusMod ActiveStatusMod { get; }
        IComparer Sort { get; }
        string DisplayName { get; }
        StatusInfo Info { get; }
    }

    public class StatusViewModel : ViewModelBase, IStatusViewModel
    {
        StatusMod _activeStatusMod;
        StatusInfo _info;
        bool _isOpen;
        IReactiveDerivedList<IStatus> _statusItems;

        public StatusViewModel(IRepoActionHandler actionHandler) {
            DisplayName = "Downloads";

            var activeStatusModObservable = actionHandler
                .WhenAnyValue(x => x.ActiveStatusMod)
                .ObserveOn(RxApp.MainThreadScheduler);

            activeStatusModObservable
                .BindTo(this, x => x.ActiveStatusMod);

            activeStatusModObservable
                .Where(x => x == null)
                .Select(_ => false)
                .BindTo(this, x => x.IsOpen);

            this.WhenAnyValue(x => x.ActiveStatusMod.Repo)
                .Where(x => x == null)
                .Select(_ => false)
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.IsOpen);

            this.WhenAnyValue(x => x.ActiveStatusMod.Repo.Items)
                .Select(x => x == null ? null : x.CreateDerivedCollection(i => i, scheduler: RxApp.MainThreadScheduler))
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.StatusItems);

            this.WhenAnyValue(x => x.ActiveStatusMod.Repo.Info)
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.Info);

            Sort = new StatusModSorter();

            //SortDescriptions = new ReactiveList<SortDescription> { new SortDescription("Progress", ListSortDirection.Ascending) };
        }

        public StatusInfo Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }
        //public ReactiveList<SortDescription> SortDescriptions { get; private set; }

        public StatusMod ActiveStatusMod
        {
            get { return _activeStatusMod; }
            private set { SetProperty(ref _activeStatusMod, value); }
        }
        public IComparer Sort { get; }
        public string DisplayName { get; }
        public bool IsOpen
        {
            get { return _isOpen; }
            set { SetProperty(ref _isOpen, value); }
        }
        public IReactiveDerivedList<IStatus> StatusItems
        {
            get { return _statusItems; }
            private set { SetProperty(ref _statusItems, value); }
        }
    }

    public class StatusModSorter : IComparer
    {
        public int Compare(object x, object y) {
            var ox = (ITransferProgress) x;
            var oy = (ITransferProgress) y;

            return Compare(ox.Progress, oy.Progress);
        }

        // > 0 < 100
        // 0
        // 100
        static int Compare(double o, double p) {
            if (o.Equals(p))
                return 0;

            if (p.Equals(100)) {
                if (o.Equals(0))
                    return -1;
            }
            if (o.Equals(100)) {
                if (p.Equals(0) || p > o)
                    return 1;
            }

            if (p.Equals(0))
                return -1;

            if (o.Equals(0))
                return 1;

            if (o < p)
                return -1;

            return 1;
        }
    }
}