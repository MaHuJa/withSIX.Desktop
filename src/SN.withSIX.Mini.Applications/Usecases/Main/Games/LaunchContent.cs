// <copyright company="SIX Networks GmbH" file="LaunchContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class LaunchContent : IAsyncVoidCommand, IHaveId<Guid>, IHaveGameId
    {
        public LaunchContent(Guid gameId, ContentGuidSpec content, LaunchType launchType = LaunchType.Default,
            LaunchAction action = LaunchAction.Default) {
            GameId = gameId;
            Content = content;
            LaunchType = launchType;
            Action = action;
        }

        public ContentGuidSpec Content { get; }
        public LaunchType LaunchType { get; }
        public LaunchAction Action { get; }
        public Guid GameId { get; }
        public Guid Id => Content.Id;
    }

    public class LaunchContentHandler : DbCommandBase, IAsyncVoidCommandHandler<LaunchContent>
    {
        readonly IGameLauncherFactory _launcherFactory;

        public LaunchContentHandler(IDbContextLocator dbContextLocator, IGameLauncherFactory launcherFactory)
            : base(dbContextLocator) {
            _launcherFactory = launcherFactory;
        }

        public async Task<UnitType> HandleAsync(LaunchContent request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);

            using (var cts = new DoneCancellationTokenSource()) {
                var action =
                    new LaunchContentAction(request.LaunchType, cts.Token,
                        new ContentSpec(game.Contents.Find(request.Content.Id),
                            request.Content.Constraint)) {Action = request.Action};

                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Launch(_launcherFactory, action).ConfigureAwait(false);

                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }
    }
}