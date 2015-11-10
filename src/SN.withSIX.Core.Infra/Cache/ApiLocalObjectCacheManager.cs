// <copyright company="SIX Networks GmbH" file="ApiLocalObjectCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Akavache;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Core.Infra.Cache
{
    public class ApiLocalObjectCacheManager : ObjectCacheManager, IInfrastructureService, IApiLocalObjectCacheManager
    {
        readonly IApiLocalCache _localCache;

        public ApiLocalObjectCacheManager(IApiLocalCache localCache) : base(localCache) {
            _localCache = localCache;
        }

        public IObservable<byte[]> Download(Uri uri) {
            return _localCache.DownloadUrl(uri.ToString());
        }

        public IObservable<byte[]> Download(Uri uri, TimeSpan timeSpan) {
            return _localCache.DownloadUrl(uri.ToString(), timeSpan);
        }
    }
}