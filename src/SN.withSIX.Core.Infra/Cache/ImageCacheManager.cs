// <copyright company="SIX Networks GmbH" file="ImageCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Akavache;
using SN.withSIX.Core.Infra.Services;
using Splat;

namespace SN.withSIX.Core.Infra.Cache
{
    public class ImageCacheManager : IImageCacheManager, IInfrastructureService
    {
        readonly IImageCache _cache;

        public ImageCacheManager(IImageCache cache) {
            _cache = cache;
        }

        public IObservable<IBitmap> GetImage(Uri uri, DesiredImageSize desiredDimensions) {
            return _cache.LoadImageFromUrl(GetDimensionKey(uri, desiredDimensions), uri.ToString(), false,
                desiredDimensions.Width, desiredDimensions.Height);
        }

        public IObservable<IBitmap> GetImage(Uri uri, TimeSpan offset, DesiredImageSize desiredDimensions) {
            var url = uri.ToString();
            return _cache.LoadImageFromUrl(GetDimensionKey(uri, desiredDimensions), url, false, desiredDimensions.Width,
                desiredDimensions.Height,
                GetAbsoluteUtc(offset));
        }

        public IObservable<IBitmap> GetImage(Uri uri) {
            return _cache.LoadImageFromUrl(uri.ToString());
        }

        public IObservable<IBitmap> GetImage(Uri uri, TimeSpan offset) {
            return _cache.LoadImageFromUrl(uri.ToString(), false, null, null, GetAbsoluteUtc(offset));
        }

        static string GetDimensionKey(Uri uri, DesiredImageSize desiredDimensions) {
            return uri + "??dimensions=" + desiredDimensions;
        }

        static DateTime GetAbsoluteUtc(TimeSpan offset) {
            return Tools.Generic.GetCurrentUtcDateTime.Add(offset);
        }
    }
}