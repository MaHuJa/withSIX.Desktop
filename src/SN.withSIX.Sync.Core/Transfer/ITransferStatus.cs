// <copyright company="SIX Networks GmbH" file="ITransferStatus.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface ITransferStatus : ITransferProgress, IProgress
    {
        string Color { get; set; }
        TimeSpan? TimeTaken { get; set; }
        bool Failed { get; set; }
        string Item { get; }
        string Info { get; set; }
        string FileStatus { get; set; }
        string ProcessCl { get; set; }
        long FileSize { get; set; }
        long FileSizeNew { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        void Reset(RepoStatus action = RepoStatus.Processing);
        void UpdateStamp();
        void UpdateTimeTaken();
        void Fail();
        void FailOutput();
        void EndOutput();
        void EndOutput(string f);
        void FailOutput(string f);
        void StartOutput(string f);
    }
}