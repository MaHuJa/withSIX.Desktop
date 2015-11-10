// <copyright company="SIX Networks GmbH" file="IronFrontGame.cs">
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
    [Game(GameUUids.IronFront, Name = "Iron Front", Slug = "Iron-Front",
        Executables = new[] {"ironfront.exe"},
        ServerExecutables = new[] {"ironfrontserver.exe"},
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer})
    ]
    [RegistryInfo(BohemiaStudioRegistry + @"\Ironfront", "main")]
    [SteamInfo(91330, "IronFront")]
    [RvProfileInfo("ironfront", "ironfront other profiles",
        "IFProfile")]
    [SynqRemoteInfo("1ba63c97-2a18-42a7-8380-70886067582e", "82f4b3b2-ea74-4a7c-859a-20b425caeadb"
        /*GameUUids.IronFront */)]
    [DataContract]
    public class IronFrontGame : RealVirtualityGame
    {
        protected IronFrontGame(Guid id) : this(id, new IronFrontGameSettings()) {}
        public IronFrontGame(Guid id, IronFrontGameSettings settings) : base(id, settings) {}
    }
}