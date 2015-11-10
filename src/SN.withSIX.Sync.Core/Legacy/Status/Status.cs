// <copyright company="SIX Networks GmbH" file="Status.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Legacy.Status
{
    [DoNotObfuscateType]
    public class Status : TransferStatus, IStatus
    {
        long _fileSize;
        long _fileSizeNew;
        long _fileSizeTransfered;

        public Status(string item, StatusRepo repo, double progress = 0, int speed = 0,
            TimeSpan? eta = null, RepoStatus action = RepoStatus.Waiting) : base(item, progress, speed, eta, action) {
            Repo = repo;
            Repo.AddItem(this);
            Color = "Green";
        }

        public StatusRepo Repo { get; }
        public override long FileSizeTransfered
        {
            get { return _fileSizeTransfered; }
            set
            {
                SetProperty(ref _fileSizeTransfered, value);
                if (Repo != null)
                    Repo.FileSizeTransfered += value;
            }
        }
        public override long FileSize
        {
            get { return _fileSize; }
            set
            {
                SetProperty(ref _fileSize, value);
                if (Repo != null)
                    Repo.FileSize += value;
            }
        }
        public override long FileSizeNew
        {
            get { return _fileSizeNew; }
            set
            {
                SetProperty(ref _fileSizeNew, value);
                if (Repo != null)
                    Repo.FileSizeNew += value;
            }
        }
        public string RealObject { get; set; }

        public int CompareTo(IStatus other) {
            var actionState = (Action == RepoStatus.Waiting || Action == RepoStatus.Finished) &&
                              (other.Action != RepoStatus.Waiting && other.Action != RepoStatus.Finished)
                ? 1
                : 0;
            if (actionState != 0)
                return actionState;

            if ((Progress > 0.00 && Progress < 100.0) && (other.Progress <= 0.00 || other.Progress >= 100))
                return -1;

            if ((Progress <= 0.00 || Progress >= 100) && (other.Progress <= 0.00 || other.Progress >= 100))
                return 0;

            if ((other.Progress > 0.00 && other.Progress < 100.0) && (Progress <= 0.00 || Progress >= 100))
                return 1;

            return Progress.CompareTo(other.Progress);
        }
    }
}