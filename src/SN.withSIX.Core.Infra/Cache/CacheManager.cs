// <copyright company="SIX Networks GmbH" file="CacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Core.Infra.Cache
{
    public class CacheManager : ICacheManager, IInfrastructureService
    {
        readonly List<IBlobCache> _caches = new List<IBlobCache>();

        public Task Vacuum() {
            IBlobCache[] caches;
            lock (_caches)
                caches = _caches.ToArray();

            return caches.Select(x => x.Vacuum()).Merge().ToList().Select(_ => Unit.Default).ToTask();
        }

        public Task Shutdown() {
            IBlobCache[] caches;
            lock (_caches)
                caches = _caches.ToArray();

            return caches.Select(x => {
                x.Dispose();
                return x.Shutdown;
            }).Merge().ToList().Select(_ => Unit.Default).ToTask();
        }

        public void RegisterCache(IBlobCache cache) {
            lock (_caches)
                _caches.Add(cache);
        }
    }
}