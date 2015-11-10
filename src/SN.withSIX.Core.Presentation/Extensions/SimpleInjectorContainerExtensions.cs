// <copyright company="SIX Networks GmbH" file="SimpleInjectorContainerExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Caliburn.Micro;
using ShortBus;
using SimpleInjector;
using SimpleInjector.Advanced;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using Action = System.Action;
using Container = SimpleInjector.Container;

namespace SN.withSIX.Core.Presentation.Extensions
{
    public class CustomLifestyleSelectionBehavior : ILifestyleSelectionBehavior
    {
        readonly Type[] _notificationHandlers = {
            typeof (INotificationHandler<>),
            typeof (IAsyncNotificationHandler<>)
        };
        readonly Lifestyle lifestyle = Lifestyle.Transient;

        public Lifestyle SelectLifestyle(Type serviceType, Type implementationType) {
            //Requires.IsNotNull(serviceType, nameof(serviceType));
            //Requires.IsNotNull(implementationType, nameof(implementationType));
            if (serviceType.IsGenericType) {
                if (_notificationHandlers.Contains(serviceType.GetGenericTypeDefinition()))
                    return Lifestyle.Singleton;
            }

            return lifestyle;
        }
    }

    public static class SimpleInjectorContainerExtensions
    {
        static readonly Type[] reserved = {
            typeof (INotifyPropertyChanged), typeof (INotifyPropertyChanging),
            typeof (IScreen), typeof (IEnableLogging), typeof (IHandle), typeof (IDisposable)
        };
        static readonly Type[] reservedRoot = {
            typeof (IHandle)
        };

        public static void RegisterPlugins<T>(this Container container, IEnumerable<Assembly> assemblies,
            Lifestyle lifestyle = null) where T : class {
            var pluginTypes = GetTypes<T>(assemblies).ToArray();
            HandleLifestyle<T>(container, lifestyle, pluginTypes);
            container.RegisterCollection<T>(pluginTypes);
        }

        static void HandleLifestyle<T>(Container container, Lifestyle lifestyle, IEnumerable<Type> pluginTypes)
            where T : class {
            if (lifestyle == null || lifestyle == Lifestyle.Transient)
                return;
            var serviceType = typeof (T);
            foreach (var t in pluginTypes)
                container.Register(serviceType, t, lifestyle);
        }

        public static void RegisterPlugins<TImplements, TExport>(this Container container,
            IEnumerable<Assembly> assemblies, Lifestyle lifestyle = null) where TExport : class {
            var pluginTypes = GetTypes<TImplements>(assemblies).ToArray();
            HandleLifestyle<TExport>(container, lifestyle, pluginTypes);
            container.RegisterCollection<TExport>(pluginTypes);
        }

        public static IEnumerable<Type> GetTypes<T>(this IEnumerable<Assembly> assemblies) {
            return assemblies.GetTypes(typeof (T));
        }

        public static IEnumerable<Type> GetTypes(this IEnumerable<Assembly> assemblies, Type t) {
            return from assembly in assemblies.Distinct()
                from type in assembly.GetTypes()
                where t.IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where !type.IsInterface
                select type;
        }

        public static void RegisterAllInterfaces<T>(this Container container, IEnumerable<Assembly> assemblies) {
            var ifaceType = typeof (T);
            foreach (var s in assemblies.GetTypes<T>()) {
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.Register(i, s);
            }
        }

        static bool Predicate(Type x, Type ifaceType) {
            return x != ifaceType && !reserved.Contains(x) && !reservedRoot.Any(t => t.IsAssignableFrom(x));
        }

        public static void RegisterAllInterfacesAndType<T>(this Container container,
            IEnumerable<Assembly> assemblies, Func<Type, bool> predicate = null) {
            var enumerable = assemblies.GetTypes<T>();
            if (predicate != null)
                enumerable = enumerable.Where(predicate);
            container.RegisterInterfacesAndType<T>(enumerable);
        }

        public static void RegisterInterfacesAndType<T>(this Container container, IEnumerable<Type> types) {
            var ifaceType = typeof (T);
            foreach (var s in types) {
                container.Register(s, s);
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.Register(i, s);
            }
        }

        public static void RegisterInterfaces<T>(this Container container, IEnumerable<Type> types) {
            var ifaceType = typeof (T);
            foreach (var s in types) {
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.Register(i, s);
            }
        }

        public static void RegisterSingleInterfacesAndType<T>(this Container container, IEnumerable<Type> types) {
            var ifaceType = typeof (T);
            foreach (var s in types) {
                container.RegisterSingleton(s, s);
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.RegisterSingleton(i, () => container.GetInstance(s));
            }
        }

