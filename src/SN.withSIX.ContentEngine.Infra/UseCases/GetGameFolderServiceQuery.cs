// <copyright company="SIX Networks GmbH" file="GetGameFolderServiceQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.ContentEngine.Infra.Services;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    public sealed class GetGameFolderServiceQuery : GetContentEngineService<IGameFolderService>
    {
        public GetGameFolderServiceQuery(RegisteredMod mod) : base(mod) {}
    }

    public sealed class GetGameFolderServiceQueryHandler :
        GetServiceQueryHandler<GetGameFolderServiceQuery, IGameFolderService, GameFolderService>,
        IGetGameFolderServiceFactory, IInfrastructureService
    {
        readonly IContentEngineGameContext _gameContext;

        public GetGameFolderServiceQueryHandler(IServiceRegistry serviceRegistry, IContentEngineGameContext gameContext)
            : base(serviceRegistry) {
            _gameContext = gameContext;
        }

        protected override GameFolderService CreateService(GetGameFolderServiceQuery request) {
            return new GameFolderService(request.Mod, _gameContext.Get(request.Mod.Mod.GameId));
        }
    }

    public interface IGetGameFolderServiceFactory : IServiceFactory<GetGameFolderServiceQuery, IGameFolderService> {}
}