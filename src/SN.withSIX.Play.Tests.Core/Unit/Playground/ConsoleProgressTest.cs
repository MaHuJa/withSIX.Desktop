// <copyright company="SIX Networks GmbH" file="ConsoleProgressTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground
{
    class ConsoleProgressTest {}

    public class StatusItem : PropertyChangedBase
    {
        IStatus _state;

        public IStatus State {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
    }

    public interface IStatusItem
    {
        IStatus State { get; set; }
    }

    public abstract class ConsoleProgress2 : IDisposable
    {
        readonly IDisposable _disposable;

        protected ConsoleProgress2() {
            _disposable = SetupProgressReporting();
        }

        public void Dispose() {
            Dispose(true);
        }

        protected abstract IDisposable SetupProgressReporting();

        protected static string GetProgressComponent(double progress) {
            return " " + progress + "%";
        }

        protected static string GetSpeedComponent(long speed) {
            return speed > 0
                ? " " + GetSpeed(speed)
                : "               ";
        }

        static string GetSpeed(long speed) {
            return Tools.FileUtil.GetFileSize(speed) + "/s";
        }

        protected static void ResetP(RepoStatus action) {
            Console.Write("\r{0}: 100%                                                                       \n", action);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                _disposable.Dispose();
        }
    }

    public class ConsoleTransferProgress : ConsoleProgress2
    {
        readonly IStatusItem _status;
        RepoStatus _action;

        public ConsoleTransferProgress(IStatusItem status) {
            _status = status;
            _action = status.State.Action;
        }

        protected void ResetProgress(RepoStatus newAction) {
            ResetP(_action);
        }

        protected override IDisposable SetupProgressReporting() {
            var observable = _status.WhenAnyValue(x => x.State);
            var observable2 = _status.WhenAnyValue(x => x.State.Action);
            return new CompositeDisposable(observable2.Skip(1).Subscribe(ResetProgress),
                observable.Subscribe(WriteProgress));
        }

        void WriteProgress(IStatus status) {
            _action = status.Action;
            Console.Write("\r" + status.Action + ":" + GetProgressComponent(status.Progress) +
                          GetSpeedComponent(status.Speed)
                          + "                    ");
        }
    }

    /*
    public class ConsoleQueueProgress : ConsoleProgress2
    {
        readonly StatusRepo _statusRepo;
        RepoStatus _action;

        public ConsoleQueueProgress(StatusRepo status) {
            _statusRepo = status;
        }

        protected void ResetProgress(RepoStatus newAction) {
            ResetP(_action);
        }

        protected override IDisposable SetupProgressReporting() {
            var stateObservable = _statusRepo.WhenAnyValue(x => x.State);
            var actionObservable = _statusRepo.WhenAnyValue(x => x.State.Action);
            return new CompositeDisposable(actionObservable.Skip(1).Subscribe(ResetProgress),
                stateObservable.Subscribe(WriteProgress));
        }

        void WriteProgress(StatusRepo status) {
            _action = status.Action;
            ConsoleEx.WriteR(status.Action + ":" + GetProgressComponent(status.Progress) +
                             GetSpeedComponent(status.Speed) + GetQueueComponent(status)
                             + "                    ");
        }

        static string GetQueueComponent(StatusRepo status) {
            return " " + status.Done + "/" + status.Total + " (" + status.Active + ")";
        }
    }
     */
}