// <copyright company="SIX Networks GmbH" file="ContentBaseDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Infra.Api.ContentApi.Dto
{
    [DataContract, DoNotObfuscateType]
    class NewSyncBaseDto : ISyncBaseDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        public DateTime CreatedAt { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        public DateTime UpdatedAt { get; set; }
    }

    [DataContract, DoNotObfuscateType]
    class NewContentBaseDto : NewSyncBaseDto
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string HomepageUrl { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string Revision { get; set; }
    }

    [DataContract, DoNotObfuscateType]
    class ContentBaseDto : SyncBaseDto
    {
        [DataMember]
        [JsonProperty("description")]
        public string Description { get; set; }
        [DataMember]
        [JsonProperty("author")]
        public string Author { get; set; }
        [DataMember]
        [JsonProperty("homepage_url")]
        public string HomepageUrl { get; set; }
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("version")]
        public string Version { get; set; }
        [DataMember]
        [JsonProperty("revision")]
        public string Revision { get; set; }
    }
}