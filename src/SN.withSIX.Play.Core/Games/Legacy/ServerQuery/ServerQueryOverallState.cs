// <copyright company="SIX Networks GmbH" file="ServerQueryOverallState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class ServerQueryOverallState : PropertyChangedBase, IServerQueryOverallState
    {
        readonly object _stateLock = new object();
        bool _active;
        volatile int _busy;
        double _maximum;
        double _progress;
        double _unProcessed;

        public ServerQueryOverallState() {
            this.WhenAnyValue(x => x.UnProcessed)
                .Select(x => x > 0)
                .DistinctUntilChanged()
                .Subscribe(x => Active = x);

            this.WhenAnyValue(x => x.Progress)
                .Subscribe(x => { UnProcessed = Maximum - x; });
        }

        public double UnProcessed
        {
            get { return _unProcessed; }
            set { SetProperty(ref _unProcessed, value); }
        }
        public int Busy
        {
            get { return _busy; }
            set { _busy = value; }
        }
        public int Count { get; set; }
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }
        public bool IsIndeterminate { get; set; }
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }
        public double Maximum
        {
            get { return _maximum; }
            set { SetProperty(ref _maximum, value); }
        }
        public int Canceled { get; set; }

        public void IncrementCancelled() {
            lock (_stateLock) {
                Canceled++;
                Progress++;
            }
        }

        public void IncrementProcessed() {
            lock (_stateLock)
                Progress++;
        }
    }
}