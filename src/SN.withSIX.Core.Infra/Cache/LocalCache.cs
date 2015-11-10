// <copyright company="SIX Networks GmbH" file="LocalCache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;
using Akavache.Sqlite3;

namespace SN.withSIX.Core.Infra.Cache
{
    public class LocalCache : SQLitePersistentBlobCache, ILocalCache
    {
        public LocalCache(string databaseFile, IScheduler scheduler = null) : base(databaseFile, scheduler) {}
    }
}