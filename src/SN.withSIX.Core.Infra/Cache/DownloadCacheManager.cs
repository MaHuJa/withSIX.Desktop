// <copyright company="SIX Networks GmbH" file="DownloadCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Akavache;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Core.Infra.Cache
{
    public class DownloadCacheManager : IDownloadCacheManager, IInfrastructureService
    {
        readonly ILocalCache _localCache;

        public DownloadCacheManager(ILocalCache localCache) {
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