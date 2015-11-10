// <copyright company="SIX Networks GmbH" file="IProgress.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IProgress : ITProgress
    {
        RepoStatus Action { get; set; }
    }
}