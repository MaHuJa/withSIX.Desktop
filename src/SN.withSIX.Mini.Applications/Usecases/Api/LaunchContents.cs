// <copyright company="SIX Networks GmbH" file="LaunchContents.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class LaunchContents : ContentsBase, IAsyncVoidCommand
    {
        public LaunchContents(Guid gameId, List<ContentGuidSpec> contents) : base(gameId, contents) {}
    }

    public class LaunchContentsHandler : ApiDbCommandBase, IAsyncVoidCommandHandler<LaunchContents>
    {
        readonly IGameLauncherFactory _factory;

        public LaunchContentsHandler(IDbContextLocator gameContext, IGameLauncherFactory factory) : base(gameContext) {
            _factory = factory;
        }

        public async Task<UnitType> HandleAsync(LaunchContents request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            await RaiseGameEvent(game).ConfigureAwait(false);

            using (var cts = new DoneCancellationTokenSource()) {
                var action = GetAction(request, game, cts.Token);

                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Launch(_factory, action).ConfigureAwait(false);
                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }

        // TODO: Optimize query
        static LaunchContentAction GetAction(LaunchContents request, Game game, CancellationToken token) {
            var action =
                new LaunchContentAction(
                    request.Contents.Select(x => new ContentSpec(game.Contents.FindOrThrow(x.Id), x.Constraint))
                        .ToArray(), cancelToken: token)
                {Name = request.Name};
            return action;
        }
    }
}