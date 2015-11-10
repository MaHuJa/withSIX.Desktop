// <copyright company="SIX Networks GmbH" file="PlaySquirrel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using Squirrel;

namespace SN.withSIX.Play.Applications.Services
{
    public class PlaySquirrel
    {
        public async Task<Version> GetNewVersion() {
            var updateInfo = await new SquirrelUpdater().CheckForUpdates().ConfigureAwait(false);
            return NotEqualVersions(updateInfo) && HasFutureReleaseEntry(updateInfo)
                ? updateInfo.FutureReleaseEntry.Version.Version
                : null;
        }

        static bool HasFutureReleaseEntry(UpdateInfo updateInfo) {
            return updateInfo.FutureReleaseEntry != null;
        }

        static bool NotEqualVersions(UpdateInfo updateInfo) {
            return updateInfo.FutureReleaseEntry != updateInfo.CurrentlyInstalledVersion;
        }
    }
}