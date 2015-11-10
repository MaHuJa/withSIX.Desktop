// <copyright company="SIX Networks GmbH" file="HomeWorld2GameData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class HomeWorld2GameData : GameData, IModdingGameData
    {
        public ContentPaths ModPaths { get; set; }
    }
}