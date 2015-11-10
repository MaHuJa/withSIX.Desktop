// <copyright company="SIX Networks GmbH" file="ITransferProgress.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface ITransferProgress : ITProgress
    {
        TimeSpan? Eta { get; set; }
        bool Completed { get; set; }
        string Output { get; }
        long FileSizeTransfered { get; set; }
        string ZsyncLoopData { get; set; }
        int ZsyncLoopCount { get; set; }
        bool ZsyncIncompatible { get; set; }
        bool ZsyncHttpFallback { get; set; }
        int ZsyncHttpFallbackAfter { get; set; }
        int Tries { get; set; }
        void UpdateOutput(string data);
        void ResetZsyncLoopInfo();
    }
}