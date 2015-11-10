// <copyright company="SIX Networks GmbH" file="UserCache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;
using Akavache.Sqlite3;

namespace SN.withSIX.Core.Infra.Cache
{
    public class UserCache : SQLitePersistentBlobCache, IUserCache
    {
        public UserCache(string databaseFile, IScheduler scheduler = null) : base(databaseFile, scheduler) {}
    }
}