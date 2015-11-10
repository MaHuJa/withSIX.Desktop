// <copyright company="SIX Networks GmbH" file="Witcher3Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;

namespace SN.withSIX.Mini.Plugin.Witcher3.Models
{
    [Game(GameUUids.Witcher3, Executables = new[] {@"bin\x64\witcher3.exe"}, Name = "The Witcher 3", IsPublic = false,
        Slug = "witcher-3")]
    [RegistryInfo(@"SOFTWARE\GOG.com\Games\1207664643", "PATH")]
    [DataContract]
    public class Witcher3Game : BasicGame
    {
        public Witcher3Game(Guid id, Witcher3GameSettings settings) : base(id, settings) {}
    }
}