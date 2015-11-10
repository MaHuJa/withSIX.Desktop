// <copyright company="SIX Networks GmbH" file="OpenContentLink.cs">
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

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class OpenContentLink : IHaveId<Guid>, IHaveGameId, IAsyncVoidCommand
    {
        public OpenContentLink(Guid gameId, Guid id) {
            GameId = gameId;
            Id = id;
        }

        public Guid GameId { get; }
        public Guid Id { get; }
    }

    public class OpenContentLinkHandler : DbCommandBase, IAsyncVoidCommandHandler<OpenContentLink>
    {
        public OpenContentLinkHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(OpenContentLink request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            var content = (IHavePath) game.Contents.FindOrThrowFromRequest(request);

            await UriOpener.OpenUri(Urls.Play, game.GetContentPath(content)).ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}