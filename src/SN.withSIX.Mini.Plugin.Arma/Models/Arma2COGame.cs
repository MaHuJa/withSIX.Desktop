// <copyright company="SIX Networks GmbH" file="Arma2COGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [Game(GameUUids.Arma2Co, Name = "Arma 2: Combined Operations", Slug = "Arma-2",
        Executables = new[] {"arma2oa.exe"},
        ServerExecutables = new[] {"arma2oaserver.exe"},
        IsPublic = true,
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer},
        Dlcs = new[] { "BAF", "PMC", "ACR" })
    ]
    [SynqRemoteInfo("1ba63c97-2a18-42a7-8380-70886067582e", "82f4b3b2-ea74-4a7c-859a-20b425caeadb"
        /*GameUUids.Arma2Co */)]
    [DataContract]
    public class Arma2COGame : Arma2OaGame
    {
        readonly Arma2COGameSettings _settings;

        protected Arma2COGame(Guid id) : this(id, new Arma2COGameSettings()) {
            SetupDefaultDirectories();
        }

        public Arma2COGame(Guid id, Arma2COGameSettings settings) : base(id, settings) {
            _settings = settings;
        }

        void SetupDefaultDirectories() {
            // TODO: Access arma2 game class instead, and e.g dirs set there too??
            if (_settings.Arma2GameDirectory == null)
                _settings.Arma2GameDirectory = GetDefaultArma2Directory();
        }

        static IAbsoluteDirectoryPath GetDefaultArma2Directory() {
            return GetDirectoryFromRegistryOrSteam(typeof (Arma2Game));
        }

        protected override IEnumerable<IAbsoluteDirectoryPath> GetAdditionalLaunchMods() {
            return _settings.Arma2GameDirectory == null
                ? base.GetAdditionalLaunchMods()
                : new[] {_settings.Arma2GameDirectory}.Concat(base.GetAdditionalLaunchMods());
        }

        static IAbsoluteDirectoryPath GetDirectoryFromRegistryOrSteam(Type type) {
            return type.GetMetaData(RegistryInfoAttribute.Default).TryGetDefaultDirectory()
                   ?? type.GetMetaData(SteamInfoAttribute.Default).TryGetDefaultDirectory();
        }
    }
}