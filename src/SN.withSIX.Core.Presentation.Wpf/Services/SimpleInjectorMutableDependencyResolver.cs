// <copyright company="SIX Networks GmbH" file="SimpleInjectorMutableDependencyResolver.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactiveUI;
using SimpleInjector;
using SimpleInjector.Advanced;
using SN.withSIX.Core.Presentation.Extensions;
using Splat;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public static class MutableDependencyResolverExtensions
    {
        public static void RegisterAllInterfaces<T>(this IMutableDependencyResolver resolver,
            IEnumerable<Assembly> assemblies, Func<Type, Type, bool> predicate) {
            var ifaceType = typeof (T);
            foreach (var s in assemblies.GetTypes<T>()) {
                foreach (var i in s.GetInterfaces().Where(x => predicate(x, ifaceType))) {
                    var contract = s.GetCustomAttribute<ViewContractAttribute>();
                    resolver.Register(() => Activator.CreateInstance(s), i, contract == null ? null : contract.Contract);
                }
            }
        }
    }

    // Does not support contracts or mapping non existent interfaces like IViewFor<Concrete>, but directs back to original resolver instead.
    // And SI GetAllInstances actually throws exception when no registered
    public class SimpleInjectorMutableDependencyResolver : IMutableDependencyResolver
    {
        readonly Container _container;
        readonly IMutableDependencyResolver _resolver;

        public SimpleInjectorMutableDependencyResolver(Container container, IMutableDependencyResolver resolver) {
            _container = container;
            _resolver = resolver;
        }

        public void Dispose() {
            //_container.Dispose();
            _resolver.Dispose();
        }

        public object GetService(Type serviceType, string contract = null) {
            return string.IsNullOrEmpty(contract)
                ? _container.GetItem(serviceType) ?? _resolver.GetService(serviceType)
                : _resolver.GetService(serviceType, contract);
        }

        public IEnumerable<object> GetServices(Type serviceType, string contract = null) {
            return string.IsNullOrEmpty(contract)
                ? _container.GetAllInstances(serviceType) ?? _resolver.GetServices(serviceType)
                : _resolver.GetServices(serviceType, contract);
        }

        public void Register(Func<object> factory, Type serviceType, string contract = null) {
            _resolver.Register(factory, serviceType, contract);
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback) {
            return _resolver.ServiceRegistrationCallback(serviceType, contract, callback);
        }
    }
}