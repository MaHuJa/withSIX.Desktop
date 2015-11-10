// <copyright company="SIX Networks GmbH" file="RepositoryStorePackagesDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    [DataContract, DoNotObfuscateType]
    public class RepositoryStorePackagesDto
    {
        public RepositoryStorePackagesDto() {
            Packages = new Dictionary<string, List<string>>();
            PackagesContentTypes = new Dictionary<string, List<string>>();
            PackagesCustomConfigs = new Dictionary<string, PackagesStoreCustomConfigsDto>();
        }

        [DataMember, JsonProperty("packages")]
        public Dictionary<string, List<string>> Packages { get; set; }
        [DataMember, JsonProperty("packages_content_types")]
        public Dictionary<string, List<string>> PackagesContentTypes { get; set; }
        [DataMember, JsonProperty("packages_custom_configurations")]
        public Dictionary<string, PackagesStoreCustomConfigsDto> PackagesCustomConfigs { get; set; }
    }
}