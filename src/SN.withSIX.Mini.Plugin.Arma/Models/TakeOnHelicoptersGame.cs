// <copyright company="SIX Networks GmbH" file="TakeOnHelicoptersGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [Game(GameUUids.TKOH, Name = "Take On Helicopters", Slug = "Take-on-Helicopters",
        Executables = new[] {"takeonh.exe"},
        //IsPublic = true,
        ServerExecutables = new[] {"takeonhserver.exe"},
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer})
    ]
    [RegistryInfo(BohemiaStudioRegistry + @"\Take On Helicopters", "main")]
    [SteamInfo(65730, "Take On Helicopters")]
    [SynqRemoteInfo("1ba63c97-2a18-42a7-8380-70886067582e", "82f4b3b2-ea74-4a7c-859a-20b425caeadb" /*GameUUids.TKOH */)]
    [RvProfileInfo("Take on Helicopters",
        "Take on Helicopters other profiles", "TakeOnHProfile")]
    [DataContract]
    public class TakeOnHelicoptersGame : ArmaGame
    {
        protected TakeOnHelicoptersGame(Guid id) : this(id, new TakeOnHelicoptersGameSettings()) {}
        public TakeOnHelicoptersGame(Guid id, TakeOnHelicoptersGameSettings settings) : base(id, settings) {}
    }
}