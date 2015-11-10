// <copyright company="SIX Networks GmbH" file="SyncBaseDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Infra.Api.ContentApi.Dto
{
    interface ISyncBaseDto
    {
        [DataMember]
        [JsonProperty("id")]
        Guid Id { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        [JsonProperty("created_at")]
        DateTime CreatedAt { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        [JsonProperty("updated_at")]
        DateTime UpdatedAt { get; set; }
    }

    [DataContract, DoNotObfuscateType]
    class SyncBaseDto : ISyncBaseDto
    {
        [Obsolete, DataMember]
        public string Uuid { get; set; }
        [DataMember]
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (Uuid != null) {
                Guid id;
                if (Guid.TryParse(Uuid, out id)) {
                    Id = id;
                    Uuid = null;
                }
            }
        }
    }
}