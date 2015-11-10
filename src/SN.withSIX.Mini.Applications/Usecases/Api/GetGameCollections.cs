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
    public class GetGameCollections : IAsyncQuery<GameCollectionsApiModel>, IHaveId<Guid>
    {
        public GetGameCollections(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetGameCollectionsHandler : DbQueryBase,
        IAsyncRequestHandler<GetGameCollections, GameCollectionsApiModel>
    {
        public GetGameCollectionsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<GameCollectionsApiModel> HandleAsync(GetGameCollections request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<GameCollectionsApiModel>();
        }
    }

    public class GameCollectionsApiModel
    {
        public List<ContentApiModel> Collections { get; set; }
    }
}