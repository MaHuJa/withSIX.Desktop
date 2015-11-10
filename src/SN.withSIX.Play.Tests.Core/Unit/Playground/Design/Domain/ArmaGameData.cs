// <copyright company="SIX Networks GmbH" file="ArmaGameData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class ArmaGameData : RealVirtualityGameData, IModdingGameData, IMissionGameData
    {
        public ContentPaths[] MissionPaths { get; set; }
        public ContentPaths ModPaths { get; set; }
    }
}