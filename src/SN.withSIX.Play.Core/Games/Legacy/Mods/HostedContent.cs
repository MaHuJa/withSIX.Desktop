// <copyright company="SIX Networks GmbH" file="HostedContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Api.Models;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class HostedContent : Content
    {
        [DataMember] Guid? _networkId;

        protected HostedContent(Guid id)
            : base(id) {
            NetworkId = id;
        }

        public Guid NetworkId
        {
            get { return _networkId.GetValueOrDefault(Id); }
            set { _networkId = value; }
        }

        public override Uri ProfileUrl() {
            return Tools.Transfer.JoinUri(CommonUrls.PlayUrl, GetGameSlug(), GetSlugType(), GetShortId(), GetSlug());
        }

        ShortGuid GetShortId() {
            return new ShortGuid(Id);
        }

        protected virtual string GetSlug() {
            return Name.Sluggify();
        }
    }
}