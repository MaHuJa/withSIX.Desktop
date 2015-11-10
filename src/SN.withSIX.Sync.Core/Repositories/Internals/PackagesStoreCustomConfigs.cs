// <copyright company="SIX Networks GmbH" file="PackagesStoreCustomConfigs.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    public class PackagesStoreCustomConfigs
    {
        public PackagesStoreCustomConfigs() {
            KeepSpecificVersions = new List<Version>();
            KeepSpecificBranches = new List<string>();
        }

        public int? KeepLatestVersions { get; set; }
        public List<Version> KeepSpecificVersions { get; set; }
        public List<string> KeepSpecificBranches { get; set; }
    }

    [DataContract, DoNotObfuscateType]
    public class PackagesStoreCustomConfigsDto
    {
        public PackagesStoreCustomConfigsDto() {
            KeepSpecificVersions = new List<Version>();
            KeepSpecificBranches = new List<string>();
        }

        [DataMember, JsonProperty("keep_latest_versions", NullValueHandling = NullValueHandling.Ignore)]
        public int? KeepLatestVersions { get; set; }
        [DataMember, JsonProperty("keep_specific_versions")]
        public List<Version> KeepSpecificVersions { get; set; }
        [DataMember, JsonProperty("keep_specific_branches")]
        public List<string> KeepSpecificBranches { get; set; }
    }
}