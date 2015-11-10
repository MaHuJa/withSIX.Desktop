// <copyright company="SIX Networks GmbH" file="MultiThreadingSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Sync.Core.Legacy
{
    public class MultiThreadingSettings
    {
        const int DefaultMaxThreads = 6;
        const int DefaultMaxThreadsChecksums = 2;
        const int DefaultHostThreadDiv = 2;

        public MultiThreadingSettings() {
            IsEnabled = true;
            Checksums = false;
            Pack = true;
            Wd = true;
            MultiMirror = true;
            PackInclUnpack = true;
            HostThreadDiv = DefaultHostThreadDiv;
            MaxThreads = DefaultMaxThreads;
            MaxThreads2 = DefaultMaxThreadsChecksums;
        }

        public bool IsEnabled { get; set; }
        public bool Checksums { get; set; }
        public bool MultiMirror { get; set; }
        public bool Pack { get; set; }
        public bool PackInclUnpack { get; set; }
        public bool Wd { get; set; }
        public int HostThreadDiv { get; set; }
        public int MaxThreads { get; set; }
        public int MaxThreads2 { get; set; }
    }
}