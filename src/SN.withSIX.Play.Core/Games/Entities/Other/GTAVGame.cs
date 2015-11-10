// <copyright company="SIX Networks GmbH" file="GTAVGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;
using _Game = SN.withSIX.Play.Core.Games.Entities.Other.GTAVGame;
using _Settings = SN.withSIX.Play.Core.Games.Entities.Other.GTAVSettings;
using _StartupParameters = SN.withSIX.Play.Core.Games.Entities.Other.GTAVStartupParameters;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class GTAVGame : GTAGame, ISupportSteamSettings, ILaunchWith<IBasicGameLauncher>
    {
        static readonly SteamInfo steamInfo = new SteamInfo(271590, "Grand Theft Auto V");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Grand Theft Auto V",
            ShortName = "GTAV",
            Author = "Rockstar Games",
            Description =
                "Grand Theft Auto V is an open world, action-adventure video game developed by Rockstar North and published by Rockstar Games.",
            Slug = "gta-v",
            StoreUrl = "http://store.steampowered.com/app/271590/".ToUri(),
            SupportUrl = "https://support.rockstargames.com/hc/en-us/categories/200013306-Grand-Theft-Auto-V".ToUri(),
            ReleasedOn = new DateTime(2015, 4, 14),
            IsFree = false
        };

        public GTAVGame(Guid id, GameSettingsController settingsController)
            : base(id, new _Settings(id, new _StartupParameters(), settingsController)) {}

        public override GameMetaData MetaData
        {
            get { return metaData; }
        }
        protected override SteamInfo SteamInfo
        {
            get { return steamInfo; }
        }
        public bool LaunchUsingSteam { get; set; }
        public bool ResetGameKeyEachLaunch { get; set; }

        protected override IAbsoluteFilePath GetExecutable() {
            var playGtaVExe = GetFileInGameDirectory("PlayGTAV.exe");
            if (playGtaVExe.Exists)
                return playGtaVExe;
            return GetFileInGameDirectory("GTAVLauncher.exe");
        }

        public override async Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory,
            string identifier) {
            return null;
        }

        public override async Task<int> Launch(IGameLauncherFactory factory) {
            var launchHandler = factory.Create(this);
            await PreLaunch(launchHandler).ConfigureAwait(false);

            if (IsLaunchingSteamApp())
                return await LaunchSteam(launchHandler).ConfigureAwait(false);
            return await LaunchBasic(launchHandler).ConfigureAwait(false);
        }

        async Task<int> LaunchSteam(IBasicGameLauncher launcher) {
            var p = await LaunchSteamModern(launcher).ConfigureAwait(false);
            return await RegisterLaunchIf(p, launcher).ConfigureAwait(false);
        }

        protected async Task<Process> LaunchSteamModern(IBasicGameLauncher launcher) {
            return
                await
                    launcher.Launch(
                        SteamLaunchParameters(Enumerable.Empty<string>()))
                        .ConfigureAwait(false);
        }

        protected override string GetStartupLine() {
            return InstalledState.IsInstalled
                ? new[] {InstalledState.LaunchExecutable.ToString()}.Concat(Settings.StartupParameters.Get())
                    .CombineParameters()
                : string.Empty;
        }

        protected override GameController GetController() {
            var content = this as ISupportContent;
            if (content == null)
                return null;
            var controller = new Gta5GameController(content);
            return controller;
        }

        protected LaunchGameWithSteamInfo SteamLaunchParameters(IEnumerable<string> startupParameters) {
            return new LaunchGameWithSteamInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory, startupParameters) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    SteamAppId = SteamInfo.AppId,
                    SteamDRM = IsLaunchingSteamApp(),
                    Priority = Settings.Priority
                };
        }

        protected override LocalModsContainer GameLocalModsContainer() {
            return new LocalModsContainer(MetaData.Name + " Game folder", InstalledState.Directory.ToString(), this);
        }
    }

    public class GTAVSettings : GTAGameSettings
    {
        public GTAVSettings(Guid gameId, _StartupParameters sp, GameSettingsController controller)
            : base(gameId, sp, controller) {
            StartupParameters = sp;
        }

        public new _StartupParameters StartupParameters { get; private set; }
    }

    public class GTAVStartupParameters : GTAStartupParameters
    {
        public GTAVStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Automatically loads the in-game benchmark instead of single or multiplayer game modes")]
        public bool Benchmark
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Output frame times from the benchmark to help identify stuttering")]
        public bool BenchmarkFrameTimes
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Specifies the number of benchmark runs")]
        public string BenchmarkIterations
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Limits the benchmark to one of the four scenes")]
        public string BenchmarkPass
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Disables audio in the benchmark")]
        public bool BenchmarkNoAudio
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Disables Hyper Threading on CPUs")]
        public bool DisableHyperThreading
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Loads the game directly into a multiplayer match")]
        public bool GoStraightToMP
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Prevents the game from resetting graphics options when swapping GPUs")]
        public bool IgnoreDifferentVideoCard
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Specify the number of GPUs that should be utilized")]
        public string GPUCount
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description(
            "Overrides the Population Density setting, enabling you to manually specify the number of civilians. Use in concert with -vehicleLodBias to fine-tune Population Density to your tastes"
            )]
        public string PedLodBias
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description(
            "Sets Rockstar Social Club to offline mode, which helps accelerate single-player loading times, and eliminates any spoilerific pop-ups about friends' progress"
            )]
        public bool ScOfflineOnly
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Loads the game directly into multiplayer freemode")]
        public bool StraightIntoFreemode
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description(
            "Overrides the Population Density setting, enabling you to manually specify the number of vehicles. Use in concert with -pedLodBias to fine-tune Population Density to your tastes"
            )]
        public string VehicleLodBias
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
    }
}