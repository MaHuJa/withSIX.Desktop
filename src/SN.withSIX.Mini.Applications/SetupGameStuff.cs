// <copyright company="SIX Networks GmbH" file="SetupGameStuff.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications
{
    public interface ISetupGameStuff
    {
        Task Initialize();
        Task HandleGameContentsWhenNeeded();
    }

    public class SetupGameStuff : IApplicationService, ISetupGameStuff
    {
        readonly IDbContextLocator _gameContextFactory;
        readonly INetworkContentSyncer _networkContentSyncer;

        public SetupGameStuff(IDbContextLocator gameContextFactory, INetworkContentSyncer networkContentSyncer) {
            _gameContextFactory = gameContextFactory;
            _networkContentSyncer = networkContentSyncer;
        }

        public static List<Guid> GameIds { get; private set; }

        public async Task Initialize() {
            await Migrate().ConfigureAwait(false);
        }

        public async Task HandleGameContentsWhenNeeded() {
            var settingsCtx = _gameContextFactory.GetSettingsContext();
            var hashes = await _networkContentSyncer.GetHashes().ConfigureAwait(false);
            var localHashes = settingsCtx.Settings.Local.ApiHashes;

            var shouldSyncBecauseTime = settingsCtx.Settings.Local.LastSync.ToLocalTime() <
                                        Tools.Generic.GetCurrentUtcDateTime.ToLocalTime()
                                            .Subtract(TimeSpan.FromMinutes(10));

            var shouldSyncBecauseHashes = localHashes == null || localHashes.Mods != hashes.Mods;

            await HandleGameContents(new HashStats {
                Hashes = hashes,
                ShouldSyncBecauseHashes = shouldSyncBecauseHashes,
                ShouldSyncBecauseTime = shouldSyncBecauseTime
            }).ConfigureAwait(false);
            settingsCtx.Settings.Local.LastSync = Tools.Generic.GetCurrentUtcDateTime;
            settingsCtx.Settings.Local.ApiHashes = hashes;
            await settingsCtx.SaveSettings().ConfigureAwait(false);
        }

        async Task Migrate() {
            var gc = _gameContextFactory.GetGameContext();
            var gameSpecs = GameFactory.GetGameTypesWithAttribute().ToArray();
            GameIds = gameSpecs.Select(x => x.Value.Id).ToList();
            await gc.Migrate().ConfigureAwait(false);
            //await Task.Run(() => gc.Migrate()).ConfigureAwait(false);
            await HandleMissingGames(gc, gameSpecs).ConfigureAwait(false);
        }

        static async Task HandleMissingGames(IGameContext gc, IEnumerable<KeyValuePair<Type, GameAttribute>> gameSpecs) {
            await gc.LoadAll(true).ConfigureAwait(false);
            var newGames = gameSpecs
                .Where(x => !gc.Games.Select(g => g.Id).Contains(x.Value.Id))
                .Select(x => GameFactory.CreateGame(x.Key, x.Value))
                .ToArray();
            foreach (var ng in newGames)
                gc.Games.Add(ng);
            if (newGames.Any())
                await gc.SaveChanges().ConfigureAwait(false);
        }

        async Task HandleGameContents(HashStats hashStats) {
            await new StatusChanged(Status.Preparing).RaiseEvent().ConfigureAwait(false);
            try {
                await TryHandleGameContents(hashStats).ConfigureAwait(false);
            } finally {
                await new StatusChanged(Status.Synchronized).RaiseEvent().ConfigureAwait(false);
            }
        }

        async Task TryHandleGameContents(HashStats hashStats) {
            var gc = _gameContextFactory.GetGameContext();
            await gc.LoadAll().ConfigureAwait(false);

            if (hashStats.ShouldSyncBecauseHashes)
                await SynchronizeContent(gc.Games, hashStats.Hashes).ConfigureAwait(false);
            await new StatusChanged(Status.Preparing, 50).RaiseEvent().ConfigureAwait(false);
            await SynchronizeCollections(gc.Games).ConfigureAwait(false);
            await gc.SaveChanges().ConfigureAwait(false);
        }

        Task SynchronizeContent(ICollection<Game> games, ApiHashes hashes) {
            return _networkContentSyncer.SyncContent(games.ToArray(), hashes);
        }

        Task SynchronizeCollections(ICollection<Game> games) {
            var contents = games.SelectMany(x => x.Contents).OfType<NetworkContent>().Distinct().ToArray();
            return
                _networkContentSyncer.SyncCollections(games.SelectMany(x => x.SubscribedCollections).ToArray(),
                    contents, false);
        }

        public class HashStats
        {
            public ApiHashes Hashes { get; set; }
            public bool ShouldSyncBecauseHashes { get; set; }
            public bool ShouldSyncBecauseTime { get; set; }
        }

        static class GameFactory
        {
            static readonly Type gameType = typeof (Game);

            public static Game CreateGame(Type type, GameAttribute attr) {
                return (Game) Activator.CreateInstance(type, attr.Id, CreateGameSettings(type));
            }

            static GameSettings CreateGameSettings(Type gt) {
                return (GameSettings) Activator.CreateInstance(GetSettingsModelType(gt));
            }

            static Type GetSettingsModelType(Type x) {
                var typeName = MapToSettingsType(x);
                var type = x.Assembly.GetType(typeName);
                if (type == null)
                    throw new InvalidOperationException("Cannot find the SettingsModelType required for " + x);
                return type;
            }

            static string MapToSettingsType(Type x) {
                return x.FullName.Replace("Game", "GameSettings");
            }

            public static IEnumerable<KeyValuePair<Type, GameAttribute>> GetGameTypesWithAttribute() {
                return FindGameTypes().ToDictionary(x => x, x => x.GetCustomAttribute<GameAttribute>());
            }

            static IEnumerable<Type> FindGameTypes() {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(IsGameType);
            }

            static bool IsGameType(Type x) {
                return !x.IsInterface && !x.IsAbstract && gameType.IsAssignableFrom(x) &&
                       x.GetCustomAttribute<GameAttribute>() != null;
            }
        }
    }
}