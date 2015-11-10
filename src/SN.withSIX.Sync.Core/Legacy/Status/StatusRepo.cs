// <copyright company="SIX Networks GmbH" file="StatusRepo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NDepend.Path;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Legacy.Status
{
    [DoNotObfuscateType]
    public class StatusRepo : ModelBase, IHaveTimestamps, IDisposable
    {
        RepoStatus _action;
        long _fileSize;
        long _fileSizeNew;
        long _fileSizeTransfered;
        StatusMod _owner;
        StatusInfo _statusInfo;
        int _total = -1;

        public StatusRepo() {
            CreatedAt = Tools.Generic.GetCurrentUtcDateTime;
            UpdatedAt = Tools.Generic.GetCurrentUtcDateTime;
            CTS = new Lazy<CancellationTokenSource>();
            Info = new StatusInfo(RepoStatus.Waiting, 0, 0, 0, 0);
        }

        public CancellationToken CancelToken
        {
            get { return CTS.Value.Token; }
        }
        public StatusInfo Info
        {
            get { return _statusInfo; }
            set { SetProperty(ref _statusInfo, value); }
        }

        public void Dispose() {
            Dispose(true);
        }

        public void UpdateTotals() {
            var items = Items.ToArrayLocked();
            var done = GetDoneCount(items);
            UpdateData(CalculateProgress(items), GetActiveItemsCount(items), done, items.Sum(x => x.Speed));
        }

        double CalculateProgress(ICollection<IStatus> items) {
            if (items.Count == 0)
                return 0;
            if (Total == -1)
                return CalculateProgressBasedOnItemProgress(items);
            if (Total < -1)
                throw new ArgumentOutOfRangeException("Total", "Less than -1");
            return PackFolder == null || Action != RepoStatus.Downloading
                ? CalculateProgressBasedOnItemProgress(items) // CalculateProgressBasedOnCount(done, Total)
                : CalculateProgressBasedOnSize(items);
        }

        static double CalculateProgressBasedOnItemProgress(ICollection<IStatus> items) {
            if (items.Count == 0)
                return 0;
            return items.Sum(x => x.Progress)/items.Count;
        }

        static int GetActiveItemsCount(IEnumerable<IStatus> items) {
            return items.Count(x => {
                var inty = (int) x.Action;
                return inty > 0 && inty < 900;
            });
        }

        public void Finish() {
            Info = Info.Finish();
        }

        public void Reset() {
            Info = new StatusInfo(Action, 0, 0, 0, 0);
        }

        void UpdateData(double progress, int? active, int done, long speed) {
            Info = new StatusInfo(Action, progress, speed, active, done);
        }

        static int GetDoneCount(IEnumerable<IStatus> items) {
            return items.Count(x => x.Completed || x.Failed);
        }

        // This does not work with rsync or casual http download, because rsync uses weird .IOJCOIJ_Q!KDp filenames, and http downloader uses .sixtmp as temporary file
        // So this is of limited use right now.
        double CalculateProgressBasedOnSize(IEnumerable<IStatus> items) {
            var totalDownloaded = ProcessStatusItems(items);
            double tmp = totalDownloaded/(float) (DownloadSize - ExistingFileSize);
            if (tmp > 1)
                tmp = 1;
            return tmp*100;
        }

        long ProcessStatusItems(IEnumerable<IStatus> items) {
            long totalDownloaded = 0;
            foreach (var status in items) {
                if (status.Action == RepoStatus.Finished)
                    totalDownloaded += status.FileSizeNew;
                else if (PackFolder != null)
                    totalDownloaded += TryGetFileLength(status);
            }
            return totalDownloaded;
        }

        long TryGetFileLength(IStatus status) {
            try {
                var location = TryGetObjectPath(status);
                if (string.IsNullOrWhiteSpace(location))
                    return 0;
                var partPath = PackFolder.GetChildFileWithName(location + ".part");
                var fullPath = PackFolder.GetChildFileWithName(location);
                var partFileExists = partPath.Exists;
                if (!partFileExists && !fullPath.Exists)
                    return 0;
                return !partFileExists
                    ? fullPath.FileInfo.Length
                    : partPath.FileInfo.Length;
            } catch (Exception) {
                // File doesn't exist, don't bother.
                return 0;
            }
        }

        static string TryGetObjectPath(IStatus status) {
            var realObject = status.RealObject;
            // Synq uses RealObject, other transfer methods have the fileName actually set as Item
            return string.IsNullOrWhiteSpace(realObject) ? status.Item : realObject.Replace("/", "\\");
        }

        protected virtual void Dispose(bool disposing) {
            var cts = CTS;
            CTS = null;
            if (cts != null && cts.IsValueCreated)
                cts.Value.Dispose();
        }

        #region StatusRepo Members

        Lazy<CancellationTokenSource> CTS;

        public StatusMod Owner
        {
            get { return _owner; }
            set
            {
                SetProperty(ref _owner, value);
                if (value != null)
                    value.Repo = this;
            }
        }

        public ReactiveList<IStatus> Items { get; } = new ReactiveList<IStatus>();

        public int Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value); }
        }

        public long FileSize
        {
            get { return _fileSize; }
            set { SetProperty(ref _fileSize, value); }
        }

        public long FileSizeNew
        {
            get { return _fileSizeNew; }
            set { SetProperty(ref _fileSizeNew, value); }
        }

        public long FileSizeTransfered
        {
            get { return _fileSizeTransfered; }
            set { SetProperty(ref _fileSizeTransfered, value); }
        }

        public bool Aborted { get; set; }

        public long DownloadSize { get; set; }

        public IAbsoluteDirectoryPath PackFolder { get; set; }

        public long ExistingFileSize { get; set; }
        public RepoStatus Action
        {
            get { return _action; }
            set { SetProperty(ref _action, value); }
        }

        public void AddItem(IStatus item) {
            lock (Items)
                Items.Add(item);
        }

        public bool Failed() {
            return Items.Any(x => x.Failed);
        }

        public void CalcFileSizes() {
            long fs = 0;
            long fsNew = 0;
            long fsT = 0;

            foreach (var item in Items.ToArrayLocked()) {
                fs += item.FileSize;
                fsNew += item.FileSizeNew;
                fsT += item.FileSizeTransfered;
            }

            FileSize = fs;
            FileSizeNew = fsNew;
            FileSizeTransfered = fsT;
        }


        public void IncrementDone() {
            Info = Info.IncrementDone();
        }

        public void Restart() {
            UpdateProgress(0);
        }

        public void UpdateProgress(double progress) {
            Info = Info.UpdateProgress(progress);
        }

        public void Reset(RepoStatus status, int total) {
            lock (Items)
                Items.Clear();
            ResetWithoutClearItems(status, total);
        }

        public void ResetWithoutClearItems(RepoStatus status, int total) {
            Total = total;
            Action = status;
            UpdateData(0, 0, 0, 0);
        }

        public void ProcessSize(IEnumerable<string> unchanged, IAbsoluteDirectoryPath packFolder, long downloadSize) {
            DownloadSize = downloadSize;
            PackFolder = packFolder;
            ExistingFileSize = GetExistingPackFiles(unchanged).Sum(s => s.FileInfo.Length);
        }

        IEnumerable<IAbsoluteFilePath> GetExistingPackFiles(IEnumerable<string> unchanged) {
            return unchanged.Select(x => PackFolder.GetChildFileWithName(x)).Where(x => x.Exists);
        }

        public void Abort() {
            Aborted = true;
            var cts = CTS;
            if (cts != null)
                CTS.Value.Cancel();
        }

        #endregion
    }

    public class StatusInfo : IEquatable<StatusInfo>
    {
        public StatusInfo(RepoStatus action, double progress, long speed, int? active, int done) {
            if (progress.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(progress));
            Action = action;
            Progress = progress;
            Speed = speed;
            Active = active;
            Done = done;
        }

        public RepoStatus Action { get; }
        public double Progress { get; }
        public long Speed { get; }
        public int? Active { get; }
        public int Done { get; }

        public bool Equals(StatusInfo other) {
            return other != null && other.Action == Action && other.Progress.Equals(Progress) && other.Speed == Speed &&
                   other.Active == Active && other.Done == Done;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) Action;
                hashCode = (hashCode*397) ^ Progress.GetHashCode();
                hashCode = (hashCode*397) ^ Speed.GetHashCode();
                hashCode = (hashCode*397) ^ Active.GetHashCode();
                hashCode = (hashCode*397) ^ Done;
                return hashCode;
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as StatusInfo);
        }
    }

    public static class StatusInfoExtensions
    {
        public static StatusInfo IncrementDone(this StatusInfo statusInfo) {
            return new StatusInfo(statusInfo.Action, statusInfo.Progress, statusInfo.Speed, statusInfo.Active,
                statusInfo.Done + 1);
        }

        public static StatusInfo UpdateProgress(this StatusInfo statusInfo, double progress) {
            return new StatusInfo(statusInfo.Action, progress, statusInfo.Speed, statusInfo.Active, statusInfo.Done);
        }

        public static StatusInfo Finish(this StatusInfo statusInfo) {
            return new StatusInfo(statusInfo.Action, 100, 0, 0, statusInfo.Done);
        }
    }
}