// <copyright company="SIX Networks GmbH" file="TakeOnMarsGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class TakeOnMarsGame : Game, ILaunchWith<IBasicGameLauncher>
    {
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Take on Mars",
            ShortName = "TOM",
            Author = "Bohemia Interactive",
            Description =
                @"Take On Mars places you in the seat of a Rover Operator, allowing you to control the various, fully simulated mobile Rovers and stationary Landers. With this scientific arsenal at your disposal, you will work your way through the numerous Science Missions in each location, unlocking the secrets of Mars' distant past.
                Explore the scarred face of another world. Journey through rocky terrain and sandy wastes, pushing your vehicles to the max in this new installment to the Take On series.",
            StoreUrl = "https://store.bistudio.com/take-on-mars?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://mars.takeonthegame.com/".ToUri(),
            ReleasedOn = new DateTime(2013, 8, 10),
            Slug = "take-on-mars"
        };
        static readonly SteamInfo steamInfo = new SteamInfo(244030, "Take on Mars");
        static readonly RegistryInfo registryInfo = new RegistryInfo(@"SOFTWARE\Bohemia Interactive\Take On Mars",
            "main");

        public TakeOnMarsGame(Guid id, GameSettingsController settingsController)
            : base(
                id,
                new TakeOnMarsSettings(id, new TakeOnMarsStartupParams(DefaultStartupParameters), settingsController)) {}

        public override GameMetaData MetaData
        {
            get { return metaData; }
        }
        protected override RegistryInfo RegistryInfo
        {
            get { return registryInfo; }
        }
        protected override SteamInfo SteamInfo
        {
            get { return steamInfo; }
        }

        protected override IAbsoluteFilePath GetExecutable() {
            return GetFileInGameDirectory("tkom.exe");
        }

        public override Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory,
            string identifier) {
            throw new NotImplementedException();
        }

        public override Task<int> Launch(IGameLauncherFactory factory) {
            return LaunchBasic(factory.Create(this));
        }

        protected override string GetStartupLine() {
            return InstalledState.IsInstalled
                ? new[] {InstalledState.LaunchExecutable.ToString()}.Concat(Settings.StartupParameters.Get())
                    .CombineParameters()
                : string.Empty;
        }
    }
}