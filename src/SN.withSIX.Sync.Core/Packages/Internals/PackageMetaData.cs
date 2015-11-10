// <copyright company="SIX Networks GmbH" file="PackageMetaData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    public class PackageMetaData : MetaDataBase, IComparePK<PackageMetaData>
    {
        public PackageMetaData(string fqn) : this(new SpecificVersion(fqn)) {}

        public PackageMetaData(SpecificVersion fullyQualifiedName)
            : base(fullyQualifiedName) {
            Files = new Dictionary<string, string>();
            Licenses = new List<string>();
            ReleaseType = String.Empty;
            ContentType = String.Empty;
        }

        public List<string> Licenses { get; set; }
        public long Size { get; set; }
        public long SizePacked { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public string ContentType { get; set; }
        public string ReleaseType { get; set; }

        public override bool ComparePK(object other) {
            var o = other as PackageMetaData;
            if (o != null)
                return ComparePK((PackageMetaData) other);
            var o2 = other as Dependency;
            return o2 != null && ComparePK((Dependency) other);
        }

        public bool ComparePK(PackageMetaData other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        public IEnumerable<Dependency> GetDependencies() {
            return Dependencies.Select(x => new Dependency(x.Key, x.Value));
        }

        public IEnumerable<FileObjectMapping> GetFiles() {
            return Files.Select(x => new FileObjectMapping(x.Key, x.Value));
        }

        public bool Compare(PackageMetaData other) {
            if (other.Files.Count != Files.Count)
                return false;

            if (other.Size != Size)
                return false;

            if (other.SizePacked != SizePacked)
                return false;

            if (other.Files.Any(x => !Files.ContainsKey(x.Key) || !Files[x.Key].Equals(x.Value)))
                return false;

            if (Files.Any(x => !other.Files.ContainsKey(x.Key) || !other.Files[x.Key].Equals(x.Value)))
                return false;

            return true;
        }

        public PackageMetaData SpawnNewVersion(string desiredVersion) {
            var dep = new Dependency(Name, desiredVersion);
            var metaData = Repository.MappingEngine.Map<PackageMetaData>(this);
            metaData.Date = Tools.Generic.GetCurrentDateTime;
            metaData.Version = new Version(dep.Version);
            metaData.Branch = dep.Branch;
            metaData.Size = 0;
            metaData.SizePacked = 0;
            return metaData;
        }
    }
}