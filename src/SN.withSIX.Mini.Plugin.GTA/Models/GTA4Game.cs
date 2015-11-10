// <copyright company="SIX Networks GmbH" file="GTA4Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller.Attributes;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    [Game(GameUUids.GTAIV, Name = "GTA 4", Slug = "GTA-4",
        Executables = new[] {"GTAIV\\GTAIV.exe"})]
    //[GTA4ContentCleaning]
    [SteamInfo(12210, "Grand Theft Auto 4")]
    [SynqRemoteInfo(GameUUids.GTAIV)]
    [RegistryInfo(@"Rockstar Games\Grand Theft Auto IV", "InstallFolder")]
    [DataContract]
    public class GTA4Game : GTAGame
    {
        protected GTA4Game(Guid id) : this(id, new GTA4GameSettings()) {}
        public GTA4Game(Guid id, GTA4GameSettings settings) : base(id, settings) {}
    }

    public class GTA4ContentCleaningAttribute : ContentCleaningAttribute
    {
        public override IReadOnlyCollection<IRelativePath> Exclusions => GameFiles();
        // TODO
        static IReadOnlyCollection<IRelativeFilePath> GameFiles() {
            return new string[0].Select(x => x.ToRelativeFilePath()).ToList();
        }
    }
}