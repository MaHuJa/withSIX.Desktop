// <copyright company="SIX Networks GmbH" file="TakeOnMarsGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [Game(GameUUids.TKOM, Name = "Take on Mars", Slug = "Take-on-Mars", Executables = new[] {"tkom.exe"})]
    [SteamInfo(244030, "Take on Mars")]
    [RegistryInfo(@"SOFTWARE\Bohemia Interactive\Take On Mars", "main")]
    public class TakeOnMarsGame : BasicGame
    {
        protected TakeOnMarsGame(Guid id) : this(id, new TakeOnMarsGameSettings()) {}
        public TakeOnMarsGame(Guid id, TakeOnMarsGameSettings settings) : base(id, settings) {}
    }
}