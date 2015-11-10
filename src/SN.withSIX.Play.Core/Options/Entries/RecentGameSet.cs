// <copyright company="SIX Networks GmbH" file="RecentGameSet.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "RecentGameSet",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class RecentGameSet
    {
        [DataMember] readonly DateTime _on;
        [DataMember] readonly Guid _Uuid;

        public RecentGameSet(Game game) {
            _Uuid = game.Id;
            _on = Tools.Generic.GetCurrentUtcDateTime;
        }

        public Guid Id
        {
            get { return _Uuid; }
        }
    }
}