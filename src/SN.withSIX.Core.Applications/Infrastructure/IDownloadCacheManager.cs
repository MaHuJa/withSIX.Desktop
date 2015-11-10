// <copyright company="SIX Networks GmbH" file="IDownloadCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Applications.Infrastructure
{
    public interface IDownloadCacheManager
    {
        IObservable<byte[]> Download(Uri uri);
        IObservable<byte[]> Download(Uri uri, TimeSpan timeSpan);
    }
}