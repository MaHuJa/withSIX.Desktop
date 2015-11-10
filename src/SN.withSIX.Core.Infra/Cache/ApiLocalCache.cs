// <copyright company="SIX Networks GmbH" file="ApiLocalCache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;

namespace SN.withSIX.Core.Infra.Cache
{
    public class ApiLocalCache : LocalCache, IApiLocalCache
    {
        public ApiLocalCache(string databaseFile, IScheduler scheduler = null) : base(databaseFile, scheduler) {}
    }
}