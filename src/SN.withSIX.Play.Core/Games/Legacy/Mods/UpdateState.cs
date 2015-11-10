// <copyright company="SIX Networks GmbH" file="UpdateState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public class UpdateState : IComparePK<UpdateState>
    {
        static readonly Version DefaultVersion = new Version("0.0.1");

        public UpdateState(IContent mod) {
            Mod = mod;
        }

        public long Size { get; set; }
        public long SizeWd { get; set; }
        public string Revision { get; set; }
        public string CurrentRevision { get; set; }
        public string CurrentVersion { get; set; }
        public IContent Mod { get; set; }

        public bool ComparePK(UpdateState other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;
            if (other.Mod == null || Mod == null)
                return false;
            return other.Mod.Equals(Mod);
        }

        public virtual bool ComparePK(object obj) {
            var emp = obj as UpdateState;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }

        public bool IsNewer() {
            var versions = GetVersions();
            return versions.Item2 > versions.Item1;
        }

        public bool IsEqual() {
            var versions = GetVersions();
            return versions.Item2 == versions.Item1;
        }

        Tuple<Version, Version> GetVersions() {
            var current = DefaultVersion;
            var latest = DefaultVersion;

            if (!string.IsNullOrWhiteSpace(Revision)) {
                Version version;
                if (Version.TryParse(Revision, out version))
                    latest = version;
            }

            if (!string.IsNullOrWhiteSpace(CurrentRevision)) {
                Version version;
                if (Version.TryParse(CurrentRevision, out version))
                    current = version;
            }

            return Tuple.Create(current, latest);
        }
    }
}