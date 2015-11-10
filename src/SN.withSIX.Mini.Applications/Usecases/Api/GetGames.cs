// <copyright company="SIX Networks GmbH" file="GetGames.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetGames : IAsyncQuery<GamesApiModel> {}

    public class GetGamesHandler : DbQueryBase, IAsyncRequestHandler<GetGames, GamesApiModel>
    {
        public GetGamesHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<GamesApiModel> HandleAsync(GetGames request) {
            await GameContext.LoadAll().ConfigureAwait(false);
            var games = await GameContext.Games.Where(x => x.InstalledState.IsInstalled).ToListAsync().ConfigureAwait(false);

            return games.MapTo<GamesApiModel>();
        }
    }

    public class GamesApiModel
    {
        public List<GameApiModel> Games { get; set; }
    }
}