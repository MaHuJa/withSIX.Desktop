// <copyright company="SIX Networks GmbH" file="ImageCache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;
using Akavache.Sqlite3;

namespace SN.withSIX.Core.Infra.Cache
{
    public class ImageCache : SQLitePersistentBlobCache, IImageCache
    {
        public ImageCache(string databaseFile, IScheduler scheduler = null) : base(databaseFile, scheduler) {}
    }
}