        public static void RegisterSingleInterfaces<T>(this Container container, IEnumerable<Type> types) {
            var ifaceType = typeof (T);
            foreach (var s in types) {
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.RegisterSingleton(i, s);
            }
        }

        public static void RegisterSingleAllInterfaces<T>(this Container container, IEnumerable<Assembly> assemblies,
            Func<Type, bool> predicate = null) {
            var enumerable = assemblies.GetTypes<T>();
            if (predicate != null)
                enumerable = enumerable.Where(predicate);
            container.RegisterSingleInterfaces<T>(enumerable);
        }

        public static void RegisterSingleAllInterfacesAndType<T>(this Container container,
            IEnumerable<Assembly> assemblies, Func<Type, bool> predicate = null) {
            var enumerable = assemblies.GetTypes<T>();
            if (predicate != null)
                enumerable = enumerable.Where(predicate);
            container.RegisterSingleInterfacesAndType<T>(enumerable);
        }

        public static void RegisterLazy(this Container container, Type serviceType) {
            var method = typeof (SimpleInjectorContainerExtensions).GetMethod("CreateLazy")
                .MakeGenericMethod(serviceType);

            var lazyInstanceCreator =
                Expression.Lambda<Func<object>>(
                    Expression.Call(method, Expression.Constant(container)))
                    .Compile();

            var lazyServiceType =
                typeof (Lazy<>).MakeGenericType(serviceType);

            container.Register(lazyServiceType, lazyInstanceCreator);
        }

        public static void RegisterLazy<T>(this Container container) where T : class {
            Func<T> factory = container.GetInstance<T>;
            container.Register(() => new Lazy<T>(factory));
        }
    }

    public static class ResolvingFactoriesExtensions
    {
        public static void AllowResolvingExportFactories(this Container container) {
            container.ResolveUnregisteredType += (s, e) => {
                var type = e.UnregisteredServiceType;
                if (!type.IsGenericType ||
                    type.GetGenericTypeDefinition() != typeof (ExportFactory<>))
                    return;

                var args = type.GetGenericArguments();
                var method = typeof (ResolvingFactoriesExtensions).GetMethod("CreateEF")
                    .MakeGenericMethod(args[0]);
                var factoryDelegate = Expression.Lambda<Func<object>>(Expression.Call(method,
                    Expression.Constant(container))).Compile()();
                e.Register(Expression.Constant(factoryDelegate));
            };
        }

        [DoNotObfuscate]
        public static ExportFactory<T> CreateEF<T>(Container container) where T : class {
            ConfirmTransient(container, typeof (T));
            return new ExportFactory<T>(() => {
                var instance = new Lazy<T>(container.GetInstance<T>);
                var act = typeof (IDisposable).IsAssignableFrom(typeof (T))
                    ? () => {
                        if (instance.IsValueCreated)
                            ((IDisposable) instance.Value).Dispose();
                    }
                    : TaskExt.NullAction;
                return new Tuple<T, Action>(instance.Value, act);
            });
        }

        static void ConfirmTransient(Container container, Type serviceType) {
            var registration = container.GetRegistration(serviceType);
            ConfirmTransient(registration, serviceType);
        }

        static void ConfirmTransient(InstanceProducer registration, Type serviceType) {
            if (registration.Lifestyle != Lifestyle.Transient)
                throw new ArgumentOutOfRangeException("type", "Lifestyle != LifeStyle.Transient for: " + serviceType);
        }

        // This extension method is equivalent to the following registration, for each and every T:
        // container.RegisterSingleton<Func<T>>(() => container.GetInstance<T>());
        // This is useful for consumers that need to create multiple instances of a dependency.
        // This mimics the behavior of Autofac. In Autofac this behavior is default.
        public static void AllowResolvingFuncFactories(this Container container) {
            container.ResolveUnregisteredType += (sender, e) => {
                if (!e.UnregisteredServiceType.IsGenericType ||
                    e.UnregisteredServiceType.GetGenericTypeDefinition() != typeof (Func<>))
                    return;
                var serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];

                var registration = container.GetRegistration(serviceType, true);
                ConfirmTransient(registration, serviceType);

                var funcType = typeof (Func<>).MakeGenericType(serviceType);

                var factoryDelegate =
                    Expression.Lambda(funcType, registration.BuildExpression()).Compile();

                e.Register(Expression.Constant(factoryDelegate));
            };
        }

