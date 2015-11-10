// <copyright company="SIX Networks GmbH" file="AutoMapperWrapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using AutoMapper;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Infra.Services
{
    abstract class AutoMapperWrapper : IMapper
    {
        protected IMappingEngine Engine;
        public IConfigurationProvider ConfigurationProvider
        {
            get { return Engine.ConfigurationProvider; }
        }

        public TDestination Map<TDestination>(object source) {
            return Engine.Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) {
            return Engine.Map(source, destination);
        }

        public void Dispose() {
            Engine.Dispose();
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts) {
            return Engine.Map<TDestination>(source, opts);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions> opts) {
            return Engine.Map(source, destination, opts);
        }

        public object Map(object source, Type sourceType, Type destinationType) {
            return Engine.Map(source, sourceType, destinationType);
        }

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) {
            return Engine.Map(source, sourceType, destinationType, opts);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType) {
            return Engine.Map(source, destination, sourceType, destinationType);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts) {
            return Engine.Map(source, destination, sourceType, destinationType, opts);
        }

        public TDestination DynamicMap<TDestination>(object source) {
            return Engine.DynamicMap<TDestination>(source);
        }

        public object DynamicMap(object source, Type sourceType, Type destinationType) {
            return Engine.DynamicMap(source, sourceType, destinationType);
        }

        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination) {
            Engine.DynamicMap(source, destination);
        }

        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType) {
            Engine.DynamicMap(source, destination, sourceType, destinationType);
        }
    }
}