// <copyright company="SIX Networks GmbH" file="LaunchContents.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class LaunchContents : IAsyncVoidCommand, IHaveGameId
    {
        public LaunchContents(Guid gameId, List<ContentGuidSpec> contents, LaunchType launchType = LaunchType.Default, LaunchAction action = LaunchAction.Default) {
            GameId = gameId;
            Contents = contents;
            LaunchType = launchType;
            Action = action;
        }

        public List<ContentGuidSpec> Contents { get; }
        public LaunchType LaunchType { get; }
        public LaunchAction Action { get; }
        public Guid GameId { get; }
    }


    public class LaunchContentsHandler : DbCommandBase, IAsyncVoidCommandHandler<LaunchContents>
    {
        readonly IGameLauncherFactory _launcherFactory;

        public LaunchContentsHandler(IDbContextLocator dbContextLocator, IGameLauncherFactory launcherFactory)
            : base(dbContextLocator) {
            _launcherFactory = launcherFactory;
        }

        public async Task<UnitType> HandleAsync(LaunchContents request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);

            // TODO: Optimize query
            using (var cts = new DoneCancellationTokenSource()) {
                var action =
                    new LaunchContentAction(
                        request.Contents.Select(x => new ContentSpec(game.Contents.FindOrThrow(x.Id), x.Constraint))
                            .ToArray(), cancelToken: cts.Token) {
                                Action = request.Action
                            };

                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Launch(_launcherFactory, action).ConfigureAwait(false);

                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }
    }
}