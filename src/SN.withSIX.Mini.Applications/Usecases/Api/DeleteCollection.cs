// <copyright company="SIX Networks GmbH" file="DeleteCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class DeleteCollection : IAsyncVoidCommand, IHaveId<Guid>, IHaveGameId
    {
        public DeleteCollection(Guid gameId, Guid id) {
            GameId = gameId;
            Id = id;
        }

        public Guid GameId { get; }
        public Guid Id { get; }
    }

    public class DeleteCollectionHandler : DbCommandBase, IAsyncVoidCommandHandler<DeleteCollection>
    {
        public DeleteCollectionHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(DeleteCollection request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            game.RemoveCollection(game.Collections.FindFromRequest(request));

            await GameContext.SaveChanges().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}