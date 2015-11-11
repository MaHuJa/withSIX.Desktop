// <copyright company="SIX Networks GmbH" file="ContentManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using MoreLinq;
using NDepend.Path;
using ReactiveUI;
using ShortBus;
using SmartAssembly.ReportUsage;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Api.Models.Content.Arma3;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Applications.UseCases.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Games.Legacy.Servers;
using SN.withSIX.Play.Core.Games.Services.Infrastructure;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Sync.Core.ExternalTools;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.Yoma;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using Timer = System.Timers.Timer;

namespace SN.withSIX.Play.Applications.Services
{
    public class ContentManager : IHandle<ModPathChanged>, IHandle<GamePathChanged>,
        IContentManager, IHandle<SynqPathChanged>, IHandle<GameContentAfterSynced>,
        IHandle<ApiKeyUpdated>, IEnableLogging, IApplicationService
    {
        const long AttachmentCeil = 20*1024*1024;
        static readonly Regex oldStyleRegex =
            new Regex(@"^" + SixRepo.PwsProtocolRegex + @"(server|mod|game|mission|ts3|publishMission)=");
        static readonly string[] ignoredCollectionProperties = {
            "Revision", "Version", "CppName", "Aliases", "Author", "Description"
        };
        static readonly Regex rx = new Regex(@"^Collection [(][#]([0-9]+)[)]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly int timeLoop = 15.Minutes();
        readonly SemaphoreSlim _afterSyncLock = new SemaphoreSlim(1);
        readonly IContentApiHandler _apiHandler;
        readonly SemaphoreSlim _apiSyncLock = new SemaphoreSlim(1);
        readonly IAuthProvider _authProvider;
        readonly IConnectApiHandler _connectApi;
        readonly IGameContext _context;
        readonly IDialogManager _dialogManager;
        readonly IFileDownloadHelper _downloader;
        readonly IEventAggregator _eventBus;
        readonly EvilGlobalSelectedGame _evilGlobalSelectedGame = DomainEvilGlobal.SelectedGame;
        readonly IGameContext _gameContext;
        readonly Func<ISupportServers, ExportLifetimeContext<IServerList>> _getServerList;
        readonly Lazy<LaunchManager> _launchManager;
        readonly Timer _listTimer;
        readonly IMediator _mediator;
        readonly PboTools _pboTools;
        readonly IResourceService _resources;
        readonly UserSettings _settings;
        readonly IShutdownHandler _shutdownHandler;
        readonly IStringDownloader _stringDownloader;
        readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1);
        readonly ISystemInfo _systemInfo;
        readonly object _updateAllModsLock = new object();
        readonly Lazy<IUpdateManager> _updateManager;
        bool _initial = true;
        bool _initialSyncedSyncManager;
        volatile bool _isSyncingApi;
        Game _lastGame;
        ExportLifetimeContext<IServerList> _serverList;

        public ContentManager(IGameContext gameContext, IMediator mediator,
            IAuthProvider authProvider,
            IStringDownloader stringDownloader, IFileDownloadHelper downloader,
            UserSettings settings, ISystemInfo systemInfo,
            Func<ISupportServers, ExportLifetimeContext<IServerList>> getServerList,
            IShutdownHandler shutdownHandler, PboTools pboTools, IConnectApiHandler connectApi,
            Lazy<IUpdateManager> updateManager, Lazy<LaunchManager> launchManager, IDialogManager dialogManager,
            IEventAggregator ea, IContentApiHandler apiHandler, IGameContext context, IContentEngine contentEngine,
            IResourceService resources) {
            ContentEngine = contentEngine;
            _resources = resources;
            _listTimer = new TimerWithElapsedCancellationAsync(timeLoop, ListTimerElapsed, null, false);
            _eventBus = ea;
            _apiHandler = apiHandler;
            _context = context;

            _gameContext = gameContext;
            _mediator = mediator;
            _authProvider = authProvider;
            _stringDownloader = stringDownloader;
            _downloader = downloader;
            _updateManager = updateManager;
            _launchManager = launchManager;
            _dialogManager = dialogManager;
            _systemInfo = systemInfo;
            _settings = settings;
            _getServerList = getServerList;
            _shutdownHandler = shutdownHandler;


            _settings.GameOptions.GameSettingsController.WhenAnyValue(x => x.ActiveProfile)
                .Skip(1)
                .Subscribe(x => SelectLastUsed());

            // sucks
            Collections = gameContext.Collections.Local;
            CustomCollections = _gameContext.CustomCollections.Local;
            SubscribedCollections = _gameContext.SubscribedCollections.Local;
            CustomRepositories = _gameContext.CustomRepositories;
            LocalModsContainers = _gameContext.LocalModsContainers;

            Collections.TrackChanges(x1 => x1.Process(this), null, reset => reset.ForEach(x => x.Process(this)));
            CustomCollections.TrackChanges(x2 => { x2.Process(this); }, null,
                reset1 => { reset1.ForEach(x => x.Process(this)); });
            SubscribedCollections.TrackChanges(x3 => { x3.Process(this); },
                x => _settings.ModOptions.SubscribedCollections.Remove(x),
                reset2 => { reset2.ForEach(x => x.Process(this)); });

            Mods = gameContext.Mods.Local;
            Missions = gameContext.Missions.Local;
            LocalMissionsContainers = gameContext.LocalMissionsContainers;
            _pboTools = pboTools;
            _connectApi = connectApi;

            CalculatedGameSettings.ContentManager = this;

            HandleServerList(Game);
        }

        Game Game
        {
            get { return _evilGlobalSelectedGame.ActiveGame; }
        }
        public bool SyncManagerSynced { get; private set; }

        public async Task InitAsync(bool syncOnline = true) {
            await TryLoadFromDisk().ConfigureAwait(false);
            if (syncOnline)
                await TrySync().ConfigureAwait(false);
        }

        public Task Sync(ApiHashes hashes = null, bool suppressExceptionDialog = false) {
            return TrySync(hashes, suppressExceptionDialog);
        }

        public IContentEngine ContentEngine { get; }
        public ReactiveList<Collection> Collections { get; }
        public ReactiveList<Mod> Mods { get; }
        public ReactiveList<Mission> Missions { get; }
        public ReactiveList<LocalMissionsContainer> LocalMissionsContainers { get; }
        public ReactiveList<LocalModsContainer> LocalModsContainers { get; }
        public ReactiveList<SixRepo> CustomRepositories { get; }
        public ReactiveList<CustomCollection> CustomCollections { get; }
        public ReactiveList<SubscribedCollection> SubscribedCollections { get; }
        public IServerList ServerList
        {
            get
            {
                var sl = _serverList;
                return sl == null ? null : sl.Value;
            }
        }

        public CustomCollection CreateAndSelectCustomModSet(IContent content = null) {
            var modSet = CreateAndAddCustomModSet(content);
            SelectCollection(modSet);

            Common.App.PublishDomainEvent(new CollectionCreatedEvent(modSet));

            return modSet;
        }

        public CustomCollection CreateAndSelectCustomModSet(IReadOnlyCollection<IContent> content) {
            var modSet = CreateAndAddCustomModSet(content);
            SelectCollection(modSet);

            Common.App.PublishDomainEvent(new CollectionCreatedEvent(modSet));

            return modSet;
        }

        public async Task HandlePwsUrl(string pwsUrl) {
            var param = pwsUrl.TrimEnd('/');
            this.Logger().Info("Processing Processed Param: {0}", param);
            var state = new UrlHandlerState(param);
            await HandlePreActions(state).ConfigureAwait(false);
            await HandleUri(state).ConfigureAwait(false);
            await HandleAfterActions(state).ConfigureAwait(false);
        }

        public void UpdateCollectionStates() {
            GetAllCollectionsFor(Game).ForEach(x => x.UpdateStatus(Game));
        }

        public async Task<IAbsoluteFilePath> CreateIcon(Collection collection) {
            var url = new Uri(ContentBase.GetResourcePath(collection.Image));
            var path = Common.Paths.LocalDataPath + @"\Icons";
            var png = Path.Combine(path, collection.Id + ".png").ToAbsoluteFilePath();
            var icon = png.ToString().Replace(".png", ".ico");
            await _downloader.DownloadFileAsync(url, png).ConfigureAwait(false);

            WriteIconToDisk(icon, png);
            return icon.ToAbsoluteFilePath();
        }

        public async Task InitialServerSync(bool updateOnlyWhenActive = false) {
            if (!Game.SupportsServers() || ServerList.InitialSync ||
                ServerList.DownloadingServerList)
                return;

            await
                ServerList.GetAndUpdateAll(updateOnlyWhenActive, !_systemInfo.IsInternetAvailable).ConfigureAwait(false);
        }

        public async Task RefreshCollectionInfo(Collection collection, bool report = true) {
            var modSet = collection as AdvancedCollection;
            if (modSet != null)
                await modSet.HandleCustomRepositories(this, report).ConfigureAwait(false);
        }

        public async Task ProcessLegacyCustomCollection(CustomCollection modSet, bool report) {
            await ProcessCustomRepo(modSet, report).ConfigureAwait(false);
            _updateManager.Value.RefreshModInfo();
        }

        public async Task<SixRepo> GetRepo(Uri uri) {
            retry:
            try {
                return
                    await
                        LoadRemoteRepoCached(uri).ConfigureAwait(false);
            } catch (HttpDownloadException e) {
                // TODO: Or should we rather abstract this away into the downloader exceptions instead?
                if (e.StatusCode != HttpStatusCode.Unauthorized)
                    throw;
            } catch (FtpDownloadException e) {
                if (e.StatusCode != FtpStatusCode.NotLoggedIn && e.StatusCode != FtpStatusCode.AccountNeeded &&
                    e.StatusCode != FtpStatusCode.NeedLoginAccount)
                    throw;
            }

            uri = await GetNewUri(uri);
            goto retry;
        }

        public void SelectMission(MissionBase mission) {
            _evilGlobalSelectedGame.ActiveGame.CalculatedSettings.Mission = mission;
        }

        public Task PublishMission(MissionBase missionBase, string missionName) {
            var mission = missionBase as Mission;
            return mission != null
                ? PublishMission(mission, missionName)
                : PublishMission((MissionFolder) missionBase, missionName);
        }

        public MissionBase[] GetLocalMissions(string path = null) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(path));
            var missions = GetMissions(path).ToArray();
            return missions;
        }

        public async Task<List<MissionModel>> GetMyMissions(Game game) {
            _connectApi.ConfirmLoggedIn();
            using (var session = await _connectApi.StartSession().ConfigureAwait(false)) {
                var missions =
                    await
                        _connectApi.GetMyMissions(game.MetaData.Slug, 1)
                            .ConfigureAwait(false);

                await session.Close();
                return missions.Items;
            }
        }

        public IEnumerable<IMod> FindOrCreateLocalMods(ISupportModding game, IEnumerable<string> mods,
            IReadOnlyCollection<IMod> inputMods = null) {
            Contract.Requires<ArgumentNullException>(game != null);
            Contract.Requires<ArgumentNullException>(mods != null);

            return NonNullOrEmptyMods(mods)
                .Select(mod => FindMod(mod, inputMods) ?? LocalMod.FromStringIfValid(mod, game))
                .Where(m => m != null)
                .DistinctBy(x => x.Name.ToLower());
        }

        public IEnumerable<string> GetModsInclDependencies(IList<string> modList,
            IReadOnlyCollection<IMod> inputMods = null) {
            Contract.Requires<ArgumentNullException>(modList != null);
            return GetDependencies(modList, inputMods).Concat(modList).Distinct();
        }

        public IEnumerable<string> GetDependencies(IList<string> mods, IReadOnlyCollection<IMod> inputMods = null) {
            Contract.Requires<ArgumentNullException>(mods != null);
            return NonNullOrEmptyMods(mods).Select(mod => FindMod(mod, inputMods)).Where(m => m != null)
                .SelectMany(m => m.Dependencies)
                .Where(m => !mods.ContainsIgnoreCase(m))
                .DistinctBy(x => x.ToLower());
        }

        public IEnumerable<IMod> GetMods(ISupportModding game, IEnumerable<string> mods,
            IReadOnlyCollection<IMod> inputMods = null) {
            Contract.Requires<ArgumentNullException>(game != null);
            Contract.Requires<ArgumentNullException>(mods != null);

            return HandleDependencies(game, FindOrCreateLocalMods(game, mods, inputMods), inputMods);
        }

        public IMod FindMod(string mod, IReadOnlyCollection<IMod> inputMods = null) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(mod));
            mod = Path.GetFileName(mod);
            return FindModInLists(mod, Mods.ToArrayLocked(), inputMods);
        }

        public IEnumerable<IMod> CompatibleMods(IEnumerable<IMod> inputMods, ISupportModding game) {
            Contract.Requires<ArgumentNullException>(game != null);
            Contract.Requires<ArgumentNullException>(inputMods != null);

            return inputMods.Where(x => x.CompatibleWith(game));
        }

        public CustomCollection CreateCustomRepoServerModSet(SixRepoServer repoServer, string key, SixRepo repo) {
            var modSet = new CustomCollection(Guid.NewGuid(), GetDesiredGame(repoServer).Modding()) {
                ServerKey = key,
                IsHidden = repoServer.IsHidden
            };
            var url = repo.GetUrl(key);
            modSet.UpdateCustomRepoServerInfo(this, repoServer, url, repo);
            return modSet;
        }

        public CustomCollection CreateCustomCollection(ISupportModding game, IContent content = null,
            CustomCollection existingCol = null) {
            var name = GetSuggestedCollectionName(content);
            var ms = content as Collection;
            if (ms != null) {
                var col = ms.Clone();
                col.Name = name;
                if (existingCol != null)
                    HandleExistingCol(col, existingCol);
                return col;
            } else {
                var col = new CustomCollection(Guid.NewGuid(), game) {
                    Name = name
                };
                if (existingCol != null)
                    HandleExistingCol(col, existingCol);
                var mod = content.ToMod();
                if (mod != null) {
                    col.UpdateFromMod(mod);
                    col.Image = mod.Image;
                    col.ImageLarge = mod.ImageLarge;
                    col.AddModAndUpdateStateIfPersistent(mod, this);
                }
                return col;
            }
        }

        public string GetSuggestedCollectionName(IContent content = null) {
            return String.Format("Collection ({0})", content == null ? "#" + GetNewCollectionId() : content.Name);
        }

        public CustomCollection CreateAndAddCustomModSet(Server server) {
            var collection = CreateCustomCollection(server);
            lock (CustomCollections)
                CustomCollections.Add(collection);
            return collection;
        }

        public void RemoveCollection(CustomCollection collection) {
            var cgs = Game.CalculatedSettings;
            if (cgs.Collection == collection)
                cgs.Collection = null;
            lock (CustomCollections)
                CustomCollections.Remove(collection);
        }

        public void SelectCollection(Collection collection) {
            Game.CalculatedSettings.Collection = collection;
        }

        public Collection CloneCollection(Collection current) {
            var cm = current.Clone();
            cm.UpdateState();
            if (cm.Name.EndsWith(")")) {
                var bracket = cm.Name.LastIndexOf("(");
                var strNumber = cm.Name.Substring(bracket + 1, cm.Name.Length - bracket - 2);
                var number = 0;
                int.TryParse(strNumber, out number);
                number += 1;
                cm.Name = cm.Name.Substring(0, bracket) + "(" + number + ")";
            } else
                cm.Name = cm.Name + " (1)";
            lock (CustomCollections)
                CustomCollections.Add(cm);
            return cm;
        }

        async Task TryLoadFromDisk() {
            await _apiSyncLock.WaitAsync().ConfigureAwait(false);
            try {
                _isSyncingApi = true;
                await TryLoadFromDiskInternal().ConfigureAwait(false);
                if (_apiHandler.Loaded) {
                    await ImportResultsAsync().ConfigureAwait(false);
                    SyncManagerSynced = true;
                    await AnnounceSyncFinished().ConfigureAwait(false);
                }
            } finally {
                _isSyncingApi = false;
                _apiSyncLock.Release();
            }
        }

        async Task AnnounceSyncFinished() {
            await _afterSyncLock.WaitAsync().ConfigureAwait(false);
            try {
                await TryAfterSync().ConfigureAwait(false);
                _eventBus.PublishOnCurrentThread(new GameContentSynced());
                _eventBus.PublishOnCurrentThread(new GameContentAfterSynced());

                if (!_initialSyncedSyncManager) {
                    // This event is ever only expected to fire once!
                    _eventBus.PublishOnCurrentThread(new GameContentInitialSynced());
                    _initialSyncedSyncManager = true;
                }
                DomainEvilGlobal.SelectedGame.ActiveGame.CalculatedSettings.InitialSynced = true;
            } finally {
                _afterSyncLock.Release();
            }
        }

        async Task TryLoadFromDiskInternal() {
            try {
                await _apiHandler.LoadFromDisk().ConfigureAwait(false);
            } catch (Exception e) {
                await UserError.Throw(new InformationalUserError(e,
                    "An error occurred while trying to load API SyncData: " + e.Message, null));
            }
        }

        async Task TrySync(ApiHashes hashes = null, bool suppressExceptionDialog = false) {
            try {
                await InternalSync(hashes).ConfigureAwait(false);
            } catch (RestExceptionBase e) {
                if (suppressExceptionDialog)
                    this.Logger().FormattedErrorException(e);
                else {
                    await UserError.Throw(new InformationalUserError(e,
                        "An issue occurred during Synchronization. If the application doesn't work properly, best try restarting\n\nMain reasons for failed sync is unable to connect to withSIX API, either due to internet outage, firewall misconfiguration or possible API outage.",
                        "An issue occurred during Synchronization"));
                }
            } catch (Exception e) {
                if (suppressExceptionDialog)
                    this.Logger().FormattedErrorException(e);
                else {
                    await UserError.Throw(new InformationalUserError(e,
                        "An issue occurred during Synchronization. If the application doesn't work properly, best try restarting\n\nMain reasons for failed sync is unable to connect to withSIX API, either due to internet outage, firewall misconfiguration or possible API outage.",
                        null));
                }
            }
        }

        async Task<bool> ListTimerElapsed() {
            if (!_isSyncingApi)
                await TrySync(null, true).ConfigureAwait(false);
            return true;
        }

        async Task InternalSync(ApiHashes hashes = null) {
            await _apiSyncLock.WaitAsync().ConfigureAwait(false);

            try {
                _isSyncingApi = true;
                _listTimer.Stop();
                ExceptionDispatchInfo ex = null;
                try {
                    var changed = await
                        (hashes == null ? _apiHandler.LoadFromApi() : _apiHandler.LoadFromApi(hashes)).ConfigureAwait(
                            false);
                    if (changed)
                        await ImportResultsAsync().ConfigureAwait(false);
                } catch (Exception e) {
                    ex = ExceptionDispatchInfo.Capture(e);
                }

                SyncManagerSynced = true;
                await AnnounceSyncFinished().ConfigureAwait(false);
                if (ex != null)
                    ex.Throw();
            } finally {
                _isSyncingApi = false;
                _apiSyncLock.Release();
                _listTimer.Stop();
                _listTimer.Start();
            }
        }

        Task ImportResultsAsync() {
            return Task.Run(() => ImportResults());
        }

        void ImportResults() {
            _context.ImportMissions(_apiHandler.GetList<Mission>());
            _context.ImportMods(_apiHandler.GetList<Mod>());
            _context.ImportCollections(_apiHandler.GetList<Collection>());
        }

        void HandleServerList(Game game) {
            if (_serverList != null && ServerList.Game == game)
                return;

            var oldList = _serverList;
            var supportsServers = game as ISupportServers;
            _serverList = supportsServers != null ? _getServerList(supportsServers) : null;

            if (oldList != null)
                oldList.Dispose();

            if (_settings.AppOptions.ServerListEnabled) {
                var t = InitialServerSync();
            }
        }

        async Task TryAfterSync() {
            try {
                using (this.Bench())
                    await AfterSync().ConfigureAwait(false);
            } catch (Exception e) {
                await UserError.Throw(new InformationalUserError(e,
                    "A problem occurred while processing API data, please try again later: " + e.Message, null));
            }
        }

        static string FixOldStyleUrl(string param) {
            return oldStyleRegex.Replace(param, x => "pws" + x.Groups[1].Value + "://?" + x.Groups[2].Value + "=");
        }

        async Task HandlePreActions(UrlHandlerState state) {
            foreach (var item in state.Dic) {
                switch (item.Key.ToLower()) {
                case PwsUrlPreActions.Game: {
                    var gameId = new Guid(item.Value);
                    _evilGlobalSelectedGame.ActiveGame =
                        _gameContext.Games.Find(gameId);
                    break;
                }
                case PwsUrlPreActions.Mod: {
                    var modId = Guid.Parse(item.Value);
                    await HandleModAction(modId).ConfigureAwait(false);

                    break;
                }
                case PwsUrlPreActions.ModSet: {
                    var modSetId = Guid.Parse(item.Value);
                    var modSet = Find(x => x.Id == modSetId);
                    SelectCollection(modSet);
                    break;
                }
                case PwsUrlAfterActions.Mission: {
                    SelectMissionByUuid(Guid.Parse(item.Value));
                    break;
                }
                case PwsUrlAfterActions.MissionPackage: {
                    SelectMissionPackage(item);
                    break;
                }
                case PwsUrlAfterActions.Share: {
                    Share(item);
                    break;
                }
                case PwsUrlAfterActions.Server: {
                    SelectServer(item);
                    break;
                }
                case PwsUrlPreActions.Collection: {
                    if (item.Value.ToLower() == "subscribe")
                        state.Subscribe = true;
                    break;
                }
                }
            }
        }

        async Task HandleModAction(Guid modId) {
            var mod = Mods.FirstOrDefault(x => x.Id == modId);
            if (mod == null) {
                await Sync().ConfigureAwait(false);
                mod = Mods.FirstOrDefault(x => x.Id == modId);
                if (mod == null) {
                    await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                        "The mod that was requested either does not exist on the network or is not yet avalible for clients. Please try again in 10 minutes.",
                        "The mod request was not found."));
                }
            }
            if (mod != null)
                CreateAndSelectCustomModSet(mod);
        }

        // TODO: Move this kind of messaging to the website?
        // TODO: The processing of PwSUri is rather a Presentation concern, which should execute App/Domain layer stuff..
        void Share(KeyValuePair<string, string> item) {
            var relative = item.Value.Substring(1);
            Common.App.PublishEvent(
                new ShareToContactEvent(new FakeContent("Shared content",
                    Tools.Transfer.JoinUri(CommonUrls.PlayUrl, relative))));
            /*
            var split = relative.Split('/');
            var game = _gameContext.Games.First(x => x.MetaData.Slug.Equals(split[0], StringComparison.InvariantCultureIgnoreCase));
            var contentType = split[1].ToLower();
            var contentShortId = ShortGuid.Parse(split[2]);

            switch (contentType) {
                case "missions": {
                    Common.App.PublishEvent(new ShareToContactEvent(new FakeContent("Shared content", Tools.Transfer.JoinUri(CommonUrls.PlayUrl, relative))));
                    break;
                }
                case "collections": {
                    Common.App.PublishEvent(new ShareToContactEvent(new CustomCollection(contentShortId.ToGuid(), game.Modding()) { Name = "Share content" }));
                    break;
                }
                case "mods": {
                    Common.App.PublishEvent(new ShareToContactEvent(new Mod(contentShortId.ToGuid().ToString()) { Name = "Share content", Type =  }));
                    break;
                }
            }
*/
        }

        async Task HandleUri(UrlHandlerState state) {
            if (state.Uri.AbsolutePath.EndsWith(Repository.ConfigFileName, StringComparison.OrdinalIgnoreCase)) {
                await HandleCustomRepo(state.Uri, true).ConfigureAwait(false);
                return;
            }

            if (state.Uri.AbsolutePath.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)) {
                await HandleCustomRepoServer(state.Uri, true).ConfigureAwait(false);
                return;
            }

            if (state.Uri.AbsolutePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase)) {
                this.Logger().Info("Processing YASRepo");
                await HandleYasRepo(state.Uri).ConfigureAwait(false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(state.Uri.DnsSafeHost) && !string.IsNullOrWhiteSpace(state.Uri.AbsolutePath)) {
                await
                    HandleCustomRepo(Tools.Transfer.JoinUri(state.Uri, Repository.ConfigFileName), true)
                        .ConfigureAwait(false);
            }
        }

        async Task HandleAfterActions(UrlHandlerState state) {
            foreach (var item in state.Dic) {
                switch (item.Key.ToLower()) {
                case PwsUrlAfterActions.CollectionId: {
                    await HandleCollectionAction(ShortGuid.Parse(item.Value), state).ConfigureAwait(false);
                    break;
                }
                case PwsUrlAfterActions.PublishMission: {
                    await PublishMission(item.Value);
                    break;
                }
                case PwsUrlAfterActions.Ts3: {
                    Tools.Generic.TryOpenUrl("ts3server://" + item.Value);
                    break;
                }
                case PwsUrlAfterActions.Action: {
                    var actions = item.Value.Split(',');
                    if (actions.Length > 0) {
                        if (!Common.Flags.SkipExecutionConfirmation) {
                            var report = !(await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                                "You're about to execute the following actions on the active collection: " +
                                String.Join(", ", actions) + "\nFull URL: " + state.Param,
                                "About to execute, are you sure?",
                                SixMessageBoxButton.YesNo))).IsYes();

                            UsageCounter.ReportUsage(
                                "Dialog - Execute actions on the active collection: {0}".FormatWith(report));

                            if (report)
                                continue;
                        }
                    }
                    if (await PerformActions(actions))
                        return;
                    break;
                }
                }
            }
        }

        async Task HandleCollectionAction(ShortGuid collectionId, UrlHandlerState state) {
            var subscribe = state.Subscribe;

            if (subscribe) {
                // ReSharper disable once SimplifyLinqExpression (The simplified expression would break this statement)
                if (!SubscribedCollections.Any(x => x.CollectionID == collectionId))
                    await FetchCollection(collectionId).ConfigureAwait(false);
            }

            // ReSharper disable once SimplifyLinqExpression (The simplified expression would break this statement)
            if (!SubscribedCollections.Any(x => x.CollectionID == collectionId)) {
                var c =
                    await
                        _mediator.RequestAsync(new FetchCollectionQuery(collectionId.ToGuid())).ConfigureAwait(false);
                Common.App.PublishEvent(
                    new RequestOpenBrowser(c.ProfileUrl(_evilGlobalSelectedGame.ActiveGame)));
            } else {
                var ms = SubscribedCollections.First(x => x.CollectionID == collectionId);
                SelectCollection(ms);
            }
        }

        Task FetchCollection(Guid collectionId) {
            return _mediator.RequestAsync(new ImportCollectionCommand(collectionId));
        }

        async Task<bool> PerformActions(IEnumerable<string> actions) {
            foreach (var action in actions) {
                switch (action) {
                case PwsUrlActions.Update: {
                    await HandleInstallUpdateAction().ConfigureAwait(false);
                    await Task.Delay(5.Seconds()).ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.Install: {
                    await HandleInstallUpdateAction().ConfigureAwait(false);
                    await Task.Delay(5.Seconds()).ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.Join: {
                    await HandleJoinServerAction().ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.Launch: {
                    await HandleLaunchAction().ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.Missions: {
                    await HandleFetchRepoMissionsAction().ConfigureAwait(false);
                    await Task.Delay(2.Seconds()).ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.MPMissions: {
                    await HandleFetchRepoMPMissionsAction().ConfigureAwait(false);
                    await Task.Delay(2.Seconds()).ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.Apps: {
                    await HandleProcessAppsAction().ConfigureAwait(false);
                    break;
                }
                case PwsUrlActions.Shutdown: {
                    _shutdownHandler.Shutdown();
                    return true;
                }
                }
            }
            return false;
        }

        Task HandleLaunchAction() {
            return _launchManager.Value.StartGame();
        }

        Task HandleProcessAppsAction() {
            return _updateManager.Value.ProcessRepoApps();
        }

        Task HandleFetchRepoMPMissionsAction() {
            return _updateManager.Value.ProcessRepoMpMissions();
        }

        Task HandleFetchRepoMissionsAction() {
            return _updateManager.Value.ProcessRepoMissions();
        }

        Task HandleJoinServerAction() {
            return _launchManager.Value.JoinServer();
        }

        async Task HandleInstallUpdateAction() {
            var modSet = Game.CalculatedSettings.Collection;
            if (modSet == null) {
                UsageCounter.ReportUsage("Dialog - No active modset to update");
                await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("No active modset to update"));
                return;
            }
            await _updateManager.Value.HandleConvertOrInstallOrUpdate().ConfigureAwait(false);
        }

        void SelectServer(KeyValuePair<string, string> item) {
            var addr = item.Value.TrimEnd('/');
            var server = ServerList.FindOrCreateServer(SAStuff.GetAddy(addr));
            Game.CalculatedSettings.Server = server;
        }

        void SelectMissionByUuid(Guid id) {
            Common.App.PublishEvent(new RequestShowMissionLibrary());
            SelectMission(Missions.FirstOrDefault(x => x.Id == id));
        }

        void SelectMissionPackage(KeyValuePair<string, string> item) {
            Common.App.PublishEvent(new RequestShowMissionLibrary());
            SelectMission(Missions.FirstOrDefault(x => x.PackageName == item.Value));
        }

        async Task PublishMission(string fn) {
            try {
                Game.Missions().PublishMission(fn);
                return;
            } catch (DirectoryNotFoundException) {}
            UsageCounter.ReportUsage("Dialog - Could not find the mission");
            await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("Tried to publish " + fn +
                                                                            ". But could not find the mission"));
        }

        async Task HandleYasRepo(Uri uri) {
            var url = uri.GetCleanuri().ToString();
            var yas = new YomaContent(Game.Modding().ModPaths.Path, url.GetParentUri(), _downloader);
            yas.Create();
            await yas.DownloadConfig(url.Split('/').Last()).ConfigureAwait(false);
            yas.UnpackConfig();
            yas.ParseConfig();
            await yas.Download().ConfigureAwait(false);
            yas.UnpackMod();
        }

        Task AfterSync() {
            return RefreshContentInfo();
        }

        void WriteIconToDisk(string icon, IAbsoluteFilePath png) {
            using (var inputStream = _resources.GetResource("app.ico")) {
                var iconOverlay = new Icon(inputStream);
                using (var stream = File.OpenWrite(icon)) {
                    var bitmap = (Bitmap) Image.FromFile(png.ToString());
                    Tools.FileUtil.AddIconOverlay(Icon.FromHandle(bitmap.GetHicon()), iconOverlay).Save(stream);
                }
            }
        }

        async Task RefreshContentInfo() {
            var game = Game;
            var same = _lastGame == game;
            _lastGame = game;

            await UpdateCollectionLists().ConfigureAwait(false);
            await RefreshCollectionInfo(game.CalculatedSettings.Collection, false).ConfigureAwait(false);

            if (Game.InstalledState.IsInstalled)
                Game.CalculatedSettings.UpdateSignatures();

            await UpdateGameListsAsync(game).ConfigureAwait(false);
            await UpdateContentStates().ConfigureAwait(false);
            _updateManager.Value.RefreshModInfo();
        }

        async Task ProcessCustomRepo(CustomCollection collection, bool report = true) {
            var split = collection.CustomRepoUrl.Split('/');
            var last = split.Last();
            var uri =
                new Uri(last.EndsWith(".yml")
                    ? String.Join("/", split.Take(split.Length - 1))
                    : collection.CustomRepoUrl);

            await TryUpdateCustomRepo(collection, uri, report).ConfigureAwait(false);
            await TryUpdateCustomRepoModSet(collection, report).ConfigureAwait(false);
        }

        async Task TryUpdateCustomRepo(CustomCollection collection, Uri newUri, bool report = true) {
            try {
                collection.CustomRepo = await GetRepo(newUri).ConfigureAwait(false);
            } catch (OperationCanceledException) {
                this.Logger().Info("Cancelled processing " + newUri);
            } catch (Exception e) {
                if (report) {
                    await
                        UserError.Throw(new InformationalUserError(e, "Error during processing repo from " + newUri,
                            null));
                } else
                    this.Logger().FormattedWarnException(e, "Failure during processing repo from " + newUri);
            }
        }

        async Task TryUpdateCustomRepoModSet(CustomCollection collection, bool report = true) {
            try {
                await UpdateCustomRepoModSet(collection).ConfigureAwait(false);
            } catch (Exception e) {
                if (report) {
                    await UserError.Throw(new InformationalUserError(e,
                        "Unhandled exception during processing of repository for " + collection.Name, null));
                } else {
                    this.Logger()
                        .FormattedWarnException(e,
                            "Unhandled exception during processing of repository for " + collection.Name);
                }
            }
        }

        async Task HandleCustomRepo(Uri uri, bool userInitiated = false) {
            this.Logger().Info("Processing CustomRepo " + Repository.ConfigFileName);

            var repo = await GetRepo(uri.GetParentUri().GetCleanedAuthlessUrl()).ConfigureAwait(false);

            if (!await RepoConfirmationDialog(repo)) {
                CustomRepositories.Remove(repo);
                return;
            }

            foreach (var repoServer in repo.Servers.Where(x => !x.Value.IsHidden))
                await HandleCustomRepoServer2(repoServer.Key, repo, userInitiated).ConfigureAwait(false);

            DomainEvilGlobal.Settings.RaiseChanged();
        }

        async Task HandleCustomRepoServer2(string serverKey, SixRepo repo, bool userInitiated = false) {
            var modSet =
                await
                    HandleCustomRepoServer(repo.GetUri(serverKey), userInitiated)
                        .ConfigureAwait(false);

            modSet.ServerKey = serverKey;

            if (modSet != null) {
                modSet.SetRemember();
                await UpdateCustomRepoModSet(modSet).ConfigureAwait(false);
            }
        }

        async Task UpdateCustomRepoModSet(CustomCollection customCollection) {
            var repo = customCollection.CustomRepo;
            if (repo == null)
                return;

            var url = customCollection.CustomRepoUrl;
            if (String.IsNullOrWhiteSpace(url))
                return;

            if (SixRepo.IsServerUrl(url)) {
                var urlInfo = SixRepo.GetUrlInfo(url);
                var serverName = urlInfo.Item2;

                var errorMessage = customCollection.RefreshRepoInfo(repo, serverName);
                if (!string.IsNullOrWhiteSpace(errorMessage)) {
                    await AskToRemoveModset(errorMessage, customCollection);
                    return;
                }
                if (errorMessage == "")
                    return;

                customCollection.ServerKey = serverName;

                var repoServer = repo.Servers[serverName];
                customCollection.UpdateCustomRepoServerInfo(this, repoServer, url, repo);
                var gameUuid = repoServer.GetGameUuid();
                var game = _gameContext.Games.FirstOrDefault(x => x.Id == gameUuid && x.SupportsMods());
                if (game != null)
                    customCollection.ChangeGame(game);

                if (String.IsNullOrWhiteSpace(customCollection.ImageLarge))
                    customCollection.ImageLarge = customCollection.Image;
                if (customCollection.Id != Guid.Empty) {
                    lock (CustomCollections)
                        CustomCollections.UpdateOrAdd(customCollection);
                }

                _evilGlobalSelectedGame.ActiveGame = (Game) customCollection.Game;
                SelectCollection(customCollection);

                var server = ServerList.FindOrCreateServer(repoServer.Address, true);
                server.ForceServerName = repoServer.ForceServerName;
                if (!server.Mods.Any())
                    UpdateServerMods(server, repoServer);

                if (string.IsNullOrWhiteSpace(server.Name) || server.ForceServerName)
                    server.Name = repoServer.Name;

                var pass = repoServer.Password;
                if (pass != null) {
                    server.SavePassword = true;
                    server.SavedPassword = pass;
                }

                repoServer.Server = server;
                await server.TryUpdateAsync().ConfigureAwait(false);
                Game.CalculatedSettings.Server = server;
            } else
                customCollection.UpdateCustomRepoInfo(this, url, repo);
        }

        static void UpdateServerMods(Server server, SixRepoServer repoServer) {
            var requiredMods =
                repoServer.RequiredMods.Where(x => !SixRepoServer.SYS.Contains(x)).DistinctBy(x => x.ToLower())
                    .ToArray();
            if (requiredMods.Any())
                server.UpdateModInfo(String.Join(";", requiredMods));
        }

        async Task AskToRemoveModset(string errorMessage, CustomCollection collection) {
            if (await
                _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams("Modset failed with this error: " + errorMessage,
                        "Do you want to remove faulty modset?", SixMessageBoxButton.YesNo)) == SixMessageBoxResult.Yes)
                RemoveCollection(collection);
        }

        async Task<CustomCollection> HandleCustomRepoServer(Uri uri, bool userinitiated = false) {
            this.Logger().Info("Processing CustomRepo server.yml");

            var authlessUrl = uri.GetCleanedAuthlessUrl().ToString();

            CustomCollection customModSet;
            lock (CustomCollections)
                customModSet =
                    CustomCollections.FirstOrDefault(x => x.CustomRepoUrl == authlessUrl);

            var currentModSet = Game.CalculatedSettings.Collection;

            if (customModSet != null)
                customModSet.UserInitiatedSync = userinitiated;

            if (customModSet != null && currentModSet != null) {
                await HandleExistingIfOneActive(uri, customModSet, currentModSet).ConfigureAwait(false);
                return customModSet;
            }

            if (customModSet == null || !customModSet.RememberWarn()) {
                SixRepo repo;
                if (customModSet != null)
                    repo = customModSet.CustomRepo ?? await GetRepo(uri.ProcessRepoUrl()).ConfigureAwait(false);
                else
                    repo = await GetRepo(uri.ProcessRepoUrl()).ConfigureAwait(false);
                await ConfirmRepoDialog(repo);
            }
            if (customModSet == null)
                customModSet = CreateAndAddCustomRepoModSet(authlessUrl);
            customModSet.SetRemember();
            DomainEvilGlobal.Settings.RaiseChanged();
            await ProcessCustomRepo(customModSet).ConfigureAwait(false);
            return customModSet;
        }

        async Task HandleExistingIfOneActive(Uri uri, CustomCollection customModSet, Collection currentModSet) {
            var repo = customModSet.CustomRepo ?? await GetRepo(uri.ProcessRepoUrl()).ConfigureAwait(false);
            if (!customModSet.RememberWarn())
                await ConfirmRepoDialog(repo);

            if (customModSet.Equals(currentModSet))
                await UpdateCustomRepoModSet(customModSet).ConfigureAwait(false);
            else {
                lock (CustomCollections)
                    CustomCollections.UpdateOrAdd(customModSet);
                SelectCollection(customModSet);
            }
        }

        async Task ConfirmRepoDialog(SixRepo repo) {
            var result = await RepoConfirmationDialog(repo);
            if (!result) {
                CustomRepositories.Remove(repo);
                throw new OperationCanceledException("The user cancelled the operation");
            }
        }

        async Task<bool> RepoConfirmationDialog(SixRepo repo) {
            var vm = new AddCustomRepoConfirmationViewModel(repo);
            var result = await _dialogManager.ShowDialogAsync(vm);
            return result.GetValueOrDefault();
        }

        async Task MakeCustomRepoShortcut(Uri uri) {
            var report = (await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                "Do you want to create a shortcut on your desktop for this custom repo for easy access?",
                "Create Shortcut", SixMessageBoxButton.YesNo))).IsYes();

            UsageCounter.ReportUsage("Dialog - create shortcut custom repo: {0}".FormatWith(report));

            if (report) {
                ShortcutCreator.CreateDesktopPwsIconCustomRepo(
                    "Play on CustomRepo server",
                    String.Format("Custom Repo: {0}\nCreated:{1}", uri, DateTime.Now),
                    uri);
            }
        }

        async Task<Uri> GetNewUri(Uri uri) {
            UsageCounter.ReportUsage("Dialog - Please enter username and password");
            var ev = await _dialogManager.UserNamePasswordDialogAsync("Please enter username and password",
                uri.AuthlessUri().ToString());
            if (!ev.Item3.GetValueOrDefault(false))
                throw new OperationCanceledException("The user aborted the process");
            _settings.AppOptions.SetAuthInfo(uri, ev.Item1, ev.Item2);
            return _authProvider.HandleUriAuth(uri, ev.Item1, ev.Item2);
        }

        async Task<SixRepo> LoadRemoteRepoCached(Uri uri, bool forceUpdate = true) {
            SixRepo repo;
            var key = uri.AuthlessUri().ToString();
            var repositories = _settings.ModOptions.Repositories;
            lock (repositories) {
                repo = repositories.ContainsKey(key)
                    ? repositories[key]
                    : null;

                if (repo == null) {
                    repo = new SixRepo(uri);
                    forceUpdate = true;
                    repositories[key] = repo;
                }
            }
            if (forceUpdate) {
                await repo.LoadConfigRemote(_stringDownloader).ConfigureAwait(false);
                lock (CustomRepositories) {
                    CustomRepositories.RemoveAll(r => r.Uri.Equals(repo.Uri));
                    CustomRepositories.Add(repo);
                }
            }
            repo.UpdateMods(Mods);
            return repo;
        }

        Task UpdateGameListsAsync(Game game) {
            return Task.Run(() => UpdateGameLists(game));
        }

        void UpdateGameLists(Game game) {
            if (game.SupportsMods())
                UpdateModdingLists(game);

            if (game.SupportsMissions())
                UpdateMissionLists(game);

            //game.InstalledModsCount = game.Mods.Count(x => x.Controller.IsInstalled);
            //game.InstalledMissionsCount = game.Missions.Count(x => x.Controller.IsInstalled);
        }

        void UpdateMissionLists(Game game) {
            lock (LocalMissionsContainers)
                game.Lists.CustomLocalMissionsContainers =
                    LocalMissionsContainers.Where(x => x.GameMatch(game)).ToArray();
            lock (Missions)
                game.Lists.Missions = Missions.Where(x => x.GameMatch(game)).ToArray();
        }

        void UpdateModdingLists(Game game) {
            var m = game.Modding();
            lock (Mods)
                game.Lists.Mods = Mods.Where(x => x.GameMatch(m)).ToArray();
            lock (Collections)
                game.Lists.Collections = Collections.Where(x => x.GameMatch(game)).ToArray();
            lock (CustomCollections)
                game.Lists.CustomCollections =
                    CustomCollections.Where(x => x.GameMatch(game)).ToArray();
            lock (LocalModsContainers)
                game.Lists.CustomLocalModContainers =
                    LocalModsContainers.Where(x => x.GameMatch(game)).ToArray();
        }

        Task PackMission(MissionFolder missionFolder, IAbsoluteDirectoryPath destination) {
            return PackMission(missionFolder.CustomPath.GetChildDirectoryWithName(missionFolder.FolderName), destination);
        }

        Task PackMission(IAbsoluteDirectoryPath folder, IAbsoluteDirectoryPath destination) {
            return Task.Run(() => _pboTools.CreateMissionPbo(folder, destination));
        }

        async Task PublishMission(Mission mission, string missionName) {
            _connectApi.ConfirmLoggedIn();

            var path = Path.Combine(mission.CustomPath.ToString(), mission.FileName);
            if (!SizeCheck(path))
                return;
            using (var session = await _connectApi.StartSession().ConfigureAwait(false)) {
                await _connectApi.UploadMission(new RequestMissionUploadModel {
                    FileName = mission.FileName,
                    Name = missionName,
                    GameSlug = _evilGlobalSelectedGame.ActiveGame.MetaData.Slug
                }, mission.CustomPath).ConfigureAwait(false);
                await session.Close().ConfigureAwait(false);
            }

            BrowserHelper.TryOpenUrlIntegrated(Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "me", "content"));
        }

        async Task PublishMission(MissionFolder missionFolder, string missionName) {
            var tempPath = Common.Paths.LocalDataPath.GetChildDirectoryWithName("temp");
            tempPath.MakeSurePathExists();

            await PackMission(missionFolder, tempPath).ConfigureAwait(false);
            var name = Path.GetFileNameWithoutExtension(missionFolder.FolderName);
            var mission = new LocalMission(Guid.Empty) {
                FileName = missionFolder.FolderName + ".pbo",
                FullName = Mission.NiceMissionName(name),
                Name = Mission.ValidMissionName(name),
                CustomPath = tempPath
            };

            try {
                await
                    PublishMission(mission, missionName).ConfigureAwait(false);
            } finally {
                Tools.FileUtil.Ops.DeleteWithRetry(Path.Combine(mission.CustomPath.ToString(), mission.FileName));
            }
        }

        bool SizeCheck(string item) {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Requires<ArgumentOutOfRangeException>(!string.IsNullOrWhiteSpace(item));

            var fi = new FileInfo(item);
            if (fi.Length < AttachmentCeil)
                return true;

            UsageCounter.ReportUsage("Dialog - File size is too large");
            _dialogManager.MessageBox(new MessageBoxDialogParams(
                String.Format("The file size is too large, max: {0}, current: {1}",
                    Tools.FileUtil.GetFileSize(AttachmentCeil),
                    Tools.FileUtil.GetFileSize(fi.Length))));

            return false;
        }

        static IEnumerable<MissionBase> GetMissions(string path) {
            return TryGetMissionFolder(path).Concat(GetMissionFiles(path));
        }

        static IEnumerable<MissionBase> GetMissionFiles(string path) {
            try {
                return
                    new DirectoryInfo(path).FilterDottedFiles()
                        .Select(x => TryGetMissionFile(x.FullName.ToAbsoluteFilePath()))
                        .Where(found => found != null);
            } catch (UnauthorizedAccessException ex) {
                MainLog.Logger.WarnException("Unable to read mission files from: " + path, ex);
                return Enumerable.Empty<MissionBase>();
            }
        }

        static MissionBase TryGetMissionFile(IAbsoluteFilePath file) {
            try {
                return file.FileName.EndsWith(".pbo")
                    ? CreateMission(file)
                    : null;
            } catch (Exception e) {
                MainLog.Logger.FormattedWarnException(e);
                return null;
            }
        }

        static Mission CreateMission(IAbsoluteFilePath file) {
            var missionName = Path.GetFileNameWithoutExtension(file.FileName);
            var name = Path.GetFileNameWithoutExtension(missionName);
            return new LocalMission(Guid.Empty) {
                Island = GetIslandName(missionName),
                FileName = file.FileName,
                Name = Mission.ValidMissionName(name),
                FullName = Mission.NiceMissionName(name),
                Size = file.FileInfo.Length,
                CustomPath = file.ParentDirectoryPath
            };
        }

        static IEnumerable<MissionBase> TryGetMissionFolder(string path) {
            try {
                return
                    new DirectoryInfo(path).FilterDottedDirectories()
                        .Select(x => TryGetMissionFolder(x.FullName.ToAbsoluteDirectoryPath()))
                        .Where(found => found != null);
            } catch (UnauthorizedAccessException ex) {
                MainLog.Logger.WarnException("Unable to read mission folders from: " + path, ex);
                return Enumerable.Empty<MissionBase>();
            }
        }

        static MissionBase TryGetMissionFolder(IAbsoluteDirectoryPath dir) {
            try {
                return File.Exists(Path.Combine(dir.ToString(), "mission.sqm"))
                    ? CreateMissionFolder(dir)
                    : null;
            } catch (Exception e) {
                MainLog.Logger.FormattedWarnException(e);
                return null;
            }
        }

        static MissionFolder CreateMissionFolder(IAbsoluteDirectoryPath dir) {
            return new MissionFolder(Guid.NewGuid()) {
                Name = Path.GetFileNameWithoutExtension(dir.DirectoryName),
                Island = GetIslandName(dir.DirectoryName),
                FolderName = dir.DirectoryName,
                CustomPath = dir.ParentDirectoryPath
            };
        }

        static string GetIslandName(string dirName) {
            var extension = Path.GetExtension(dirName);
            return string.IsNullOrWhiteSpace(extension) ? null : extension.Substring(1);
        }

        static IEnumerable<string> NonNullOrEmptyMods(IEnumerable<string> mods) {
            return mods.Where(x => !string.IsNullOrWhiteSpace(x));
        }

        static IMod FindModInLists(string mod, IReadOnlyCollection<IMod> items,
            IReadOnlyCollection<IMod> inputMods = null) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(mod));
            Contract.Requires<ArgumentNullException>(items != null);

            var lists = new[] {inputMods, items}
                .Where(x => x != null).ToArray();
            return FindByNameOrCppNameOrAliases(mod, lists);
        }

        IEnumerable<IMod> HandleDependencies(ISupportModding game, IEnumerable<IMod> mods,
            IReadOnlyCollection<IMod> inputMods = null) {
            var modList = new List<IMod>();
            mods.ForEach(mod => HandleDependencies(game, mod, modList, inputMods));

            return
                modList.DistinctBy(x => x.Name.ToLower());
        }

        static IMod FindByNameOrCppNameOrAliases(string mod, IReadOnlyCollection<IMod>[] lists) {
            return FindByName(mod, lists).FirstOrDefault()
                   ?? FindByCppName(mod, lists).FirstOrDefault()
                   ?? FindByAlias(mod, lists).FirstOrDefault();
        }

        static IEnumerable<IMod> FindByName(string mod, IEnumerable<IReadOnlyCollection<IMod>> lists) {
            return lists.Select(ml => ml.FirstOrDefault(x =>
                x.Name.Equals(mod, StringComparison.OrdinalIgnoreCase))).Where(m => m != null);
        }

        static IEnumerable<IMod> FindByCppName(string mod, IEnumerable<IReadOnlyCollection<IMod>> lists) {
            return lists.Select(ml => ml.FirstOrDefault(x =>
                !String.IsNullOrWhiteSpace(x.CppName)
                && x.CppName.Equals(mod, StringComparison.OrdinalIgnoreCase))).Where(m => m != null);
        }

        static IEnumerable<IMod> FindByAlias(string mod, IEnumerable<IReadOnlyCollection<IMod>> lists) {
            return lists.Select(ml => ml.FirstOrDefault(x =>
                x.Aliases.ContainsIgnoreCase(mod))).Where(m => m != null);
        }

        void HandleDependencies(ISupportModding game, IMod mod, ICollection<IMod> modList,
            IReadOnlyCollection<IMod> inputMods = null) {
            modList.Add(mod);

            var deps = mod.Dependencies;
            if (deps == null || !deps.Any())
                return;

            FindOrCreateLocalMods(game, deps.ToArray(), inputMods)
                .Where(x => !modList.Select(y => y.Name).ContainsIgnoreCase(x.Name))
                .ForEach(x => HandleDependencies(game, x, modList, inputMods));

            modList.Remove(mod);
            modList.Add(mod);
        }

        async Task UpdateContentStates() {
            var game = Game;
            if (game.SupportsMods()) {
                var modding = game.Modding();
                await Task.Factory.StartNew(() => {
                    lock (_updateAllModsLock)
                        modding.UpdateModStates(game.Lists.Mods);
                }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
            }

            if (game.SupportsMissions()) {
                var missions = game.Missions();
                await Task.Factory.StartNew(() => {
                    lock (_updateAllModsLock)
                        missions.UpdateMissionStates(game.Lists.Missions);
                }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
            }
        }

        IReadOnlyCollection<Collection> GetAllCollectionsFor(Game game) {
            return game.SupportsMods()
                ? GetAllSupportedCollections(game)
                : Enumerable.Empty<Collection>().ToArray();
        }

        Collection[] GetAllSupportedCollections(Game game) {
            lock (Collections)
                lock (CustomCollections)
                    lock (SubscribedCollections)
                        return AllCollectionsInternal().Where(x => x.GameMatch(game)).ToArray();
        }

        IEnumerable<Collection> AllCollectionsInternal() {
            return Collections.Concat(CustomCollections).Concat(SubscribedCollections);
        }

        async Task UpdateCollectionLists() {
            await UpdateSynq().ConfigureAwait(false);

            UpdateCollections();

            var items = GetAllCollectionsFor(Game);
            using (this.Bench("Updating HandleModSetMods"))
                items.ForEach(x => x.Process(this));

            using (this.Bench("Updating ModSet states"))
                items.ForEach(x => x.UpdateState());
        }

        async Task UpdateSynq() {
            try {
                await TryUpdateSynq().ConfigureAwait(false);
            } catch (IOException e) {
                await UserError.Throw(new InformationalUserError(e,
                    "Failure during processing of Synq data, please correct the problem.\nIf you feel this is a bug, please report the issue to Support",
                    null));
            } catch (NoHostsAvailableException e) {
                await UserError.Throw(new InformationalUserError(e,
                    "Could not connect to API; No hosts available. Try adjusting Protocol Preference in the settings either to 'Any', or 'Prefer x'",
                    null));
            } catch (Exception e) {
                await
                    UserError.Throw(new InformationalUserError(e, "An error occurred during processing of Synq data",
                        null));
            }
        }

        Game GetDesiredGame(SixRepoServer repoServer) {
            var gameId = repoServer.GetGameUuid();
            var game = _gameContext.Games.Find(gameId) ?? Game;
            var modding = game as ISupportModding;
            if (modding == null) {
                throw new DesiredGameDoesNotSupportModsException("Requested game does not support mods: " +
                                                                 game.MetaData.Name + ", ID: " + game.Id);
            }
            return game;
        }

        Game FindByMod(IMod mod) {
            return
                _gameContext.Games.OrderByDescending(x => x.MetaData.ReleasedOn)
                    .FirstOrDefault(x => x.SupportsMods() && mod.GameMatch(x.Modding()));
        }

        void HandleExistingCol(CustomCollection col, CustomCollection existingCol) {
            col.CustomRepo = existingCol.CustomRepo;
            col.CustomRepoApps = existingCol.CustomRepoApps;
            col.CustomRepoMods = existingCol.CustomRepoMods;
            col.CustomRepoUrl = existingCol.CustomRepoUrl;
            col.CustomRepoUuid = existingCol.CustomRepoUuid;
        }

        public CustomCollection CreateCustomCollection(ISupportModding game, IReadOnlyCollection<IContent> content) {
            var name = GetSuggestedCollectionName(content.First());
            var ms = content.Any(x => x is Collection);
            if (ms)
                throw new Exception("Selection contains a collection.");
            var modSet = new CustomCollection(Guid.NewGuid(), game) {
                Name = name
            };
            var mod = content.First().ToMod();
            if (mod != null) {
                modSet.UpdateFromMod(mod);
                modSet.Image = mod.Image;
                modSet.ImageLarge = mod.ImageLarge;
                modSet.AddModAndUpdateStateIfPersistent(mod, this);
            }

            content.Skip(1).Each((x, i) => modSet.AddModAndUpdateStateIfPersistent(x.ToMod(), this));

            return modSet;
        }

        CustomCollection CreateAndAddCustomModSet(IContent content = null) {
            var game = Game;
            var mod = content.ToMod();
            if (mod != null) {
                if (!mod.GameMatch(Game.Modding())) {
                    var g = _gameContext.Games.FirstOrDefault(x => x.SupportsMods() && mod.GameMatch(x.Modding()));
                    if (g != null) {
                        game = g;
                        _evilGlobalSelectedGame.ActiveGame = game;
                    }
                }
            }

            var collection = CreateCustomCollection(game.Modding(), content);
            lock (CustomCollections)
                CustomCollections.Add(collection);
            return collection;
        }

        CustomCollection CreateAndAddCustomModSet(IReadOnlyCollection<IContent> content) {
            var game = Game;
            var mod = content.ToMod();
            if (mod != null) {
                if (!mod.GameMatch(Game.Modding())) {
                    var g = _gameContext.Games.FirstOrDefault(x => x.SupportsMods() && mod.GameMatch(x.Modding()));
                    if (g != null) {
                        game = g;
                        _evilGlobalSelectedGame.ActiveGame = game;
                    }
                }
            }

            var collection = CreateCustomCollection(game.Modding(), content);
            lock (CustomCollections)
                CustomCollections.Add(collection);
            return collection;
        }

        CustomCollection CreateAndAddCustomRepoModSet(string url) {
            var modSet = CreateCustomCollection(Game.Modding());
            modSet.Name = "Placeholder for repo";
            modSet.CustomRepoUrl = url;
            CustomCollections.Add(modSet);

            return modSet;
        }

        void SelectLastUsedIfNonSet() {
            var gameSet = Game;
            if (gameSet.CalculatedSettings.Collection != null)
                return;
            SelectLastUsed();
        }

        void SelectLastUsed() {
            var gameSet = Game;
            var collection = gameSet.Settings.Recent.Collection;

            if (collection == null) {
                SelectCollection(null);
                return;
            }

            var ms = Find(collection.Matches);
            SelectCollection(ms != null && ms.GameMatch(gameSet) ? ms : null);
        }

        Collection Find(Predicate<Collection> predicate) {
            lock (Collections)
                lock (CustomCollections)
                    lock (SubscribedCollections)
                        return
                            AllCollectionsInternal().FirstOrDefault(x => predicate(x));
        }

        CustomCollection CreateCustomCollection(Server server) {
            Contract.Requires<ArgumentNullException>(server != null);
            var name = String.Format("Collection ({0})", server.Name);
            var modSet = new CustomCollection(Guid.NewGuid(), Game.Modding()) {
                Name = name,
                AdditionalMods = server.Mods.ToList()
            };
            return modSet;
        }

        long GetNewCollectionId() {
            return GetAllCollectionsFor(Game)
                .Select(x => rx.Match(x.Name))
                .Where(x => x.Success)
                .Select(x => x.Groups[1].Value.TryInt())
                .OrderBy(x => x)
                .LastOrDefault()
                   + 1;
        }

        async Task TryUpdateSynq() {
            await _syncLock.WaitAsync().ConfigureAwait(false);
            var initial = _initial;
            _initial = false;
            try {
                var ex =
                    await Game.UpdateSynq(this, _systemInfo.IsInternetAvailable && !initial).ConfigureAwait(false);
                // TODO: Improve multi handling
                foreach (var e in ex) {
                    await
                        UserError.Throw(new InformationalUserError(e, "A problem occurred during updating remotes", null));
                }
            } finally {
                _syncLock.Release();
            }
        }

        void UpdateCollections() {
            using (this.Bench()) {
                UpdateCustomModSets();
                // customModSets.Where(x => x.SubscribedAccountId == _settings.AccountOptions.AccountId) // TODO: Filter
            }
        }

        void UpdateCustomModSets() {
            IReadOnlyCollection<CustomCollection> customModSets;
            lock (CustomCollections)
                customModSets =
                    CustomCollections.Where(x => !String.IsNullOrWhiteSpace(x.CustomRepoUrl) && x.CustomRepo == null)
                        .ToList();

            customModSets.ForEach(x => {
                var urlInfo = SixRepo.GetUrlInfo(x.CustomRepoUrl);
                var repo = GetRemoteRepoCached(new Uri(urlInfo.Item1));
                x.CustomRepo = repo;
                if (repo != null) {
                    repo.UpdateMods(Mods); // TODO
                    x.CustomRepoMods = repo.Mods;
                }
            });
        }

        SixRepo GetRemoteRepoCached(Uri uri) {
            var key = uri.AuthlessUri().ToString();
            // TODO CLEANUP
            var repositories = _settings.ModOptions.Repositories;
            lock (repositories) {
                return repositories.ContainsKey(key)
                    ? repositories[key]
                    : null;
            }
        }

        void RemoveOnlineCollections() {
            lock (SubscribedCollections)
                SubscribedCollections.Clear();

            lock (CustomCollections) {
                var shared = CustomCollections.Where(x => x.PublishedId != null && x.PublishedId != Guid.Empty).ToArray();
                foreach (var customCollection in shared)
                    RemoveCollection(customCollection);
            }
            //Items.Reset();
        }

        public class FakeContent : IContent
        {
            readonly Uri _profileUrl;

            public FakeContent(string name, Uri profileUrl) {
                _profileUrl = profileUrl;
                Name = name;
            }

            public string NoteName { get; private set; }
            public bool HasNotes { get; private set; }
            public string Notes { get; set; }
            public bool IsFavorite { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public bool ComparePK(object other) {
                return false;
            }

            public bool ComparePK(SyncBase other) {
                return false;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public int SearchScore { get; set; }
            public Guid Id { get; set; }
            public string Version { get; set; }
            public string Name { get; set; }
            public string[] Categories { get; set; }
            public string HomepageUrl { get; set; }
            public bool HasImage { get; set; }
            public string Image { get; set; }
            public string ImageLarge { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public ContentState State { get; set; }
            public bool IsCustomContent
            {
                get { return false; }
            }

            public Uri ProfileUrl() {
                return _profileUrl;
            }

            public Uri GetChangelogUrl() {
                throw new NotImplementedException();
            }

            public void ToggleFavorite() {
                IsFavorite = !IsFavorite;
            }

            #region IHierarchicalLibraryItem

            public ReactiveList<IHierarchicalLibraryItem> Children { get; private set; }
            public ICollectionView ChildrenView { get; private set; }
            public IHierarchicalLibraryItem SelectedItem { get; set; }
            public ObservableCollection<object> SelectedItemsInternal { get; set; }

            public void ClearSelection() {
                throw new NotImplementedException();
            }

            object IHaveSelectedItem.SelectedItem
            {
                get { return SelectedItem; }
                set { SelectedItem = (IHierarchicalLibraryItem) value; }
            }
            public ICollectionView ItemsView { get; private set; }

            #endregion
        }

        static class PwsUrlActions
        {
            public const string Update = "update";
            public const string Install = "install";
            public const string Join = "join";
            public const string Missions = "missions";
            public const string MPMissions = "mpmissions";
            public const string Apps = "apps";
            public const string Shutdown = "shutdown";
            public const string Launch = "launch";
        }

        static class PwsUrlAfterActions
        {
            public const string PublishMission = "publishmission";
            public const string Mission = "mission";
            public const string MissionPackage = "mission_package";
            public const string Server = "server";
            public const string Ts3 = "ts3";
            public const string Action = "action";
            public const string CollectionId = "c";
            public const string Share = "share";
        }

        static class PwsUrlPreActions
        {
            public const string Game = "game";
            public const string Mod = "mod";
            public const string ModSet = "mod_set";
            public const string Collection = "c_action";
        }

        class UrlHandlerState
        {
            public UrlHandlerState(string param) {
                Param = param;
                var str = FixOldStyleUrl(param);
                Uri = new Uri(str);
                Dic = Tools.Transfer.GetDictionaryFromQueryString(Uri.Query);
            }

            public string Param { get; }
            public Dictionary<string, string> Dic { get; }
            public Uri Uri { get; }
            public bool Subscribe { get; set; }
        }

        #region IHandle events

        public void Handle(ApiKeyUpdated message) {
            if (message.ApiKey.IsBlankOrWhiteSpace())
                RemoveOnlineCollections();
        }

        public void Handle(GameContentAfterSynced message) {
            SelectLastUsedIfNonSet();
        }

        public async void Handle(SynqPathChanged message) {
            await UpdateSynq().ConfigureAwait(false);
        }

        public async Task Handle(ActiveGameChanged message) {
            SelectLastUsed();
            await AnnounceSyncFinished().ConfigureAwait(false);
            HandleServerList(message.Game);
        }

        public async void Handle(GamePathChanged message) {
            await UpdateContentStates().ConfigureAwait(false);
            _updateManager.Value.RefreshModInfo();
        }

        public async void Handle(ModPathChanged message) {
            await UpdateCollectionLists().ConfigureAwait(false);
            await UpdateContentStates().ConfigureAwait(false);
            _updateManager.Value.RefreshModInfo();
        }

        public async Task Handle(SubGamesChanged message) {
            await UpdateSynq().ConfigureAwait(false);
            await UpdateContentStates().ConfigureAwait(false);
            UpdateGameLists(message.Game);
        }

        #endregion
    }

    public class ShareToContactEvent
    {
        public ShareToContactEvent(IContent content) {
            Content = content;
        }

        public IContent Content { get; set; }
    }

    public class CollectionCreatedEvent : IDomainEvent
    {
        public CollectionCreatedEvent(CustomCollection collection) {}
    }

    class DesiredGameDoesNotSupportModsException : Exception
    {
        public DesiredGameDoesNotSupportModsException(string message) : base(message) {}
    }
}