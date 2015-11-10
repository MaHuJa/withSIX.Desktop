// <copyright company="SIX Networks GmbH" file="RepoStatus.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Transfer
{
    [DoNotObfuscateType]
    public enum RepoStatus
    {
        Waiting,
        Downloading,
        Updating,
        Summing,
        Packing,
        Unpacking,
        Verifying,
        Resolving,
        Processing,
        CheckOut,
        Copying,
        Removing,
        Renaming,
        Moving,
        Cleaning,

        // Below this line only 'Completed' states should exist
        Finished = 900,
        Failed
    }
}