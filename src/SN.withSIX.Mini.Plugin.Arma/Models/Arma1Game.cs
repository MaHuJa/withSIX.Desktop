// <copyright company="SIX Networks GmbH" file="Arma1Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NDepend.Path;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    /*
    [Game(GameUUids.Arma1, Name = "Arma", Slug = "Arma",
        Executables = new[] {"arma.exe"},
        ServerExecutables = new[] {"armaserver.exe"},
        IsPublic = true,
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer})
    ]
    */

    [SynqRemoteInfo("1ba63c97-2a18-42a7-8380-70886067582e", "82f4b3b2-ea74-4a7c-859a-20b425caeadb"
        /*GameUUids.Arma1 */)]
    [RegistryInfo(BohemiaStudioRegistry + @"\ArmA", "main")]
    [RvProfileInfo("Arma", "Arma other profiles",
        "ArmAProfile")]
    [SteamInfo(2780, "ArmA Armed Assault")]
    [DataContract]
    public abstract class Arma1Game : ArmaGame
    {
        static readonly IReadOnlyCollection<string> defaultModFolders = new[] {"dbe1"};
        protected Arma1Game(Guid id) : this(id, new Arma1GameSettings()) {}
        public Arma1Game(Guid id, Arma1GameSettings settings) : base(id, settings) {}

        protected override IEnumerable<IAbsoluteDirectoryPath> GetAdditionalLaunchMods() {
            return defaultModFolders.Select(x => InstalledState.Directory.GetChildDirectoryWithName(x));
        }
    }
}