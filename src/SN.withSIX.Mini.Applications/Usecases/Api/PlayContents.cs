// <copyright company="SIX Networks GmbH" file="PlayContents.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class PlayContents : ContentsBase, IAsyncVoidCommand
    {
        public PlayContents(Guid gameId, List<ContentGuidSpec> contents) : base(gameId, contents) {}
    }

    public class PlayContentsHandler : ApiDbCommandBase, IAsyncVoidCommandHandler<PlayContents>
    {
        readonly IContentInstallationService _contentInstallation;
        readonly IGameLauncherFactory _factory;

        public PlayContentsHandler(IDbContextLocator gameContext, IGameLauncherFactory factory,
            IContentInstallationService contentInstallation) : base(gameContext) {
            _factory = factory;
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(PlayContents request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            await RaiseGameEvent(game).ConfigureAwait(false);

            using (var cts = new DoneCancellationTokenSource()) {
                var action = GetAction(request, game, cts.Token);
                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Play(_factory, _contentInstallation, action).ConfigureAwait(false);
                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }

        static PlayContentAction GetAction(PlayContents request, Game game, CancellationToken token) {
            // TODO: Optimize query
            var action =
                new PlayContentAction(
                    request.Contents.Select(x => new ContentSpec(game.Contents.FindOrThrow(x.Id), x.Constraint))
                        .ToArray(), cancelToken: token)
                {Name = request.Name};
            return action;
        }
    }
}