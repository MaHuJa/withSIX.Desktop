// <copyright company="SIX Networks GmbH" file="ConsoleProgress.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Presentation.Console.Services
{
    public class ConsoleProgress : IDisposable
    {
        static bool _cannotDetermineWidth;
        readonly IDisposable _disposable;
        readonly StatusRepo _status;
        RepoStatus _action;
        bool _disposed;

        public ConsoleProgress(StatusRepo status) {
            _status = status;
            _action = status.Action;
            _disposable = SetupProgressReporting();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        IDisposable SetupProgressReporting() {
            var observable = _status.WhenAnyValue(x => x.Info);
            var observable2 = _status.WhenAnyValue(x => x.Action);
            _action = _status.Action;
            return new CompositeDisposable(observable2.Skip(1).Subscribe(ResetProgress),
                observable.Subscribe(x => WriteProgress(x.Action, x.Progress, x.Speed)), new RepoWatcher(_status));
        }

        void WriteProgress(RepoStatus action, double progress, long speed) {
            _action = action;
            var line = action + ":" + GetProgressComponent(progress) + GetSpeedComponent(speed)
                       + GetItemComponent(_status);
            System.Console.Write(FillWithSpace("\r" + line));
        }

        static string FillWithSpace(string line, int? lineLength = null) {
            var spaceCount = lineLength.GetValueOrDefault(GetWindowWidth()) - line.Length;
            return spaceCount > 0 ? line + new string(' ', spaceCount) : line;
        }

        // Bad but cool
        static string GetItemComponent(StatusRepo statusRepo) {
            return statusRepo == null || statusRepo.Total == 0
                ? ""
                : " " + statusRepo.Info.Done + "/" + statusRepo.Total + GetActiveComponent(statusRepo);
        }

        static string GetActiveComponent(StatusRepo statusRepo) {
            return statusRepo.Info.Active > 0 ? " (" + statusRepo.Info.Active + ")" : "";
        }

        void ResetProgress(RepoStatus newAction) {
            System.Console.Write(FillWithSpaceNl(string.Format("\r{0}: 100%", _action)));
        }

        static string FillWithSpaceNl(string format) {
            return FillWithSpace(format, GetWindowWidth() - 1) + "\n";
        }

        static int GetWindowWidth() {
            const int windowWidth = 0; // 80

            if (_cannotDetermineWidth)
                return windowWidth;

            try {
                return System.Console.WindowWidth;
            } catch (IOException) {
                _cannotDetermineWidth = true;
                return windowWidth;
            }
        }

        static string GetProgressComponent(double progress) {
            return " " + progress + "%";
        }

        static string GetSpeedComponent(long speed) {
            return speed > 0
                ? " " + GetSpeed(speed)
                : "";
        }

        static string GetSpeed(long speed) {
            return Tools.FileUtil.GetFileSize(speed) + "/s";
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;

            if (disposing)
                _disposable.Dispose();

            _disposed = true;
        }
    }
}