// <copyright company="SIX Networks GmbH" file="VersionIncrementer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    public static class VersionIncrementer
    {
        public static Version AutoIncrement(this Version oldVersion) {
            if (oldVersion.Revision >= 0)
                return oldVersion.IncreaseRevision();
            if (oldVersion.Build >= 0)
                return oldVersion.IncreaseBuild();

            return oldVersion.Minor >= 0
                ? oldVersion.IncreaseMinor()
                : oldVersion.IncreaseMajor();
        }

        static Version IncreaseMajor(this Version oldVersion) {
            return new Version(Math.Max(oldVersion.Major, 0) + 1, 0);
        }

        static Version IncreaseMinor(this Version oldVersion) {
            return new Version(oldVersion.Major, oldVersion.Minor + 1);
        }

        static Version IncreaseBuild(this Version oldVersion) {
            return new Version(oldVersion.Major, oldVersion.Minor, oldVersion.Build + 1);
        }

        static Version IncreaseRevision(this Version oldVersion) {
            return new Version(oldVersion.Major, oldVersion.Minor, oldVersion.Build,
                oldVersion.Revision + 1);
        }
    }
}