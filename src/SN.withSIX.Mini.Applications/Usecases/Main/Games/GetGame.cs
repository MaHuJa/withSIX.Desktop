// <copyright company="SIX Networks GmbH" file="GetGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class GetGame : IAsyncQuery<IGameViewModel>, IHaveId<Guid>
    {
        public GetGame(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetGameHandler : DbQueryBase, IAsyncRequestHandler<GetGame, IGameViewModel>
    {
        public GetGameHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<IGameViewModel> HandleAsync(GetGame request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<GameViewModel>();
        }
    }
}