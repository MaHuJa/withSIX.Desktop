// <copyright company="SIX Networks GmbH" file="FavoriteDlc.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "FavoriteDlc",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class FavoriteDlc
    {
        [DataMember] readonly Guid _Uuid;

        public FavoriteDlc(Dlc dlc) {
            _Uuid = dlc.Id;
        }

        public Dlc Dlc { get; private set; }

        public bool Matches(Dlc dlc) {
            return dlc != null && dlc.Id == _Uuid;
        }
    }
}