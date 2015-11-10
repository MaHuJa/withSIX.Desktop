// <copyright company="SIX Networks GmbH" file="Arma2COGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MoreLinq;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma2COGame : Arma2OaGame
    {
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "ARMA 2: Combined Operations",
            ShortName = "ARMA II: CO",
            Author = "Bohemia Interactive",
            Description =
                "Arma 2: Combined Operations brings together the award-winning Arma 2 and it's stand-alone expansion Arma 2: Operation Arrowhead to combine them into the ultimate military combat experience.",
            Slug = "arma-2",
            StoreUrl = "https://store.bistudio.com/military-simulations-games?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://www.arma2.com/customer-support/support_en.html".ToUri(),
            ReleasedOn = new DateTime(2009, 1, 2)
        };
        internal static readonly IEnumerable<GameModType> SupportedModTypes = new[] {
            GameModType.Arma2CaMod, GameModType.Arma2Mod, GameModType.Arma2OaMod, GameModType.Arma2OaCoMod,
            GameModType.Rv3Mod, GameModType.Rv3MinMod, GameModType.Rv2MinMod, GameModType.RvMinMod
        };
        readonly Arma2FreeGame _arma2FreeGame;
        readonly Arma2Game _arma2Game;

        public Arma2COGame(Guid id, GameSettingsController settingsController, Arma2Game arma2Game,
            Arma2FreeGame arma2FreeGame)
            : this(
                id,
                new Arma2CoSettings(id, new ArmaStartupParams(DefaultStartupParameters), settingsController,
                    arma2Game.Settings, arma2FreeGame.Settings)) {
            _arma2FreeGame = arma2FreeGame;
            _arma2Game = arma2Game;
        }

        Arma2COGame(Guid id, Arma2CoSettings settings)
            : base(id, settings) {
            Settings = settings;
        }

        public new Arma2CoSettings Settings { get; private set; }
        public override GameMetaData MetaData
        {
            get { return metaData; }
        }

        public override IEnumerable<IAbsolutePath> GetAdditionalLaunchMods() {
            return
                GetAdditionalGamePaths()
                    .Concat(new[] {InstalledState.Directory})
                    .Concat(base.GetAdditionalLaunchMods());
        }

        public override void Initialize() {
            base.Initialize();

            _arma2Game.WhenAnyValue(x => x.InstalledState)
                .Skip(1)
                .Subscribe(x => RefreshState());
            _arma2FreeGame.WhenAnyValue(x => x.InstalledState)
                .Skip(1)
                .Subscribe(x => RefreshState());
        }

        IEnumerable<IAbsoluteDirectoryPath> GetAdditionalGamePaths() {
            var arma2InstalledState = _arma2Game.InstalledState;
            var arma2FreeInstalledState = _arma2FreeGame.InstalledState;

            if (!arma2InstalledState.IsInstalled && !arma2FreeInstalledState.IsInstalled)
                return Enumerable.Empty<IAbsoluteDirectoryPath>();

            var arma2Path = arma2InstalledState.IsInstalled
                ? arma2InstalledState.Directory
                : arma2FreeInstalledState.Directory;
            return
                new[] {arma2Path}.Concat(arma2InstalledState.IsInstalled
                    ? _arma2Game.GetAdditionalLaunchMods().OfType<IAbsoluteDirectoryPath>()
                    : Enumerable.Empty<IAbsoluteDirectoryPath>());
        }

        public override IEnumerable<LocalModsContainer> LocalModsContainers() {
            return base.LocalModsContainers().Concat(_arma2Game.LocalModsContainers()).DistinctBy(x => x.Path);
        }

        protected override IEnumerable<GameModType> GetSupportedModTypes() {
            return SupportedModTypes;
        }

        protected override bool GetIsInstalled() {
            return base.GetIsInstalled() && (IsMergedWithArma2() || IsArma2Installed());
        }

        bool IsMergedWithArma2() {
            var gameDir = GetGameDirectory();
            return (gameDir.GetChildFileWithName("arma2.exe").Exists &&
                    gameDir.GetChildDirectoryWithName("addons").Exists &&
                    gameDir.GetChildDirectoryWithName("dta").Exists);
        }

        bool IsArma2Installed() {
            return _arma2Game.InstalledState.IsInstalled
                   || _arma2FreeGame.InstalledState.IsInstalled;
        }
    }
}