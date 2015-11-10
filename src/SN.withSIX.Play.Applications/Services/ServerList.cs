// <copyright company="SIX Networks GmbH" file="ServerList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using MoreLinq;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Legacy.Servers;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.Services
{
    public class ServerList : HaveReactiveItems<Server>, IServerList, IEnableLogging
    {
        const int ServerUpdateFrequency = 20*1000;
        readonly object _abortLock = new object();
        readonly object _dlLock = new Object();
        readonly IEventAggregator _eventBus;
        readonly IGameContext _gameContext;
        readonly TimerWithElapsedCancellationAsync _listTimer;
        readonly IGameServerQueryHandler _queryHandler;
        readonly TimeSpan _recentTimeSpan = new TimeSpan(90, 0, 0, 0);
        readonly TimerWithElapsedCancellationAsync _serverTimer;
        readonly UserSettings _settings;
        readonly object _updateLock = new Object();
        CancellationTokenSource _cancellationToken;
        bool _downloadingServerList;
        bool _initialSync;
        bool _isUpdating;
        DateTime _lastTimeDownloadedServerList = Tools.Generic.GetCurrentUtcDateTime;
        IServerQueryQueue _serverQueryQueue;
        DateTime _synchronizedAt;
        int _totalCount;

        public ServerList(ISupportServers game, UserSettings settings, IEventAggregator eventBus,
            IGameServerQueryHandler queryHandler, IGameContext gameContext) {
            _eventBus = eventBus;
            _queryHandler = queryHandler;
            _gameContext = gameContext;
            _settings = settings;
            _listTimer = new TimerWithElapsedCancellationAsync(GetTimerValue(), ListTimerElapsed, null, false);
            _serverTimer = new TimerWithElapsedCancellationAsync(ServerUpdateFrequency, ServerTimerElapsed);
            Game = game;
            Filter = Game.GetServerFilter();
            ServerQueryQueue = new ServerQueryQueue();
        }

        public int TotalCount
        {
            get { return _totalCount; }
            private set { SetProperty(ref _totalCount, value); }
        }
        public IFilter Filter { get; }
        public IServerQueryQueue ServerQueryQueue
        {
            get { return _serverQueryQueue; }
            private set { SetProperty(ref _serverQueryQueue, value); }
        }
        public ISupportServers Game { get; }
        public bool InitialSync
        {
            get { return _initialSync; }
            private set { SetProperty(ref _initialSync, value); }
        }
        public bool DownloadingServerList
        {
            get { return _downloadingServerList; }
            set { SetProperty(ref _downloadingServerList, value); }
        }
        public bool IsUpdating
        {
            get { return _isUpdating; }
            private set { SetProperty(ref _isUpdating, value); }
        }
        public DateTime SynchronizedAt
        {
            get { return _synchronizedAt; }
            set { SetProperty(ref _synchronizedAt, value); }
        }

        public Server FindOrCreateServer(ServerAddress address, bool isFavorite = false) {
            var server = FindServer(address);
            if (server != null)
                return server;

            server = Game.CreateServer(address);
            server.IsFavorite = isFavorite;
            AddServer(server);

            return server;
        }

        public void AbortSync() {
            CancellationTokenSource token;
            lock (_abortLock) {
                token = _cancellationToken;
                if (token == null)
                    return;
                _cancellationToken = null;
            }

            try {
                using (token)
                    token.Cancel();
            } catch (ObjectDisposedException) {}
        }

        public async Task GetAndUpdateAll(bool updateOnlyWhenActive = false, bool forceLocal = false) {
            await GetAll(forceLocal).ConfigureAwait(false);
            var items = Items.ToArray();

            if (!updateOnlyWhenActive || _settings.AppOptions.ServerListEnabled) {
                if (updateOnlyWhenActive)
                    await WaitForInitialSync().ConfigureAwait(false);
                _eventBus.PublishOnCurrentThread(new ServersAdded(items));
                if (!forceLocal)
                    await UpdateAll().ConfigureAwait(false);
            } else
                _eventBus.PublishOnCurrentThread(new ServersAdded(items));
        }

        public Task UpdateServers() {
            if (!TryLock())
                return TaskExt.Default;

            return (Tools.Generic.LongerAgoThan(_lastTimeDownloadedServerList, new TimeSpan(0, 30, 0))
                ? GetAndUpdateAll()
                : TryUpdateAllInternal());
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        Server FindServer(ServerAddress address) {
            lock (Items)
                return Items.FirstOrDefault(x => x.Address.Equals(address));
        }

        void AddServer(Server server) {
            lock (Items)
                Items.Add(server);
            _eventBus.PublishOnCurrentThread(new ServersAdded(new[] {server}));
        }

        async Task<IReadOnlyCollection<Server>> GetServers(bool forceLocal = false, int limit = 0) {
            return ProcessServers(await Game.QueryServers(_queryHandler).ConfigureAwait(false));
        }

        Task UpdateAll() {
            return !TryLock() ? TaskExt.Default : TryUpdateAllInternal();
        }

        bool TryLock() {
            if (!InitialSync)
                return false;

            lock (_updateLock) {
                if (_isUpdating)
                    return false;
                try {
                    _listTimer.Stop();
                } catch (ObjectDisposedException) {
                    return false;
                }
                IsUpdating = true;
            }
            return true;
        }

        async Task GetAll(bool forceLocal = false, int limit = 0) {
            lock (_dlLock) {
                if (DownloadingServerList)
                    return;
                DownloadingServerList = true;
            }

            await TryGetAll(forceLocal, limit).ConfigureAwait(false);
            _lastTimeDownloadedServerList = Tools.Generic.GetCurrentUtcDateTime;
        }

        async Task TryGetAll(bool forceLocal, int limit) {
            try {
                var servers = await GetServers(forceLocal, limit).ConfigureAwait(false);
                servers.SyncCollectionLocked(Items);
                if (!InitialSync)
                    InitialSync = true;
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            } finally {
                DownloadingServerList = false;
            }
        }

        async Task TryUpdateAllInternal() {
            try {
                await UpdateAllInternal().ConfigureAwait(false);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            } finally {
                SynchronizedAt = Tools.Generic.GetCurrentUtcDateTime;
                UpdateInterval();
                IsUpdating = false;
            }
        }

        async Task<bool> ServerTimerElapsed() {
            var game = (Game) Game;
            var server = game.CalculatedSettings.Server;
            if (server == null || IsUpdating)
                return true;
            var timer = _serverTimer;
            if (timer != null)
                timer.Stop();

            await server.TryUpdateAsync().ConfigureAwait(false);

            var queued = game.CalculatedSettings.Queued;
            if (queued == null || queued == server)
                return true;

            server = queued;
            await server.TryUpdateAsync().ConfigureAwait(false);

            return true;
        }

        double GetTimerValue() {
            long amount = 1;
            var value = _settings.AppOptions.AutoRefreshServersTime;
            if (value > 0)
                amount = value;

            return amount*60*1000;
        }

        async Task WaitForInitialSync() {
            var game = (Game) Game;
            while (!game.CalculatedSettings.InitialSynced)
                await Task.Delay(150).ConfigureAwait(false);
        }

        IReadOnlyCollection<Server> ProcessServers(IEnumerable<ServerQueryResult> list) {
            var timeAgo = Tools.Generic.GetCurrentUtcDateTime.Subtract(_recentTimeSpan);
            var servers = Items.ToList();
            ProcessFavorites(servers);
            ProcessRecentServers(timeAgo, servers);
            servers = servers.Where(x => x != null && x.Address != null)
                .DistinctBy(x => x.Address).ToList();

            if (list == null)
                return servers;

            foreach (var s in list)
                UpdateOrAddServer(servers, s);

            return servers;
        }

        void UpdateOrAddServer(ICollection<Server> servers, ServerQueryResult s) {
            var server = servers.FirstOrDefault(x => x.Address.ToString().Equals(s.Settings["address"]));
            if (server == null)
                servers.Add(server = Game.CreateServer(new ServerAddress(s.Settings["address"])));
            server.UpdateInfoFromSettings(s);
        }

        void ProcessRecentServers(DateTime timeAgo, IList<Server> servers) {
            var recentList =
                _settings.ServerOptions.Recent.Where(x => x.On > timeAgo)
                    .DistinctBy(x => x.Address)
                    .Where(recent => Game.SupportsServerType(recent.GameName) && servers.None(recent.Matches))
                    .OrderByDescending(x => x.On)
                    .ToArray();

            servers.AddRange(recentList.Select(x => Server.FromStored(Game, x)));
        }

        void ProcessFavorites(IList<Server> servers) {
            var favoriteList =
                _settings.ServerOptions.Favorites.DistinctBy(x => x.Address)
                    .Where(
                        favorite =>
                            Game.SupportsServerType(favorite.GameName) && servers.None(favorite.Matches))
                    .ToArray();

            servers.AddRange(favoriteList.Select(x => Server.FromStored(Game, x)));
        }

        async Task UpdateAllInternal() {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            var queue = new ServerQueryQueue();
            ServerQueryQueue = queue;

            try {
                await SyncAndProcess().ConfigureAwait(false);
            } finally {
#if DEBUG
                sw.Stop();
                this.Logger().Debug(
                    "Done processing servers: {0} (Canceled: {1}, TotalSI: {2}). Took: {3}s",
                    queue.State.Progress, queue.State.Canceled,
                    TotalCount,
                    sw.Elapsed.TotalSeconds);
#endif
                _eventBus.PublishOnCurrentThread(new ServersUpdated());
            }
        }

        async Task SyncAndProcess() {
            var ms = ((Game) Game).CalculatedSettings.Collection;
            var moddingFilter = Filter as IHaveModdingFilters;
            var servers = moddingFilter == null ||
                          (ms == null && !moddingFilter.Modded && !moddingFilter.IncompatibleServers)
                ? Items.ToArray()
                : Items.Where(server => server.HasMod(ms, moddingFilter.Modded, moddingFilter.IncompatibleServers))
                    .ToArray();
            if (!servers.Any())
                return;
            using (_cancellationToken = new CancellationTokenSource()) {
                await ServerQueryQueue.SyncAsync(servers, _cancellationToken.Token).ConfigureAwait(false);
                _cancellationToken = null;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                _listTimer.Dispose();
                _serverTimer.Dispose();
                AbortSync();
            }
            // free native resources
        }

        async Task<bool> ListTimerElapsed() {
            try {
                if (IsRefreshSuspended())
                    return true;
                await UpdateServers().ConfigureAwait(false);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }

            return true;
        }

        bool IsRefreshSuspended() {
            if (!_settings.AppOptions.ServerListEnabled)
                return true;

            if (_settings.AppOptions.AutoRefreshServersTime <= 0)
                return true;

            return (_settings.AppOptions.SuspendSyncWhileGameRunning)
                   && AnyGamesRunning();
        }

        bool AnyGamesRunning() {
            var processes = _gameContext.Games.RunningProcesses().ToArray();
            using (new CompositeDisposable(processes.Cast<IDisposable>()))
                return processes.Any();
        }

        void UpdateInterval() {
            try {
                TryUpdateInterval();
            } catch (ObjectDisposedException) {}
        }

        void TryUpdateInterval() {
            _listTimer.Stop();
            var i = GetTimerValue();
            _listTimer.Interval = i;
            _listTimer.Start();
        }
    }
}