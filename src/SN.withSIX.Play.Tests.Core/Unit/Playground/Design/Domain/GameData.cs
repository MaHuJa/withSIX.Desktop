// <copyright company="SIX Networks GmbH" file="GameData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public abstract class GameData
    {
        public IAbsoluteDirectoryPath Directory { get; set; }
    }

    public interface IMissionGameData
    {
        ContentPaths[] MissionPaths { get; }
    }

    public interface IModdingGameData
    {
        ContentPaths ModPaths { get; }
    }
}