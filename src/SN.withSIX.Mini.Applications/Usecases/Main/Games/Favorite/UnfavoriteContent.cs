// <copyright company="SIX Networks GmbH" file="UnfavoriteContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games.Favorite
{
    public class UnfavoriteContent : IAsyncVoidCommand, IHaveId<Guid>, IHaveGameId
    {
        public UnfavoriteContent(Guid gameId, Guid id) {
            GameId = gameId;
            Id = id;
        }

        public Guid GameId { get; }
        public Guid Id { get; }
    }

    public class UnfavoriteContentHandler : DbCommandBase, IAsyncVoidCommandHandler<UnfavoriteContent>
    {
        public UnfavoriteContentHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(UnfavoriteContent request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            var c = game.Contents.FindOrThrowFromRequest(request);

            game.Unfavorite(c);

            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}