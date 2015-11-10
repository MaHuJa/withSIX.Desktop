// <copyright company="SIX Networks GmbH" file="LaunchContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class LaunchContent : SingleCntentBase, IAsyncVoidCommand
    {
        public LaunchContent(Guid gameId, ContentGuidSpec content) : base(gameId, content) {}
    }


    public class LaunchContentHandler : ApiDbCommandBase, IAsyncVoidCommandHandler<LaunchContent>
    {
        readonly IGameLauncherFactory _factory;

        public LaunchContentHandler(IDbContextLocator gameContext, IGameLauncherFactory factory) : base(gameContext) {
            _factory = factory;
        }

        public async Task<UnitType> HandleAsync(LaunchContent request) {
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

        static LaunchContentAction GetAction(LaunchContent request, Game game, CancellationToken token) {
            var content = game.Contents.First(x => x.Id == request.Content.Id);
            var action =
                new LaunchContentAction(cancelToken: token,
                    content: new ContentSpec(content, request.Content.Constraint));
            // TODO: Or split Content action from just launching the game.
            return action;
        }
    }
}