        // This extension method is equivalent to the following registration, for each and every T:
        // container.Register<Lazy<T>>(() => new Lazy<T>(() => container.GetInstance<T>()));
        // This is useful for consumers that have a dependency on a service that is expensive to create, but
        // not always needed.
        // This mimics the behavior of Autofac and Ninject 3. In Autofac this behavior is default.
        public static void AllowResolvingLazyFactories(this Container container) {
            container.ResolveUnregisteredType += (sender, e) => {
                if (!e.UnregisteredServiceType.IsGenericType ||
                    e.UnregisteredServiceType.GetGenericTypeDefinition() != typeof (Lazy<>))
                    return;
                var serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];


                var method = ReflectionExtensions.GetGeneric<Func<Container, Lazy<object>>>(CreateLazy<object>,
                    serviceType);

                var factoryDelegate =
                    Expression.Lambda<Func<object>>(Expression.Call(method, Expression.Constant(container)))
                        .Compile()();

                e.Register(Expression.Constant(factoryDelegate));

                /*
                var registration = container.GetRegistration(serviceType, true);

                var funcType = typeof (Func<>).MakeGenericType(serviceType);
                var lazyType = typeof (Lazy<>).MakeGenericType(serviceType);

                var factoryDelegate =
                    Expression.Lambda(funcType, registration.BuildExpression()).Compile();

                var lazyConstructor = (
                    from ctor in lazyType.GetConstructors()
                    where ctor.GetParameters().Length == 1
                    where ctor.GetParameters()[0].ParameterType == funcType
                    select ctor)
                    .Single();

                e.Register(Expression.New(lazyConstructor, Expression.Constant(factoryDelegate)));
                */
            };
        }

        [DoNotObfuscate]
        public static Lazy<T> CreateLazy<T>(Container container)
            where T : class {
            return new Lazy<T>(container.GetInstance<T>);
        }

        public static void AllowResolvingParameterizedFuncFactories(this Container container) {
            container.ResolveUnregisteredType += (sender, e) => {
                if (!IsParameterizedFuncDelegate(e.UnregisteredServiceType))
                    return;

                var genericArguments = e.UnregisteredServiceType.GetGenericArguments();

                var componentType = genericArguments.Last();

                if (componentType.IsAbstract)
                    return;

                var funcType = e.UnregisteredServiceType;

                var factoryArguments = genericArguments.Take(genericArguments.Length - 1).ToArray();

                var serviceType = factoryArguments.Last();
                ConfirmTransient(container, serviceType);

                var constructor = container.Options.ConstructorResolutionBehavior
                    .GetConstructor(componentType, componentType);

                //var ctorArguments = constructor.GetParameters().Select(p => p.ParameterType).ToArray();

                var parameters = (
                    from factoryArgumentType in factoryArguments
                    select Expression.Parameter(factoryArgumentType))
                    .ToArray();

                var factoryDelegate = Expression.Lambda(funcType,
                    BuildNewExpression(container, constructor, parameters),
                    parameters)
                    .Compile();

                e.Register(Expression.Constant(factoryDelegate));
            };
        }

        static bool IsParameterizedFuncDelegate(Type type) {
            if (!type.IsGenericType || !type.FullName.StartsWith("System.Func`"))
                return false;

            return type.GetGenericTypeDefinition().GetGenericArguments().Length > 1;
        }

        static NewExpression BuildNewExpression(Container container,
            ConstructorInfo constructor,
            ParameterExpression[] funcParameterExpression) {
            var ctorParameters = constructor.GetParameters();
            var ctorParameterTypes = ctorParameters.Select(p => p.ParameterType).ToArray();
            var funcParameterTypes = funcParameterExpression.Select(p => p.Type).ToArray();

            var funcParametersIndex = IndexOfSubCollection(ctorParameterTypes, funcParameterTypes);

            if (funcParametersIndex == -1) {
                throw new ActivationException(string.Format(
                    "The constructor of type {0} did not contain the sequence of the following " +
                    "constructor parameters: {1}.",
                    constructor.DeclaringType.FullName,
                    string.Join(", ", funcParameterTypes.Select(t => t.Name))));
            }

            var firstCtorParameterExpressions = ctorParameterTypes
                .Take(funcParametersIndex)
                .Select(type => container.GetRegistration(type, true).BuildExpression());

            var lastCtorParameterExpressions = ctorParameterTypes
                .Skip(funcParametersIndex + funcParameterTypes.Length)
                .Select(type => container.GetRegistration(type, true).BuildExpression());

            var expressions = firstCtorParameterExpressions
                .Concat(funcParameterExpression)
                .Concat(lastCtorParameterExpressions)
                .ToArray();

            return Expression.New(constructor, expressions);
        }

        static int IndexOfSubCollection(Type[] collection, Type[] subCollection) {
            return (
                from index in Enumerable.Range(0, collection.Length - subCollection.Length + 1)
                let collectionPart = collection.Skip(index).Take(subCollection.Length)
                where collectionPart.SequenceEqual(subCollection)
                select (int?) index)
                .FirstOrDefault() ?? -1;
        }
    }
}