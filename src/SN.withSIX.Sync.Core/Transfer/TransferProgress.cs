// <copyright company="SIX Networks GmbH" file="TransferProgress.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Transfer
{
    [DoNotObfuscateType]
    public class TransferProgress : ITransferProgress
    {
        readonly ConsoleWriter _output = new ConsoleWriter();
        readonly Object _outputLock = new Object();

        public void ResetZsyncLoopInfo() {
            ZsyncLoopData = null;
            ZsyncLoopCount = 0;
        }

        #region ITransferProgress Members

        public TransferProgress() {
            ZsyncHttpFallbackAfter = 60;
        }

        public double Progress { get; set; }

        public long Speed { get; set; }
        public TimeSpan? Eta { get; set; }
        public bool Completed { get; set; }
        public long FileSizeTransfered { get; set; }

        public bool ZsyncIncompatible { get; set; }
        public bool ZsyncHttpFallback { get; set; }
        public int ZsyncHttpFallbackAfter { get; set; }
        public int Tries { get; set; }

        public string Output
        {
            get
            {
                lock (_outputLock)
                    return _output.ToString();
            }
        }

        public void UpdateOutput(string data) {
            lock (_outputLock)
                _output.UpdateOutput(data);
        }

        public string ZsyncLoopData { get; set; }
        public int ZsyncLoopCount { get; set; }

        #endregion
    }
}