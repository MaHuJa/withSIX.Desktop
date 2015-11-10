// <copyright company="SIX Networks GmbH" file="IImageCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Splat;

namespace SN.withSIX.Core.Infra.Cache
{
    public interface IImageCacheManager
    {
        IObservable<IBitmap> GetImage(Uri uri, DesiredImageSize desiredDimensions);
        IObservable<IBitmap> GetImage(Uri uri, TimeSpan offset, DesiredImageSize desiredDimensions);
        IObservable<IBitmap> GetImage(Uri uri);
        IObservable<IBitmap> GetImage(Uri uri, TimeSpan offset);
    }
}