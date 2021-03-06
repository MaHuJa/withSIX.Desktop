﻿// <copyright company="SIX Networks GmbH" file="PlayContent.cs">
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
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class PlayContent : SingleCntentBase, IAsyncVoidCommand
    {
        public PlayContent(Guid gameId, ContentGuidSpec content) : base(gameId, content) {}
    }

    public class PlayContentHandler : ApiDbCommandBase, IAsyncVoidCommandHandler<PlayContent>
    {
        readonly IContentInstallationService _contentInstallation;
        readonly IGameLauncherFactory _factory;

        public PlayContentHandler(IDbContextLocator gameContext, IGameLauncherFactory factory,
            IContentInstallationService contentInstallation) : base(gameContext) {
            _factory = factory;
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(PlayContent request) {
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

        static PlayContentAction GetAction(PlayContent request, Game game, CancellationToken token) {
            var content = game.Contents.First(x => x.Id == request.Content.Id);
            var action = new PlayContentAction(cancelToken: token,
                content: new ContentSpec(content, request.Content.Constraint));
            return action;
        }
    }
}