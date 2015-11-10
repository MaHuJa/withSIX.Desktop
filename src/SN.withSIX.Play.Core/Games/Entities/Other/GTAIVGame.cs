// <copyright company="SIX Networks GmbH" file="GTAIVGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;
using _Game = SN.withSIX.Play.Core.Games.Entities.Other.GTAIVGame;
using _Settings = SN.withSIX.Play.Core.Games.Entities.Other.GTAIVSettings;
using _StartupParameters = SN.withSIX.Play.Core.Games.Entities.Other.GTAIVStartupParameters;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class GTAIVGame : GTAGame, ISupportSteamSettings, ILaunchWith<IBasicGameLauncher>
    {
        static readonly SteamInfo steamInfo = new SteamInfo(12210);
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "GTA IV",
            ShortName = "GTAIV",
            Author = "Rockstar Games",
            Description =
                "Grand Theft Auto IV is an open world, action-adventure video game developed by Rockstar North and published by Rockstar Games.",
            Slug = "gta-iv",
            StoreUrl = "http://store.steampowered.com/app/12210/".ToUri(),
            SupportUrl = "https://support.rockstargames.com/hc/en-us/categories/200013096-Grand-Theft-Auto-IV".ToUri(),
            ReleasedOn = new DateTime(2008, 12, 2),
            IsFree = false
        };

        public GTAIVGame(Guid id, GameSettingsController settingsController)
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
            return GetFileInGameDirectory("GTAIV\\GTAIV.exe");
        }

        protected override IAbsoluteDirectoryPath GetWorkingDirectory() {
            return GetExecutable().ParentDirectoryPath;
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

        protected override GameController GetController() {
            var content = this as ISupportContent;
            if (content == null)
                return null;
            var controller = new Gta4GameController(content);
            return controller;
        }

        protected override LocalModsContainer GameLocalModsContainer() {
            return new LocalModsContainer(MetaData.Name + " Game folder", GetExecutable().ParentDirectoryPath.ToString(),
                this);
        }
    }

    public class GTAIVSettings : GTAGameSettings
    {
        public GTAIVSettings(Guid gameId, _StartupParameters sp, GameSettingsController controller)
            : base(gameId, sp, controller) {
            StartupParameters = sp;
        }

        public new _StartupParameters StartupParameters { get; private set; }
    }

    public class GTAIVStartupParameters : GTAStartupParameters
    {
        public GTAIVStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        //==== [ Global ] ====
        [Description("Use the specified screen adapter number <zero-based>")]
        public string adapter
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Turn of the imposter rendering for vehicles")]
        public bool disableimposters
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Don't block the window update when it loses focus.")]
        public bool noBlockOnLostFocus
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        //==== [AUDIO] ====
        [Description("Force high-end CPU audio footprint")]
        public bool fullspecaudio
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Force low-end CPU audio footprint")]
        public bool minspecaudio
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        //==== [GLOBAL] ====
        [Description("Determines if we run the benchmark immediately")]
        public bool benchmark
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Sets graphics setting to lowest setting")]
        public bool safemode
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        //==== [GRAPHICS] ====
        [Description("Enable 64 bit mirrors")]
        public bool forcehighqualitymirrors
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("force r2vb")]
        public bool forcer2vb
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("number of frames to limit game to")]
        public string frameLimit
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Force framelock to work even in a window <works best with 60Hz monitor refresh>")]
        public bool framelockinwindow
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Force fullscreen mode")]
        public bool fullscreen
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Lets you manually set the GPU count if query fails")]
        public string gpucount
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description(" Set height of main render window <default is 480>")]
        public string height
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Use D3D runtime managed resources")]
        public bool managed
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description(
            "Disables the ability to restore the game from minimize and changing resolutions - Reduces System Memory Footprint"
            )]
        public bool nominimize
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Do not limit graphics settings")]
        public bool norestrictions
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Disable sleep delay before Present <disable fix for hard Present stalls>")]
        public bool noswapdelay
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Disable wait for vblank")]
        public bool novblank
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Set refresh rate of main render window")]
        public string refreshrate
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set game to support stereo rendering mode")]
        public bool stereo
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Use application managed resources")]
        public bool unmanaged
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description(" Set width of main render window <default is 640>")]
        public string width
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Force windowed mode")]
        public bool windowed
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        //==== [INPUT] ====
        [Description("Allow DirectInput alongside XInput support.")]
        public bool usedirectinput
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        //==== [MEMORY] ====
        [Description("Percentage of available video memory")]
        public string availablevidmem
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set the restriction the amount of available memory for managed resources")]
        public string memrestrict
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description(" Disable 32bit OS with /3GB")]
        public bool no_3GB
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Do not restrict the amount of available memory for managed resources")]
        public bool nomemrestrict
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Do not precache resources")]
        public bool noprecache
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Percentage of video memory to make available to GTA")]
        public string percentvidmem
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Amount of memory to set aside for other applications")]
        public string reserve
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Amount of memory to leave available within application space")]
        public string reservedApp
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        //==== [QUALITY SETTINGS] ====
        [Description("Automatically adjust quality setting to maintain desired frame rate <15-120>")]
        public string autoconfig
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set detail distance <0-99>")]
        public string detailquality
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set anisotropic filtering <0-4>")]
        public string renderquality
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set the number of lights that cast shadows")]
        public string shadowdensity
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set the shadow quality <0-4>")]
        public string shadowquality
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set texture quality <0-2>")]
        public string texturequality
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description("Set LOD view distance <0-99>")]
        public string viewdistance
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        //==== [TIME] ====
        [Description("Disable Time Fix")]
        public bool notimefix
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
    }
}