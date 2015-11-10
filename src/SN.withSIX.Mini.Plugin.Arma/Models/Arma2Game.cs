// <copyright company="SIX Networks GmbH" file="Arma2Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    /*
    [Game(GameUUids.Arma2, Name = "Arma 2: Original", Slug = "Arma-2",
        Executables = new[] { "arma2.exe" },
        ServerExecutables = new[] { "arma2server.exe" },
        IsPublic = true,
        LaunchTypes = new[] { LaunchType.Singleplayer, LaunchType.Multiplayer })
    ]
    */

    [SynqRemoteInfo("1ba63c97-2a18-42a7-8380-70886067582e", "82f4b3b2-ea74-4a7c-859a-20b425caeadb"
        /*GameUUids.Arma2 */)]
    [RegistryInfo(BohemiaStudioRegistry + @"\ArmA 2", "main")]
    [RvProfileInfo("Arma 2", "Arma 2 other profiles",
        "ArmA2Profile")]
    [SteamInfo(33910, "Arma 2")]
    [DataContract]
    public abstract class Arma2Game : ArmaGame
    {
        protected Arma2Game(Guid id) : this(id, new Arma2GameSettings()) {}
        public Arma2Game(Guid id, Arma2GameSettings settings) : base(id, settings) {}
    }
}