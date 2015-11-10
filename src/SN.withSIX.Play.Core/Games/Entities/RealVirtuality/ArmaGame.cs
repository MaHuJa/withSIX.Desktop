// <copyright company="SIX Networks GmbH" file="ArmaGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public abstract class ArmaGame : RealVirtualityGame, ISupportModding, ISupportMissions,
        ISupportProfiles,
        ISupportServers
    {
        ContentPaths[] _missionPaths;
        ContentPaths _modPaths;

        protected ArmaGame(Guid id, ArmaSettings settings) : base(id, settings) {
            Settings = settings;

            settings.DefaultModDirectory = GetDocumentsGamePath();
            if (settings.ModDirectory == null)
                settings.ModDirectory = settings.DefaultModDirectory;
        }

        protected ArmaGame(Guid id, GameSettingsController settingsController)
            : this(id, new ArmaSettings(id, new ArmaStartupParams(DefaultStartupParameters), settingsController)) {}

        public new ArmaSettings Settings { get; }
        protected abstract SeparateClientAndServerExecutable Executables { get; }
        protected abstract ServersQuery ServerQueryInfo { get; }
        protected override bool IsClient
        {
            get
            {
                return !Settings.StartupParameters.Server && !Settings.StartupParameters.Client &&
                       GetExecutable().FileName == Executables.Client;
            }
        }
        public abstract bool SupportsContent(Mission mission);
        public ContentPaths[] MissionPaths
        {
            get { return _missionPaths ?? (_missionPaths = GetMissionPaths(Settings.RepositoryDirectory)); }
            private set { SetProperty(ref _missionPaths, value); }
        }

        public void PublishMission(string fn) {
            PublishMissionInternal(fn);
        }

        public IEnumerable<LocalMissionsContainer> LocalMissionsContainers() {
            return GetLocalMissionsContainers();
        }

        public void UpdateMissionStates(IReadOnlyCollection<MissionBase> missions) {
            foreach (var m in missions)
                m.Controller.UpdateState(this);
        }

        public ContentPaths PrimaryContentPath
        {
            get { return ModPaths; }
        }

        public virtual bool SupportsContent(IMod mod) {
            return GetSupportedModTypes().Contains(mod.Type);
        }

        public ContentPaths ModPaths
        {
            get { return _modPaths ?? (_modPaths = GetModPaths()); }
            private set { SetProperty(ref _modPaths, value); }
        }

        public virtual IEnumerable<LocalModsContainer> LocalModsContainers() {
            var installedState = InstalledState;

            if (!installedState.IsInstalled)
                return Enumerable.Empty<LocalModsContainer>();

            var contentPaths = ModPaths;
            var list = new List<LocalModsContainer> {
                new LocalModsContainer(MetaData.Name + " Game folder", installedState.Directory.ToString(), this)
            };

            if (
                !Tools.FileUtil.ComparePathsOsCaseSensitive(contentPaths.Path.ToString(),
                    installedState.Directory.ToString()))
                list.Add(new LocalModsContainer(MetaData.Name + " Mods", contentPaths.Path.ToString(), this));
            return list;
        }

        public virtual IEnumerable<IAbsolutePath> GetAdditionalLaunchMods() {
            var additionalMods = Settings.AdditionalMods;
            return string.IsNullOrWhiteSpace(additionalMods)
                ? Enumerable.Empty<IAbsolutePath>()
                : HandleAdditionalMods(additionalMods);
        }

        public virtual void UpdateModStates(IReadOnlyCollection<IMod> mods) {
            foreach (var m in mods)
                m.Controller.UpdateState(this);
        }

        public IEnumerable<string> GetProfiles() {
            return GetProfilesInternal();
        }

        public Server CreateServer(ServerAddress address) {
            return new ArmaServer(this, address);
        }

        public abstract Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler);
        public abstract Task QueryServer(ServerQueryState state);

        public virtual IFilter GetServerFilter() {
            return Settings.ServerFilter;
        }

        public virtual bool SupportsServerType(string type) {
            return ServerQueryInfo.Tag == type;
        }

        IEnumerable<IAbsolutePath> HandleAdditionalMods(string additionalMods) {
            return additionalMods.Split(';').Select(ConvertIfValid).Where(x => x != null);
        }

        IAbsoluteDirectoryPath ConvertIfValid(string i) {
            if (i.IsValidAbsoluteDirectoryPath())
                return i.ToAbsoluteDirectoryPath();

            try {
                return InstalledState.Directory.GetChildDirectoryWithName(i);
            } catch {
                return null;
            }
        }

        public override void RefreshState() {
            UpdateInstalledState();
            UpdateContentPaths();
            CalculatedSettings.Update();
        }

        public override void Initialize() {
            base.Initialize();

            this.WhenAnyValue(x => x.InstalledState)
                .Skip(1)
                .Subscribe(x => UpdateContentPaths());

            Settings.WhenAnyValue(x => x.ServerMode, x => x.StartupParameters.Server, (x, y) => x || y)
                .Skip(1)
                .Subscribe(x => RefreshState());

            Settings.WhenAnyValue(x => x.RepositoryDirectory, x => x.ModDirectory, (x, y) => true)
                .Skip(1)
                .Subscribe(x => UpdateContentPaths());

            Settings.WhenAnyValue(x => x.IncludeServerMods)
                .Skip(1)
                .Subscribe(x => CalculatedSettings.Update());

            Settings.StartupParameters.Identities = GetProfiles().ToArray();
        }

        public override void PostInstallPreLaunch(IReadOnlyCollection<ModController> procced, bool launch = false) {
            base.PostInstallPreLaunch(procced, launch);
            if (!InstalledState.IsClient)
                InstallBiKeys(procced);
        }

        void UpdateContentPaths() {
            UpdateModPaths();
            UpdateMissionPaths();
        }

        void UpdateMissionPaths() {
            MissionPaths = GetMissionPaths(Settings.RepositoryDirectory);
        }

        void UpdateModPaths() {
            ModPaths = GetModPaths();
        }

        ContentPaths GetModPaths() {
            return InstalledState.IsInstalled
                ? new ContentPaths(GetModDirectory(), GetModRepositoryDirectory())
                : new NullContentPaths();
        }

        IAbsoluteDirectoryPath GetModDirectory() {
            return Settings.ModDirectory ?? GetDocumentsGamePath();
        }

        IAbsoluteDirectoryPath GetModRepositoryDirectory() {
            return Settings.RepositoryDirectory ?? GetModDirectory();
        }

        protected abstract IEnumerable<GameModType> GetSupportedModTypes();

        protected override Tuple<string[], string[]> StartupParameters() {
            var startupBuilder = new StartupBuilder(this);
            return startupBuilder.GetStartupParameters(GetStartupSpec());
        }

        protected override bool ShouldConnectToServer() {
            return InstalledState.IsClient || Settings.StartupParameters.Client;
        }

        protected StartupBuilderSpec GetStartupSpec() {
            return new StartupBuilderSpec {
                GamePath = InstalledState.Directory,
                ModPath = ModPaths.Path,
                GameVersion = InstalledState.Version,
                InputMods = CalculatedSettings.CurrentMods.Where(x => x.Controller.Exists),
                AdditionalLaunchMods = GetAdditionalLaunchMods().OfType<IAbsoluteDirectoryPath>(),
                Mission = CalculatedSettings.Mission,
                Server = ShouldConnectToServer() ? CalculatedSettings.Server : null,
                StartupParameters = Settings.StartupParameters.Get(),
                UseParFile = true
            };
        }

        protected override IAbsoluteFilePath GetExecutable() {
            return GetExe(GetGameDirectory(), Executables, Settings.ServerMode);
        }
    }

    public class CannotLaunchBetaWithSteamLegacyException : UserException
    {
        public CannotLaunchBetaWithSteamLegacyException(string message) : base(message) {}
        public CannotLaunchBetaWithSteamLegacyException(string message, Exception inner) : base(message, inner) {}
    }
}