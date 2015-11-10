// <copyright company="SIX Networks GmbH" file="InMemoryCache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;
using Akavache;

namespace SN.withSIX.Core.Infra.Cache
{
    public class InMemoryCache : InMemoryBlobCache, IInMemoryCache
    {
        public InMemoryCache() {}
        public InMemoryCache(IScheduler scheduler = null) : base(scheduler) {}
    }
}