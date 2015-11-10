// <copyright company="SIX Networks GmbH" file="RepositoryConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    public class RepositoryConfig
    {
        public RepositoryConfig() {
            Uuid = Guid.NewGuid();
            Remotes = new Dictionary<Guid, Uri[]>();
        }

        public string Name { get; set; }
        public Guid Uuid { get; set; }
        public Dictionary<Guid, Uri[]> Remotes { get; set; }
        public bool UseVersionedPackageFolders { get; set; }
        public RepositoryOperationMode OperationMode { get; set; }
        public int? KeepVersionsPerPackage { get; set; }
    }

    public class RepositoryConfigDto
    {
        public RepositoryConfigDto() {
            Remotes = new Dictionary<Guid, Uri[]>();
            Uuid = Guid.NewGuid();
        }

        public RepositoryConfigDto(RepositoryConfig config) {
            Name = config.Name;
            Uuid = config.Uuid;
            Remotes = config.Remotes.ToDictionary(x => x.Key,
                x => x.Value);
            Mode = config.OperationMode;
            UseVersionedPackageFolders = config.UseVersionedPackageFolders;
            KeepVersionsPerPackage = config.KeepVersionsPerPackage;
        }

        [DataMember, JsonProperty("name")]
        public string Name { get; set; }
        [DataMember, JsonProperty("uuid")]
        public Guid Uuid { get; set; }
        [DataMember, JsonProperty("mode")]
        public RepositoryOperationMode Mode { get; set; }
        [DataMember, JsonProperty("use_versioned_package_folders")]
        public bool UseVersionedPackageFolders { get; set; }
        [DataMember, JsonProperty("keep_versions_per_package")]
        public int? KeepVersionsPerPackage { get; set; }
        [DataMember, JsonProperty("remotes")]
        public Dictionary<Guid, Uri[]> Remotes { get; set; }
    }
}