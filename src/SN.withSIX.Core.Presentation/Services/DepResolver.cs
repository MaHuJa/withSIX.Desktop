// <copyright company="SIX Networks GmbH" file="DepResolver.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SimpleInjector;
using SN.withSIX.Core.Presentation.Factories;

namespace SN.withSIX.Core.Presentation.Services
{
    public class DepResolver : IDepResolver, IPresentationService
    {
        readonly Container _container;

        public DepResolver(Container container) {
            _container = container;
        }

        public T GetInstance<T>() where T : class {
            return _container.GetInstance<T>();
        }

        public IEnumerable<T> GetInstances<T>() where T : class {
            return _container.GetAllInstances<T>();
        }
    }
}