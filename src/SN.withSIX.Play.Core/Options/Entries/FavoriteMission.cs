// <copyright company="SIX Networks GmbH" file="FavoriteMission.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "FavoriteMission",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models"
        )]
    public class FavoriteMission : FavoriteBase<MissionBase>
    {
        public FavoriteMission(MissionBase obj) : base(obj) {}
    }
}