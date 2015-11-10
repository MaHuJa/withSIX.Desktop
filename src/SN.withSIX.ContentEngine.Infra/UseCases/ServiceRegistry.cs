// <copyright company="SIX Networks GmbH" file="ServiceRegistry.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using ShortBus;
using SN.withSIX.ContentEngine.Infra.Attributes;
using SN.withSIX.ContentEngine.Infra.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    public interface IServiceRegistry : IInfrastructureService
    {
        bool TryGetServiceFromRegistration<TService>(string modToken, out TService service)
            where TService : IContentEngineService;

        bool TryGetServiceFromRegistration<TService>(RegisteredMod mod, out TService service)
            where TService : IContentEngineService;

        void RegisterServiceForMod<TService>(string modToken, TService service) where TService : IContentEngineService;
        void RegisterServiceForMod<TService>(RegisteredMod mod, TService service) where TService : IContentEngineService;

        void RegisterService<TService>()
            where TService : IContentEngineService;

        object GetServiceForScript(string name, string token);
    }

    public class ServiceRegistry : IServiceRegistry
    {
        readonly IDependencyResolver _depResolver;
        readonly ILogger _logger;
        readonly Dictionary<string, Type> _registeredServices = new Dictionary<string, Type>();
        readonly Dictionary<RegisteredMod, List<object>> _registrations = new Dictionary<RegisteredMod, List<object>>();
        readonly IModScriptRegistry _scriptRegistry;

        public ServiceRegistry(IModScriptRegistry scriptRegistry, IDependencyResolver depResolver) {
            _scriptRegistry = scriptRegistry;
            _depResolver = depResolver;
            _logger = MainLog.Logger;
        }

        public bool TryGetServiceFromRegistration<TService>(string modToken, out TService service)
            where TService : IContentEngineService {
            var mod = _scriptRegistry.GetMod(modToken);
            return TryGetServiceFromRegistration(mod, out service);
        }

        public bool TryGetServiceFromRegistration<TService>(RegisteredMod mod, out TService service)
            where TService : IContentEngineService {
            var registrations = GetRegistrations(mod);
            var registration = GetServiceFromRegistration<TService>(registrations);
            if (registration == null) {
                service = default(TService);
                return false;
            }
            service = (TService) registration;
            return true;
        }

        public void RegisterServiceForMod<TService>(string modToken, TService service)
            where TService : IContentEngineService {
            var mod = _scriptRegistry.GetMod(modToken);
            RegisterServiceForMod(mod, service);
        }

        public void RegisterServiceForMod<TService>(RegisteredMod mod, TService service)
            where TService : IContentEngineService {
            var registrations = GetRegistrations(mod);
            var registration = GetServiceFromRegistration<TService>(registrations);
            if (registration != null)
                throw new ArgumentException("CRITICAL: Mod already has registered Service of type", "service");

            if (CheckIfServiceIsRestricted<TService>() && !HandleRestrictedService(mod, service))
                throw new Exception("Mod tried registering a restricted service without correct permissions.");

            registrations.Add(service);
        }

        public void RegisterService<TService>()
            where TService : IContentEngineService {
            var type = typeof (TService);

            var attribute = GetServiceAttribute<TService>();

            if (attribute == null)
                throw new CustomAttributeFormatException("Registering a service with a missing CEService Attribute");


            if (_registeredServices.ContainsKey(attribute.Name))
                throw new Exception("Failure Registering Service, Service already exists");

            _registeredServices.Add(attribute.Name, type);
        }

        public object GetServiceForScript(string name, string token) {
            Contract.Requires<ArgumentNullException>(!name.IsBlankOrWhiteSpace());
            Contract.Requires<ArgumentNullException>(!token.IsBlankOrWhiteSpace());

            Type service;

            var mod = _scriptRegistry.GetMod(token);

            if (!TryGetRegisteredServiceDef(name, out service))
                throw new Exception("Requested Service is not registered!");

            var attrDef = GetServiceAttribute(service);

            var instance = Activator.CreateInstance(attrDef.QueryType, mod);

            //TODO: Try REALLY hard not to use dynamic
            //TODO: Real service factory abstraction
            return GetService((dynamic) instance);
        }

        IGameFolderService GetService(GetGameFolderServiceQuery instance) {
            return ResolveService<IGetGameFolderServiceFactory>().Handle(instance);
        }

        ITeamspeakService GetService(GetTeamspeakServiceQuery instance) {
            return ResolveService<IGetTeamSpeakServiceFactory>().Handle(instance);
        }

        T ResolveService<T>() {
            return (T) _depResolver.GetInstance(typeof (T));
        }

        static object GetServiceFromRegistration<TService>(List<object> registrations)
            where TService : IContentEngineService {
            return registrations.OfType<TService>().SingleOrDefault();
        }

        bool HandleRestrictedService<TService>(RegisteredMod mod, TService service)
            where TService : IContentEngineService {
            var attr = GetServiceAttribute<TService>();
            _logger.Warn("Mod is registering a protected Service!", mod.Guid, attr.Name);
            return true;
            throw new NotImplementedException();
            //TODO: Implement when we reach milestone 2 or 3. MUST be done before public availability.
        }

        bool CheckIfServiceIsRestricted<TService>() where TService : IContentEngineService {
            return typeof (TService).IsSubclassOf(typeof (RestrictedContentEngineService));
        }

        List<object> GetRegistrations(RegisteredMod mod) {
            if (_registrations.ContainsKey(mod))
                return _registrations[mod];
            var list = new List<object>();
            _registrations.Add(mod, list);
            return list;
        }

        bool TryGetRegisteredServiceDef(string name, out Type service) {
            Contract.Requires<ArgumentNullException>(!name.IsBlankOrWhiteSpace());

            if (!_registeredServices.ContainsKey(name)) {
                service = null;
                return false;
            }
            service = _registeredServices[name];
            return true;
        }

        static CEServiceAttribute GetServiceAttribute<TService>() where TService : IContentEngineService {
            return GetServiceAttribute(typeof (TService));
        }

        static CEServiceAttribute GetServiceAttribute(Type type) {
            return type.GetCustomAttributes(typeof (CEServiceAttribute), false)
                .Cast<CEServiceAttribute>().FirstOrDefault();
        }
    }
}