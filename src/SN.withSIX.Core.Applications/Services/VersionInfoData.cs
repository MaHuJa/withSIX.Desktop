// <copyright company="SIX Networks GmbH" file="VersionInfoData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SN.withSIX.Core.Applications.Services
{
    // TODO: DTO's should move to infrastructure, leaving only Models without persistence concerns in here / domain.
    [DataContract]
    public class VersionInfoDto
    {
        public VersionInfoDto() {
            Play = new AppVersionInfosDto();
            Shared = new AppVersionInfosDto();
            Tools = new AppVersionInfosDto();
        }

        [DataMember, JsonProperty("play")]
        public AppVersionInfosDto Play { get; set; }
        [DataMember, JsonProperty("shared")]
        public AppVersionInfosDto Shared { get; set; }
        [DataMember, JsonProperty("tools")]
        public AppVersionInfosDto Tools { get; set; }
        public bool Loaded { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (Play == null)
                Play = new AppVersionInfosDto();
            if (Shared == null)
                Shared = new AppVersionInfosDto();
            if (Tools == null)
                Tools = new AppVersionInfosDto();
        }
    }

    [DataContract]
    public class AppVersionInfosDto
    {
        public AppVersionInfosDto() {
            Stable = new AppVersionInfoDto();
            Beta = new AppVersionInfoDto();
        }

        [DataMember, JsonProperty("stable")]
        public AppVersionInfoDto Stable { get; set; }
        [DataMember, JsonProperty("beta")]
        public AppVersionInfoDto Beta { get; set; }
        [DataMember, JsonProperty("deprecated_version")]
        public Version DeprecatedVersion { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (Stable == null)
                Stable = new AppVersionInfoDto();
            if (Beta == null)
                Beta = new AppVersionInfoDto();
        }
    }

    [DataContract]
    public class AppVersionInfoDto
    {
        [DataMember, JsonProperty("version")]
        public Version Version { get; set; }
        [DataMember, JsonProperty("hash")]
        public string Hash { get; set; }
    }
}