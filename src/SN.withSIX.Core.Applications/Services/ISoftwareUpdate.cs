// <copyright company="SIX Networks GmbH" file="ISoftwareUpdate.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SN.withSIX.Core.Applications.Services
{
    public interface ISoftwareUpdate
    {
        ISelfUpdater SU { get; }
        Uri ChangelogURL { get; set; }
        bool NewVersionAvailable { get; set; }
        bool NewVersionInstalled { get; set; }
        bool NewVersionDownloaded { get; set; }
        string UpdateStatus { get; set; }
        bool IsNotInstalled { get; }
        Version OldVersion { get; set; }
        Version CurrentVersion { get; }
        bool UpdateAndExitIfNotBusy(bool force = false);
        Task TryCheckForUpdates();
        bool InstallAndExitIfNotBusy();
    }
}