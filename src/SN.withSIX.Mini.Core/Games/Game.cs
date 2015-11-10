// <copyright company="SIX Networks GmbH" file="Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Extensions;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller.Attributes;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class Game : BaseEntity<Guid>, IContentEngineGame
    {
        internal static readonly SteamHelper SteamHelper = new SteamHelper(new SteamStuff().TryReadSteamConfig(),
            SteamStuff.GetSteamPath());
        Lazy<ContentPaths> _contentPaths;
        Lazy<GameInstalledState> _installedState;

        protected Game(Guid id, GameSettings settings) {
            Id = id;
            Settings = settings;

            Metadata = this.GetMetaData<GameAttribute>();
            RemoteInfo = this.GetMetaData<RemoteInfoAttribute>();
            RegistryInfo = this.GetMetaData(RegistryInfoAttribute.Default);
            SteamInfo = this.GetMetaData(SteamInfoAttribute.Default);
            ContentCleaning = this.GetMetaData(ContentCleaningAttribute.Default);

            LastUsedLaunchType = Metadata.LaunchTypes.First();

            _installedState = new Lazy<GameInstalledState>(GetInstalledState);
            _contentPaths = new Lazy<ContentPaths>(GetContentPathsState);

            if (!DefaultDirectoriesOverriden)
                SetupDefaultDirectories();
        }

        // We use this because of chicken-egg problems because of constructor inheritance load order
        // Where usually overriden behavior depends on state that is not yet available in the base class constructor
        protected virtual bool DefaultDirectoriesOverriden => false;
        [IgnoreDataMember]
        protected ContentCleaningAttribute ContentCleaning { get; }
        [IgnoreDataMember]
        protected RemoteInfoAttribute RemoteInfo { get; }
        [IgnoreDataMember]
        protected SteamInfoAttribute SteamInfo { get; }
        [IgnoreDataMember]
        protected RegistryInfoAttribute RegistryInfo { get; }
        [DataMember]
        public GameSettings Settings { get; protected set; }
        [IgnoreDataMember]
        public GameAttribute Metadata { get; }
        [IgnoreDataMember]
        public IEnumerable<LocalContent> LocalContent => Contents.OfType<LocalContent>();
        [IgnoreDataMember]
        public IEnumerable<NetworkContent> NetworkContent => Contents.OfType<NetworkContent>();
        [DataMember]
        public virtual ICollection<Content> Contents { get; protected set; } = new List<Content>();
        [IgnoreDataMember]
        public IEnumerable<Collection> Collections => Contents.OfType<Collection>();
        [IgnoreDataMember]
        public IEnumerable<SubscribedCollection> SubscribedCollections => Contents.OfType<SubscribedCollection>();
        [IgnoreDataMember]
        public IEnumerable<LocalCollection> LocalCollections => Contents.OfType<LocalCollection>();
        [IgnoreDataMember]
        // TODO: Mods don't have Versions yet.. We need to combine these from the Synq repositories, or we need to include them in the network mods.json...
        public IOrderedEnumerable<LocalContent> Updates => LocalContent.Select(x => new { x, Nc = NetworkContent.FirstOrDefault(c => c.Id == x.ContentId || c.PackageName == x.PackageName)})
            .Where(x => x.Nc != null && (x.x.Version == null || x.x.Version != x.Nc.Version)).Select(x => x.x).OrderByDescending(x => x.UpdatedVersion);

        [DataMember]
        public DateTime? LastPlayed { get; set; }
        [DataMember]
        public bool FirstTimeRunShown { get; set; }
        [IgnoreDataMember]
        public GameInstalledState InstalledState => _installedState.Value;
        [IgnoreDataMember]
        public ContentPaths ContentPaths => _contentPaths.Value;
        [IgnoreDataMember]
        public IEnumerable<Content> FavoriteItems => Contents.Where(x => x.IsFavorite);
        [DataMember]
        public LaunchType LastUsedLaunchType { get; set; }
        [IgnoreDataMember]
        public IEnumerable<Content> RecentItems => Contents.Where(x => x.RecentInfo != null);
        // TODO: we could also choose to implement this as a wrapper/adapter class instead
        IAbsoluteDirectoryPath IContentEngineGame.WorkingDirectory => InstalledState.WorkingDirectory;
        protected virtual IAbsoluteDirectoryPath GetContentDirectory() => InstalledState.WorkingDirectory;

        public Task ScanForLocalContent() {
            return ScanForLocalContentInternal();
        }

        Task ScanForLocalContentInternal() {
            ConfirmInstalled();
            return ScanForLocalContentImpl();
        }

        protected abstract Task ScanForLocalContentImpl();

        ContentPaths GetContentPathsState() {
            if (!InstalledState.IsInstalled)
                return ContentPaths.Default;
            var contentDir = GetContentDirectory();
            if (contentDir == null)
                return ContentPaths.Default;

            var repoDir = GetRepoDirectory();
            if (repoDir == null)
                return ContentPaths.Default;

            return new ContentPaths(contentDir, repoDir);
        }

        public virtual IReadOnlyCollection<Guid> GetCompatibleGameIds() {
            return Enumerable.Repeat(Id, 1).ToList();
        }

        void SetupDefaultDirectories() {
            if (Settings.GameDirectory == null)
                Settings.GameDirectory = GetDefaultDirectory();
            if (Settings.RepoDirectory == null && Settings.GameDirectory != null)
                Settings.RepoDirectory = Settings.GameDirectory;
        }

        GameInstalledState GetInstalledState() {
            var gameDirectory = GetGameDirectory();
            if (gameDirectory == null)
                return GameInstalledState.Default;
            var executable = GetExecutable();
            if (!executable.Exists)
                return GameInstalledState.Default;
            var launchExecutable = GetLaunchExecutable();
            if (!launchExecutable.Exists)
                return GameInstalledState.Default;
            return new GameInstalledState(executable, launchExecutable, gameDirectory, GetWorkingDirectory());
        }

        // TODO: Get this path from somewhere else than global state!!
        protected IAbsoluteDirectoryPath GetGameLocalDataFolder() {
            return Common.Paths.LocalDataPath.GetChildDirectoryWithName("games")
                .GetChildDirectoryWithName(Id.ToShortId().ToString());
        }

        public void UseContent(IContentAction<IContent> action, DoneCancellationTokenSource cts = null) {
            var recentItem = ToRecentItem(action);
            action.Name = recentItem.Name;
            recentItem.Use(cts: cts);
        }

        // TODO: Crazy OfType/type conversion
        Content ToRecentItem(IContentAction<IContent> action) {
            return action.Content.Count == 1
                ? (Content) action.Content.First().Content
                : FindOrCreateLocalCollection(action);
        }

        LocalCollection FindOrCreateLocalCollection(IContentAction<IContent> action) {
            var contents = action.Content.Select(x => new ContentSpec((Content) x.Content, x.Constraint)).ToList();
            var existing =
                Collections.OfType<LocalCollection>().FirstOrDefault(x => x.Contents.SequenceEqual(contents));
            if (existing != null)
                return existing;
            var localCollection = new LocalCollection(Id, action.Name ?? "Some items", contents) {
                Image = GetActionImage(action)
            };
            Contents.Add(localCollection);
            return localCollection;
        }

        static Uri GetActionImage(IContentAction<IContent> action) {
            //if (action.Image != null)
            //  return action.Image;
            return action.Content.Count == 1
                ? action.Content.Select(x => x.Content).OfType<IHaveImage>().FirstOrDefault()?.Image
                : null;
        }

        public void AddLocalContent(params LocalContent[] localContent) {
            Contents.AddRange(localContent);
            PrepareEvent(new LocalContentAdded(Id, localContent));
        }

        public Task<int> Play(IGameLauncherFactory factory, IContentInstallationService contentInstallation,
            IPlayContentAction<LocalContent> action) {
            foreach (var li in action.Content)
                li.Content.IsEnabled = true;

            foreach (var li in LocalContent.Except(action.Content.Select(x => x.Content)))
                li.IsEnabled = false;

            return PlayInternal(factory, contentInstallation, action);
        }

        public Task<int> Play(IGameLauncherFactory factory,
            IContentInstallationService contentInstallation, IPlayContentAction<Content> action)
            => PlayInternal(factory, contentInstallation, action);

        public async Task Install(IContentInstallationService installationService,
            IContentAction<IInstallableContent> installAction) {
            await InstallInternal(installationService, installAction).ConfigureAwait(false);
            PrepareEvent(new InstallActionCompleted(installAction, this));
        }

        public async Task Uninstall(IContentInstallationService contentInstallation,
            IUninstallContentAction<IUninstallableContent> uninstallLocalContentAction) {
            await UninstallInternal(contentInstallation, uninstallLocalContentAction).ConfigureAwait(false);
            PrepareEvent(new UninstallActionCompleted(uninstallLocalContentAction, this));
        }

        public async Task<int> Launch(IGameLauncherFactory factory,
            ILaunchContentAction<Content> launchContentAction) {
            var pid = await LaunchInternal(factory, launchContentAction).ConfigureAwait(false);
            PrepareEvent(new LaunchActionCompleted(this, pid));
            return pid;
        }

        async Task<int> PlayInternal(IGameLauncherFactory factory, IContentInstallationService contentInstallation,
            IPlayContentAction<IContent> action) {
            ConfirmPlay();
            await InstallInternal(contentInstallation, action.ToInstall()).ConfigureAwait(false);
            return await LaunchInternal(factory, action).ConfigureAwait(false);
        }

        Task InstallInternal(IContentInstallationService contentInstallation,
            IContentAction<IInstallableContent> installAction) {
            ConfirmInstall();
            return InstallImpl(contentInstallation, installAction);
        }

        async Task UninstallInternal(IContentInstallationService contentInstallation,
            IUninstallContentAction<IUninstallableContent> uninstallLocalContentAction) {
            //ConfirmUninstall()
            await UninstallImpl(contentInstallation, uninstallLocalContentAction).ConfigureAwait(false);

            foreach (var c in uninstallLocalContentAction.Content.Select(x => x.Content)
                .OfType<LocalContent>())
                Contents.Remove(c);
        }

        async Task<int> LaunchInternal(IGameLauncherFactory factory, ILaunchContentAction<IContent> launchContentAction) {
            ConfirmLaunch();

            int id;
            using (var p = await LaunchImpl(factory, launchContentAction).ConfigureAwait(false))
                id = p.Id;
            LastPlayed = Tools.Generic.GetCurrentUtcDateTime;
            PrepareEvent(new GameLaunched(this, id));

            return id;
        }

        protected abstract Task<Process> LaunchImpl(IGameLauncherFactory factory,
            ILaunchContentAction<IContent> launchContentAction);

        protected abstract Task InstallImpl(IContentInstallationService installationService,
            IContentAction<IInstallableContent> content);

        protected abstract Task UninstallImpl(IContentInstallationService contentInstallation,
            IContentAction<IUninstallableContent> uninstallLocalContentAction);

        protected virtual void ConfirmPlay() {
            ConfirmInstalled();
            ConfirmNotRunning();
        }

        protected virtual void ConfirmInstall() {
            ConfirmInstalled();
            ConfirmNotRunning();
            ConfirmContentPaths();
        }

        protected virtual void ConfirmLaunch() {
            ConfirmInstalled();
        }

        void ConfirmNotRunning() {
            if (IsRunning())
                throw new GameIsRunningException(Metadata.Name + " is already running");
        }

        void ConfirmInstalled() {
            if (!InstalledState.IsInstalled)
                throw new GameNotInstalledException(Metadata.Name + " is not found");
        }

        void ConfirmContentPaths() {
            if (!ContentPaths.IsValid)
                throw new InvalidPathsException("Invalid content target directories");
        }

        protected virtual IAbsoluteDirectoryPath GetWorkingDirectory() {
            return GetExecutable().ParentDirectoryPath;
        }

        protected virtual IAbsoluteFilePath GetExecutable() {
            var path = Metadata.Executables.Select(GetFileInGameDirectory).FirstOrDefault(p => p.Exists);
            return path ?? GetFileInGameDirectory(Metadata.Executables.First());
        }

        protected virtual IAbsoluteFilePath GetLaunchExecutable() {
            return GetExecutable();
        }

        IAbsoluteDirectoryPath GetRepoDirectory() {
            return Settings.RepoDirectory;
        }

        IAbsoluteFilePath GetFileInGameDirectory(string file) {
            return GetGameDirectory().GetChildFileWithName(file);
        }

        IAbsoluteDirectoryPath GetGameDirectory() {
            return Settings.GameDirectory;
        }

        protected virtual IAbsoluteDirectoryPath GetDefaultDirectory() {
            return RegistryInfo.TryGetDefaultDirectory() ?? SteamInfo.TryGetDefaultDirectory();
        }

        protected virtual bool IsLaunchingSteamApp() {
            var gameDir = InstalledState.Directory;
            var steamApp = SteamInfo.TryGetSteamApp();
            if (steamApp.IsValid) {
                return gameDir.Equals(steamApp.AppPath) ||
                       InstalledState.LaunchExecutable.ParentDirectoryPath.DirectoryInfo.EnumerateFiles("steam_api*.dll")
                           .Any();
            }
            return false;
        }

        protected virtual Task<LaunchGameInfo> GetDefaultLaunchInfo(IEnumerable<string> startupParameters) {
            return Task.FromResult(new LaunchGameInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory,
                startupParameters));
        }

        protected virtual Task<LaunchGameWithSteamInfo> GetSteamLaunchInfo(IEnumerable<string> startupParameters) {
            return
                Task.FromResult(new LaunchGameWithSteamInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                    InstalledState.WorkingDirectory,
                    startupParameters) {
                        SteamAppId = SteamInfo.AppId,
                        SteamDRM = SteamInfo.DRM
                    });
        }

        bool IsRunning() {
            var executable = InstalledState.Executable;
            return Tools.Processes.Running(executable.FileName);
        }

        public void UpdateSettings(GameSettings settings) {
            Settings = settings;
            // We refresh the info already here because we are in background thread..
            var installedState = GetInstalledState();
            _installedState = new Lazy<GameInstalledState>(() => installedState);
            var contentPathState = GetContentPathsState();
            _contentPaths = new Lazy<ContentPaths>(() => contentPathState);

            PrepareEvent(new GameSettingsUpdated(this));
        }

        public void ContentInstalled(params Tuple<string, string>[] content)
            => ContentInstalled((IEnumerable<Tuple<string, string>>) content);

        public void ContentInstalled(IEnumerable<Tuple<string, string>> content) {
            // TODO: Update existing local content versions..
            AddLocalContent(content.Where(x => !LocalContent.Select(lc => lc.PackageName).ContainsIgnoreCase(x.Item1))
                .Select(ConvertContent).ToArray());
        }

        // TODO: ModLocalContent vs MissionLocalContent etc?
        ModLocalContent ConvertContent(Tuple<string, string> c) {
            var content =
                NetworkContent.OfType<ModNetworkContent>()
                    .FirstOrDefault(x => x.PackageName.Equals(c.Item1, StringComparison.CurrentCultureIgnoreCase));
            return content == null
                ? new ModLocalContent(c.Item1, c.Item1, Id) {Version = c.Item2}
                : new ModLocalContent(content) {Version = c.Item2};
        }

        public void MakeFavorite(Content c) {
            c.MakeFavorite();
        }

        public void Unfavorite(Content c) {
            c.Unfavorite();
        }

        public string GetContentPath(IHavePath content) {
            return Metadata.Slug + "/" + content.GetPath();
        }

        public void RemoveCollection(Collection collection) {
            Contents.Remove(collection);
        }
    }

    public class GameTerminated : IDomainEvent
    {
        public GameTerminated(Game game, int processId) {
            Game = game;
            ProcessId = processId;
        }

        public Game Game { get; }
        public int ProcessId { get; }
    }

    public class LaunchActionCompleted : IDomainEvent
    {
        public LaunchActionCompleted(Game game, int pid) {
            Game = game;
        }

        public Game Game { get; }
    }

    public class DoneCancellationTokenSource : CancellationTokenSource
    {
        public bool Disposed { get; set; }

        protected override void Dispose(bool b) {
            base.Dispose(b);
            Disposed = true;
        }
    }

    public class UninstallActionCompleted : IDomainEvent
    {
        public UninstallActionCompleted(IUninstallContentAction<IUninstallableContent> uninstallLocalContentAction,
            Game game) {
            UninstallLocalContentAction = uninstallLocalContentAction;
            Game = game;
        }

        public IUninstallContentAction<IUninstallableContent> UninstallLocalContentAction { get; }
        public Game Game { get; }
    }

    public class InstallActionCompleted : IDomainEvent
    {
        public InstallActionCompleted(IContentAction<IInstallableContent> action, Game game) {
            Action = action;
            Game = game;
        }

        public Game Game { get; }
        public IContentAction<IInstallableContent> Action { get; }
    }

    public class GameSettingsUpdated : IDomainEvent
    {
        public GameSettingsUpdated(Game game) {
            Game = game;
        }

        public Game Game { get; }
    }

    public class InvalidPathsException : InvalidOperationException
    {
        public InvalidPathsException(string message) : base(message) {}
    }

    public class GameIsRunningException : InvalidOperationException
    {
        public GameIsRunningException(string message) : base(message) {}
    }

    public class GameNotInstalledException : InvalidOperationException
    {
        public GameNotInstalledException(string message) : base(message) {}
    }

    public class ContentUsed : IDomainEvent
    {
        public ContentUsed(Content content, DoneCancellationTokenSource token) {
            Content = content;
            Token = token;
        }

        public Content Content { get; }
        public DoneCancellationTokenSource Token { get; }
    }

    public class LocalContentAdded : IDomainEvent
    {
        public LocalContentAdded(Guid gameId, params LocalContent[] localContent) {
            GameId = gameId;
            LocalContent = localContent.ToList();
        }

        public Guid GameId { get; }
        public List<LocalContent> LocalContent { get; }
    }

    public class GameLaunched : TimestampedDomainEvent
    {
        public GameLaunched(Game game, int processId) {
            Game = game;
            ProcessId = processId;
        }

        public Game Game { get; }
        public int ProcessId { get; }
    }

    public class GameRequirementMissingException : Exception {}

    public class MultiGameRequirementMissingException : GameRequirementMissingException
    {
        public MultiGameRequirementMissingException(IReadOnlyCollection<GameRequirementMissingException> exceptions) {
            Exceptions = exceptions;
        }

        public IReadOnlyCollection<GameRequirementMissingException> Exceptions { get; }
    }
}