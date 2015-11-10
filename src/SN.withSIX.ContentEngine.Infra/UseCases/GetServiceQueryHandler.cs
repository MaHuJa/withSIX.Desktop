// <copyright company="SIX Networks GmbH" file="GetServiceQueryHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.ContentEngine.Infra.Services;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    public abstract class GetServiceQueryHandler<TQuery, TService, TConcreteService>
        : IServiceFactory<TQuery, TService>
        where TQuery : IGetContentEngineService<TService>
        where TService : IContentEngineService
        where TConcreteService : class, TService

    {
        readonly IServiceRegistry _serviceRegistry;

        protected GetServiceQueryHandler(IServiceRegistry serviceRegistry) {
            _serviceRegistry = serviceRegistry;
        }

        public TService Handle(TQuery request) {
            TService tsService;
            if (_serviceRegistry.TryGetServiceFromRegistration(request.Mod, out tsService))
                return tsService;
            tsService = CreateService(request);
            _serviceRegistry.RegisterServiceForMod(request.Mod, tsService);
            return tsService;
        }

        protected virtual TConcreteService CreateService(TQuery request) {
            return (TConcreteService) Activator.CreateInstance(typeof (TConcreteService), request.Mod);
        }
    }

    public interface IServiceFactory<in TQuery, out TService> where TQuery : IGetContentEngineService<TService>
        where TService : IContentEngineService
    {
        TService Handle(TQuery request);
    }
}