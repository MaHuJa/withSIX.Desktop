// <copyright company="SIX Networks GmbH" file="SyncCollections.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class SyncCollections : IAsyncVoidCommand, IHaveGameId
    {
        public SyncCollections(Guid gameId, List<ContentGuidSpec> contents) {
            GameId = gameId;
            Contents = contents;
        }

        public List<ContentGuidSpec> Contents { get; }
        public Guid GameId { get; }
    }

    public class SyncCollectionsHandler : DbCommandBase, IAsyncVoidCommandHandler<SyncCollections>
    {
        readonly INetworkContentSyncer _contentSyncer;

        public SyncCollectionsHandler(IDbContextLocator dbContextLocator, INetworkContentSyncer contentSyncer)
            : base(dbContextLocator) {
            _contentSyncer = contentSyncer;
        }

        public async Task<UnitType> HandleAsync(SyncCollections request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);

            await DealWithCollections(game, request.Contents).ConfigureAwait(false);

            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }

        // TODO: Domain call instead?
        async Task DealWithCollections(Game game, IEnumerable<ContentGuidSpec> contents) {
            var ids = contents.Select(x => x.Id).ToArray();
            var existingCollections = game.SubscribedCollections.Where(x => ids.Contains(x.Id)).ToList();
            var newCollectionIds =
                ids.Except(existingCollections.Select(x => x.Id)).ToList();

            // TODO: Multi-game content??
            var networkContents = game.NetworkContent.ToArray();
            await
                _contentSyncer.SyncCollections(existingCollections, networkContents).ConfigureAwait(false);
            if (newCollectionIds.Any()) {
                var newCollections =
                    await
                        _contentSyncer.GetCollections(game.Id, newCollectionIds, networkContents).ConfigureAwait(false);
                game.Contents.AddRange(newCollections);
            }
        }
    }
}