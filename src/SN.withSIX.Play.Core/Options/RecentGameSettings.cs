// <copyright company="SIX Networks GmbH" file="RecentGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class RecentGameSettings
    {
        [DataMember(Name = "ModSet")]
        public RecentCollection Collection { get; set; }
        [DataMember]
        public RecentMission Mission { get; set; }
        [DataMember]
        public RecentServer Server { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            // WORKAROUND F'ING DATALOSS POSSIBILITIES
            if (Collection != null && Collection.Id == Guid.Empty)
                Collection = null;
            if (Mission != null && Mission.Key == null)
                Mission = null;
            if (Server != null && Server.Address == null)
                Server = null;
        }
    }
}