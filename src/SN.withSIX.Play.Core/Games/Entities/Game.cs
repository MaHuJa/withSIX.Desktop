// <copyright company="SIX Networks GmbH" file="Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public abstract class Game : PropertyChangedBase, IHaveId<Guid>, ILaunchable, IHaveInstalledState, IShortcutCreation,
        IContentEngineGame
    {
        protected static readonly string[] DefaultStartupParameters = {};
        static readonly SteamInfo steamInfo = new NullSteamInfo();
        static readonly RegistryInfo registryInfo = new NullRegistryInfo();
        InstalledState _installedState;
        bool? _isFavorite;
        bool _repositorySubscription;
        RunningGame _running;
        string _startupLine;

        protected Game(Guid id, GameSettings settings) {
            Contract.Requires<ArgumentOutOfRangeException>(id != Guid.Empty);
            Contract.Requires<ArgumentNullException>(settings != null);

            Id = id;
            Settings = settings;
            Controller = GetController();
            if (Controller != null) {
                Controller.WhenAnyValue(x => x.BundleManager)
                    .Where(x => x != null)
                    .Subscribe(RepositoryLoaded);
            }

            Lists = new ContentLists();
            CalculatedSettings = new CalculatedGameSettings(this);

            Settings.DefaultDirectory = GetDefaultDirectory();
            if (Settings.Directory == null)
                Settings.Directory = Settings.DefaultDirectory;
        }

        [Obsolete]
        public ContentLists Lists { get; private set; }
        protected virtual SteamInfo SteamInfo
        {
            get { return steamInfo; }
        }
        protected virtual RegistryInfo RegistryInfo
        {
            get { return registryInfo; }
        }
        public abstract GameMetaData MetaData { get; }
        public GameController Controller { get; }
        public GameSettings Settings { get; }
        [Obsolete]
        public CalculatedGameSettings CalculatedSettings { get; }
        public string StartupLine
        {
            get { return _startupLine ?? (_startupLine = GetStartupLine()); }
            private set { SetProperty(ref _startupLine, value); }
        }
        [Obsolete("Remnant, should we deal differently with this?")]
        public bool IsFavorite
        {
            get
            {
                return _isFavorite == null
                    ? (bool) (_isFavorite = DomainEvilGlobal.Settings.GameOptions.IsFavorite(this))
                    : (bool) _isFavorite;
            }
            set
            {
                if (_isFavorite == value)
                    return;
                if (value)
                    DomainEvilGlobal.Settings.GameOptions.AddFavorite(this);
                else
                    DomainEvilGlobal.Settings.GameOptions.RemoveFavorite(this);
                _isFavorite = value;
                OnPropertyChanged();
            }
        }
        protected virtual bool IsClient
        {
            get { return true; }
        }
        IAbsoluteDirectoryPath IContentEngineGame.WorkingDirectory
        {
            get { return InstalledState.WorkingDirectory; }
        }
        public Guid Id { get; }
        public virtual InstalledState InstalledState
        {
            get { return _installedState ?? (_installedState = GetInstalledState()); }
            protected set { SetProperty(ref _installedState, value); }
        }
        public RunningGame Running
        {
            get { return _running; }
            private set { SetProperty(ref _running, value); }
        }

        public void RegisterRunning(Process p) {
            var current = Running;
            if (current != null)
                current.Dispose();
            Running = new RunningGame(this, p, CalculatedSettings.Collection, CalculatedSettings.Server);
        }

        public void RegisterTermination() {
            var rg = Running;
            Running = null;
            if (rg != null)
                rg.Dispose();
        }

        public abstract Task<int> Launch(IGameLauncherFactory factory);

        public abstract Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory,
            string identifier);

        protected IAbsoluteDirectoryPath GetDefaultSynqPath() {
            return Common.Paths.SynqRootPath.GetChildDirectoryWithName(MetaData.ShortName);
        }

        protected IAbsoluteFilePath GetFileInGameDirectory(string file) {
            return GetGameDirectory().GetChildFileWithName(file);
        }

        public Uri GetUri() {
            return Tools.Transfer.JoinUri(CommonUrls.PlayUrl, MetaData.Slug);
        }

        public virtual void PostInstallPreLaunch(IReadOnlyCollection<ModController> procced, bool launch = false) {}

        protected async Task<int> LaunchBasic(IBasicGameLauncher launcher) {
            await PreLaunch(launcher).ConfigureAwait(false);
            var p = await PerformLaunch(launcher).ConfigureAwait(false);
            return await RegisterLaunchIf(p, launcher).ConfigureAwait(false);
        }

        public Tuple<IAbsoluteDirectoryPath, string> InvalidPaths() {
            var gamePath = InstalledState.Directory;
            if (!CheckPath(gamePath, "Game Path", true))
                return Tuple.Create(gamePath, "Game Path");

            var modding = this as ISupportModding;
            if (modding != null) {
                var modPaths = modding.ModPaths;
                var modPath = modPaths.Path;
                if (!CheckPath(modPath, "Mod Path"))
                    return Tuple.Create(modPath, "Mod Path");
            }
            var content = this as ISupportContent;
            if (content != null) {
                var contentPath = content.PrimaryContentPath;
                var synqPath = contentPath.RepositoryPath;
                if (!CheckPath(synqPath, "Synq Path"))
                    return Tuple.Create(synqPath, "Synq Path");
            }
            return null;
        }

        bool CheckPath(IAbsoluteDirectoryPath mp, string pathName, bool mustExist = false) {
            return mp != null && Tools.FileUtil.IsValidRootedPath(mp.ToString(), mustExist);
        }

        public virtual async Task<Exception[]> UpdateSynq(IContentManager contentList,
            bool isInternetAvailable) {
            var content = this as ISupportContent;
            if (content == null || !InstalledState.IsInstalled || !content.PrimaryContentPath.IsValid)
                return new Exception[0];

            var ex = new List<Exception>();
            await Controller.UpdateBundleManager().ConfigureAwait(false);
            if (isInternetAvailable)
                ex.Add(await UpdateRemotes(Controller.BundleManager).ConfigureAwait(false));
            Controller.Update();

            var modding = this as ISupportModding;
            if (modding != null)
                ProcessSynqMods(modding, contentList, Controller);
            var missions = this as ISupportMissions;
            if (missions != null)
                ProcessSynqMissions(missions, contentList, Controller);
            return ex.Where(x => x != null).ToArray();
        }

        protected static async Task<Exception> UpdateRemotes(BundleManager bm) {
            try {
                await bm.UpdateRemotesConditional().ConfigureAwait(false);
            } catch (TransferException e) {
                return e;
            }
            return null;
        }

        protected static void ProcessSynqMods(ISupportModding modding, IContentManager modList,
            params GameController[] controllers) {
            foreach (
                var mod in modList.Mods.Where(
                    x => !(x is LocalMod) && !(x is CustomRepoMod) && controllers.Any(y => y.Supports(x)))
                    .ToArrayLocked())
                ProcessSynqMod(controllers, mod);
        }

        protected static void ProcessSynqMissions(ISupportMissions missions, IContentManager missionList,
            params GameController[] controllers) {
            foreach (
                var mission in missionList.Missions.Where(
                    x => controllers.Any(y => x.GameMatch((Game) y.Game)))
                    .ToArrayLocked())
                ProcessSynqMission(controllers, mission);
        }

        static void ProcessSynqMission(GameController[] controllers, Mission mission) {
            var gameMatch = controllers.FirstOrDefault(x => mission.GameMatch((Game) x.Game)) ?? controllers.First();
            var package = gameMatch.FindPackage(mission);
            if (package == null)
                return;

            var mm = mission.Controller;

            mm.Package = package;
        }

        static void ProcessSynqMod(GameController[] controllers, IMod mod) {
            var gameMatch = controllers.FirstOrDefault(x => x.Supports(mod)) ?? controllers.First();
            var package = gameMatch.FindPackage(mod);
            if (package == null)
                return;

            var mm = mod.Controller;

            mm.Package = package;
        }

        protected virtual GameController GetController() {
            var content = this as ISupportContent;
            if (content == null)
                return null;
            var controller = new RealVirtualityGameController(content);
            return controller;
        }

        [Obsolete("This mess needs to get sorted")]
        void RepositoryLoaded(BundleManager bundleManager) {
            Settings.KeepLatestVersions = bundleManager.Repo.Config.KeepVersionsPerPackage;
            Settings.Refresh();
            if (_repositorySubscription)
                return;
            _repositorySubscription = true;
            Settings.WhenAnyValue(x => x.KeepLatestVersions).Skip(1).Subscribe(x => {
                var bm = Controller.BundleManager;
                if (bm == null)
                    return;
                if (this.SupportsMods() &&
                    bm.Repo.Config.KeepVersionsPerPackage != x) {
                    bm.Repo.Config.KeepVersionsPerPackage = x;
                    bm.Repo.SaveConfig();
                }
            });
        }

        // Because of issues with Lazy Initialization being triggered by this.WhenAny, we have to call this method from the outside after instantiating a game object
        // not the most elegant, but nothing better available atm..
        public virtual void Initialize() {
            // TODO: For some weird reason this.WhenAny(x => x.Settings...  does not work
            // perhaps has to do with the 'new' more specialized overrides in derrivatives?
            Settings.WhenAnyValue(x => x.Directory)
                .Skip(1)
                .Subscribe(x => RefreshState());

            Settings.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "")
                    RefreshState();
            };
        }

        Task<Process> PerformLaunch(IBasicGameLauncher launcher) {
            return SteamInfo.DRM
                ? launcher.Launch(SteamLaunchParameters())
                : launcher.Launch(LaunchParameters());
        }

        protected virtual void LaunchChecks() {
            if (!InstalledState.IsInstalled) {
                throw new GameNotDetectedException("Game does not appear to be installed: " + TryGetGameDirectory() +
                                                   ", " +
                                                   TryGetExecutable());
            }
        }

        IAbsoluteDirectoryPath TryGetGameDirectory() {
            try {
                return GetGameDirectory();
            } catch {
                return null;
            }
        }

        IAbsoluteFilePath TryGetExecutable() {
            try {
                return GetExecutable();
            } catch {
                return null;
            }
        }

        protected async Task PreLaunch(IGameLauncher launcher) {
            LaunchChecks();
            await HandleUserLaunchChecks(launcher).ConfigureAwait(false);
            await launcher.Notify(new PreGameLaunchEvent(this)).ConfigureAwait(false);
        }

        async Task HandleUserLaunchChecks(IGameLauncher launcher) {
            if (Common.Flags.IgnoreErrorDialogs)
                return;

            var preLaunchCancellable = new PreGameLaunchCancelleableEvent(this,
                CalculatedSettings.Collection, CalculatedSettings.Mission, CalculatedSettings.Server);
            await launcher.Notify(preLaunchCancellable).ConfigureAwait(false);

            if (preLaunchCancellable.Cancel)
                throw new OperationCanceledException("User Cancelled PreLaunch Checks");

            if (Common.AppCommon.IsBusy)
                throw new BusyStateHandler.BusyException();
        }

        protected async Task<int> RegisterLaunchIf(Process p, IGameLauncher launcher) {
            if (p == null)
                return 0;

            var id = p.Id;
            // TODO: better not to hold on to the process because of info going out of date / Id can't be called when the process is killed etc?
            RegisterRunning(p);
            await launcher.Notify(new GameLaunchedEvent(Running, CalculatedSettings.Server)).ConfigureAwait(false);

            return id;
        }

        InstalledState GetInstalledState() {
            return GetIsInstalled()
                ? new InstalledState(GetExecutable(), GetLaunchExecutable(), GetGameDirectory(),
                    GetWorkingDirectory(), GetVersion(), IsClient)
                : new NotInstalledState();
        }

        protected virtual IAbsoluteFilePath GetLaunchExecutable() {
            return GetExecutable();
        }

        public virtual void RefreshState() {
            UpdateInstalledState();
            CalculatedSettings.Update();
        }

        protected void UpdateInstalledState() {
            InstalledState = GetInstalledState();
            UpdateStartupLine();
        }

        public void UpdateStartupLine() {
            StartupLine = GetStartupLine();
        }

        protected abstract string GetStartupLine();
        protected abstract IAbsoluteFilePath GetExecutable();

        protected virtual bool GetIsInstalled() {
            var gameDirectory = GetGameDirectory();
            return gameDirectory != null && gameDirectory.Exists && GetExecutable().Exists;
        }

        protected virtual IAbsoluteDirectoryPath GetWorkingDirectory() {
            return GetGameDirectory();
        }

        protected Version GetVersion() {
            var executable = GetExecutable();
            if (!executable.Exists)
                return null;

            var versionInfo = FileVersionInfo.GetVersionInfo(executable.ToString());
            return versionInfo.ProductVersion.ParseVersion() ?? versionInfo.FileVersion.ParseVersion();
        }

        protected bool GetLaunchAsAdministrator() {
            return Settings.LaunchAsAdministrator || CalculatedSettings.CurrentMods.Any(x => x.RequiresAdminRights());
        }

        LaunchGameInfo LaunchParameters() {
            return new LaunchGameInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory,
                Settings.StartupParameters.Get()) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    InjectSteam = Settings.InjectSteam,
                    Priority = Settings.Priority
                };
        }

        LaunchGameWithSteamInfo SteamLaunchParameters() {
            return new LaunchGameWithSteamInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory,
                Settings.StartupParameters.Get()) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    SteamAppId = SteamInfo.AppId,
                    SteamDRM = SteamInfo.DRM,
                    Priority = Settings.Priority
                };
        }

        protected IAbsoluteDirectoryPath GetGameDirectory() {
            return Settings.Directory;
        }

        IAbsoluteDirectoryPath GetDefaultDirectory() {
            return TryGetDefaultDirectoryFromRegistry() ?? TryGetDefaultDirectoryFromSteam();
        }

        IAbsoluteDirectoryPath TryGetDefaultDirectoryFromRegistry() {
            if (RegistryInfo.Path != null) {
                var path = Tools.Generic.NullSafeGetRegKeyValue<string>(RegistryInfo.Path, RegistryInfo.Key);
                if (path != null && path.IsValidAbsoluteDirectoryPath())
                    return path.ToAbsoluteDirectoryPath();
            }
            return null;
        }

        IAbsoluteDirectoryPath TryGetDefaultDirectoryFromSteam() {
            var steamApp = TryGetSteamApp();
            return steamApp == null ? null : steamApp.AppPath;
        }

        protected SteamApp TryGetSteamApp() {
            var steamHelper = DomainEvilGlobal.LocalMachineInfo.SteamHelper;
            if (SteamInfo is NullSteamInfo || SteamInfo.AppId <= 0 || !steamHelper.SteamFound)
                return null;
            var steamApp = steamHelper.GetSteamAppById(SteamInfo.AppId);
            return steamApp;
        }

        protected virtual bool IsLaunchingSteamApp() {
            var gameDir = GetGameDirectory();
            var steamApp = TryGetSteamApp();

            if (steamApp != null) {
                return steamApp.AppPath.ToString().ToLower() == gameDir.ToString().ToLower() ||
                       GetLaunchExecutable().ParentDirectoryPath.DirectoryInfo.EnumerateFiles("steam_api*.dll").Any();
            }
            return false;
        }

        protected class SeparateClientAndServerExecutable
        {
            public SeparateClientAndServerExecutable(string client, string server) {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(client));
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(server));

                Client = client;
                Server = server;
            }

            public string Client { get; private set; }
            public string Server { get; private set; }
        }
    }

    public class PreGameLaunchEvent
    {
        public PreGameLaunchEvent(Game game) {
            Game = game;
        }

        public Game Game { get; }
    }

    [Obsolete]
    public class ContentLists : PropertyChangedBase
    {
        Collection[] _collections = new Collection[0];
        CustomCollection[] _customCollections = new CustomCollection[0];
        LocalMissionsContainer[] _customLocalMissionsContainers = new LocalMissionsContainer[0];
        LocalModsContainer[] _customLocalModContainers = new LocalModsContainer[0];
        Mission[] _missions = new Mission[0];
        IMod[] _mods = new IMod[0];
        public Mission[] Missions
        {
            get { return _missions; }
            set { SetProperty(ref _missions, value); }
        }
        public IMod[] Mods
        {
            get { return _mods; }
            set { SetProperty(ref _mods, value); }
        }
        public Collection[] Collections
        {
            get { return _collections; }
            set { SetProperty(ref _collections, value); }
        }
        public CustomCollection[] CustomCollections
        {
            get { return _customCollections; }
            set { SetProperty(ref _customCollections, value); }
        }
        public LocalModsContainer[] CustomLocalModContainers
        {
            get { return _customLocalModContainers; }
            set { SetProperty(ref _customLocalModContainers, value); }
        }
        public LocalMissionsContainer[] CustomLocalMissionsContainers
        {
            get { return _customLocalMissionsContainers; }
            set { SetProperty(ref _customLocalMissionsContainers, value); }
        }
    }

    [DoNotObfuscate]
    public class GameNotDetectedException : UserException
    {
        public GameNotDetectedException(string message) : base(message) {}
    }

    public static class SAExtensions
    {
        [Obsolete("Damn workaround!")]
        public static ServerAddress FixArmaQueryAddress(this ServerAddress addr) {
            return addr.Port == 2302 ? new ServerAddress(addr.IP, 2303) : addr;
        }

        [Obsolete("Damn workaround!")]
        public static ServerAddress GetArmaServerPort(this ServerAddress addr) {
            return addr.Port == 2302 ? addr : new ServerAddress(addr.IP, addr.Port - 1);
        }
    }

    public static class Extensions
    {
        public static bool SupportsMissions(this Game game) {
            return game is ISupportMissions;
        }

        public static bool SupportsMods(this Game game) {
            return game is ISupportModding;
        }

        public static bool SupportsServers(this Game game) {
            return game is ISupportServers;
        }

        public static ISupportMissions Missions(this Game game) {
            return (ISupportMissions) game;
        }

        public static ISupportModding Modding(this Game game) {
            return (ISupportModding) game;
        }

        public static ISupportServers Servers(this Game game) {
            return (ISupportServers) game;
        }

        public static IEnumerable<Process> RunningProcesses(this IEnumerable<Game> games) {
            var all =
                games.Select(x => x.InstalledState).Where(x => x.IsInstalled).Select(
                    x =>
                        Tools.Processes.FindProcess(x.Executable.FileNameWithoutExtension));
            return all.Aggregate(new Process[0], (current, a) => current.Concat(a).ToArray());
        }
    }

    public class SynqPathChangedEvent : IDomainEvent
    {
        public SynqPathChangedEvent(ISupportModding game, ContentPaths oldPaths, ContentPaths newPaths) {
            Contract.Requires<ArgumentNullException>(game != null);
            Game = game;
            OldPaths = oldPaths;
            NewPaths = newPaths;
        }

        public ISupportModding Game { get; private set; }
        public ContentPaths OldPaths { get; private set; }
        public ContentPaths NewPaths { get; private set; }
    }

    public class ModPathChangedEvent : IDomainEvent
    {
        public ModPathChangedEvent(ISupportModding game, ContentPaths oldPaths, ContentPaths newPaths) {
            Contract.Requires<ArgumentNullException>(game != null);
            Game = game;
            OldPaths = oldPaths;
            NewPaths = newPaths;
        }

        public ISupportModding Game { get; private set; }
        public ContentPaths OldPaths { get; private set; }
        public ContentPaths NewPaths { get; private set; }
    }

    public class ModAndSynqPathsChangedEvent : IDomainEvent
    {
        public ModAndSynqPathsChangedEvent(ISupportModding game, ContentPaths oldPaths, ContentPaths newPaths) {
            Contract.Requires<ArgumentNullException>(game != null);
            Game = game;
            OldPaths = oldPaths;
            NewPaths = newPaths;
        }

        public ISupportModding Game { get; private set; }
        public ContentPaths OldPaths { get; private set; }
        public ContentPaths NewPaths { get; private set; }
    }
}