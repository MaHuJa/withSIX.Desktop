using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetGameMods : IAsyncQuery<GameModsApiModel>, IHaveId<Guid>
    {
        public GetGameMods(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetGameModsHandler : DbQueryBase, IAsyncRequestHandler<GetGameMods, GameModsApiModel>
    {
        public GetGameModsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<GameModsApiModel> HandleAsync(GetGameMods request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<GameModsApiModel>();
        }
    }

    public class GameModsApiModel
    {
        public List<ContentApiModel> Mods { get; set; }
    }
}