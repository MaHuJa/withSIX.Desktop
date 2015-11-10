// <copyright company="SIX Networks GmbH" file="SecureCache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;
using Akavache;
using Akavache.Sqlite3;

namespace SN.withSIX.Core.Infra.Cache
{
    public class SecureCache : SQLiteEncryptedBlobCache, ISecureCache
    {
        public SecureCache(string databaseFile, IEncryptionProvider encryptionProvider = null,
            IScheduler scheduler = null) : base(databaseFile, encryptionProvider, scheduler) {}
    }
}