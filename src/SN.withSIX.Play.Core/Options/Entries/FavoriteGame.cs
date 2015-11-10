// <copyright company="SIX Networks GmbH" file="FavoriteGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "FavoriteGame",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class FavoriteGame
    {
        [DataMember] readonly Guid _uuid;

        public FavoriteGame(Game game) {
            _uuid = game.Id;
        }

        public bool Matches(Game game) {
            return game != null && game.Id == _uuid;
        }
    }
}