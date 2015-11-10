// <copyright company="SIX Networks GmbH" file="Homeworld2Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class Homeworld2Game : Game, ISupportModding, ILaunchWith<IHomeworld2Launcher>
    {
        const string Name = "Homeworld 2";
        static readonly RegistryInfo registryInfo =
            new RegistryInfo(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Homeworld2", "GAMEDIR");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = Name,
            ShortName = Name,
            Author = "Relic Entertainment",
            Description =
                @"In Homeworld 2, the eagerly awaited sequel to PC Gamer's 1999 Game of the Year, players create and control a mighty space armada that will decide the fate of the Homeworld universe.",
            StoreUrl = "http://www.amazon.com/Homeworld-2-Pc/dp/B000083JXD?t=wi0c8d-20".ToUri(),
            SupportUrl = @"http://forums.relicnews.com/forumdisplay.php?72-Homeworld-2-Tanis-Shipyards".ToUri(),
            ReleasedOn = new DateTime(2003, 1, 1)
        };
        ContentPaths _modPaths;

        public Homeworld2Game(Guid id, GameSettingsController settingsController)
            : this(
                id,
                new Homeworld2Settings(id, new Homeworld2StartupParameters(DefaultStartupParameters), settingsController)
                ) {}

        Homeworld2Game(Guid id, Homeworld2Settings settings) : base(id, settings) {
            Settings = settings;
        }

        public new Homeworld2Settings Settings { get; }
        protected override RegistryInfo RegistryInfo
        {
            get { return registryInfo; }
        }
        public override GameMetaData MetaData
        {
            get { return metaData; }
        }

        public bool SupportsContent(IMod mod) {
            return mod.GameId == Id;
        }

        public ContentPaths ModPaths
        {
            get { return _modPaths ?? (_modPaths = GetModPaths()); }
            private set { SetProperty(ref _modPaths, value); }
        }

        public IEnumerable<LocalModsContainer> LocalModsContainers() {
            var installedState = InstalledState;

            if (!installedState.IsInstalled)
                return Enumerable.Empty<LocalModsContainer>();

            var contentPaths = ModPaths;
            return new[] {
                new LocalModsContainer(MetaData.Name + " Mods", contentPaths.Path.ToString(), this)
            };
        }

        [Obsolete("Arma specific?")]
        public IEnumerable<IAbsolutePath> GetAdditionalLaunchMods() {
            return Enumerable.Empty<IAbsolutePath>();
        }

        public void UpdateModStates(IReadOnlyCollection<IMod> mods) {
            foreach (var m in mods)
                m.Controller.UpdateState(this);
        }

        public ContentPaths PrimaryContentPath
        {
            get { return ModPaths; }
        }

        protected override IAbsoluteFilePath GetExecutable() {
            return GetFileInGameDirectory(@"Bin\Release\Homeworld2.exe");
        }

        public override Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory,
            string identifier) {
            throw new NotImplementedException();
        }

        public override async Task<int> Launch(IGameLauncherFactory factory) {
            var launchHandler = factory.Create(this);
            var p = await launchHandler.Launch(LaunchParameters(launchHandler)).ConfigureAwait(false);
            return await RegisterLaunchIf(p, launchHandler).ConfigureAwait(false);
        }

        public override void RefreshState() {
            UpdateInstalledState();
            UpdateModPaths();
            CalculatedSettings.Update();
        }

        public override void Initialize() {
            base.Initialize();
            this.WhenAnyValue(x => x.InstalledState)
                .Skip(1)
                .Subscribe(x => UpdateModPaths());
            Settings.WhenAnyValue(x => x.RepositoryDirectory)
                .Skip(1)
                .Subscribe(x => UpdateModPaths());
        }

        void UpdateModPaths() {
            ModPaths = GetModPaths();
        }

        ContentPaths GetModPaths() {
            return InstalledState.IsInstalled
                ? new ContentPaths(GetModDirectory(), GetRepositoryDirectory())
                : new NullContentPaths();
        }

        protected override string GetStartupLine() {
            return InstalledState.IsInstalled
                ? new[] {InstalledState.LaunchExecutable.ToString()}.Concat(Settings.StartupParameters.Get())
                    .CombineParameters()
                : string.Empty;
        }

        IAbsoluteDirectoryPath GetModDirectory() {
            return InstalledState.Directory.GetChildDirectoryWithName("Data");
        }

        IAbsoluteDirectoryPath GetRepositoryDirectory() {
            return Settings.RepositoryDirectory ?? GetModDirectory();
        }

        protected override IAbsoluteDirectoryPath GetWorkingDirectory() {
            return GetExecutable().ParentDirectoryPath;
        }

        LaunchGameInfo LaunchParameters(IHomeworld2Launcher launcher) {
            var clone = new Homeworld2StartupParameters(Settings.StartupParameters.Get().ToArray());

            if (clone.h.IsBlankOrWhiteSpace() && clone.w.IsBlankOrWhiteSpace() && clone.Windowed == false) {
                var size = launcher.GetScreenSize();
                clone.h = size.Height.ToString();
                clone.w = size.Width.ToString();
            }

            return new LaunchGameInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory, clone.Get()) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    InjectSteam = Settings.InjectSteam,
                    Priority = Settings.Priority
                };
        }

        internal async Task<int> Launch(Homeworld2StartupParameters startupParams, IMediator mediator) {
            // .....
            throw new NotImplementedException();
        }
    }

    public interface IHomeworld2Launcher : IGameLauncher, ILaunch
    {
        ScreenResolution GetScreenSize();
    }
}