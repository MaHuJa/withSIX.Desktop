// <copyright company="SIX Networks GmbH" file="GetTeamspeakServiceQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.ContentEngine.Infra.Services;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    public sealed class GetTeamspeakServiceQuery : GetContentEngineService<ITeamspeakService>
    {
        public GetTeamspeakServiceQuery(RegisteredMod mod) : base(mod) {}
    }

    public sealed class GetTeamspeakServiceQueryHandler :
        GetServiceQueryHandler<GetTeamspeakServiceQuery, ITeamspeakService, TeamspeakService>,
        IGetTeamSpeakServiceFactory, IInfrastructureService
    {
        public GetTeamspeakServiceQueryHandler(IServiceRegistry serviceRegistry) : base(serviceRegistry) {}
    }

    public interface IGetTeamSpeakServiceFactory : IServiceFactory<GetTeamspeakServiceQuery, ITeamspeakService> { }
}