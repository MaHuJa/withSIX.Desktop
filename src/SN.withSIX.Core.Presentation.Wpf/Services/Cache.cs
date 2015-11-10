// <copyright company="SIX Networks GmbH" file="Cache.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Core.Logging;
using Splat;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class Cache
    {
        public static IImageFileCache ImageFiles;

        public interface IImageFileCache
        {
            Task<BitmapSource> BmiFromUrlAsync(Uri uri, CancellationToken token);

            Task<BitmapSource> BmiFromUrlAsync(Uri uri, double width, double height,
                CancellationToken token);

            Task<BitmapSource> BmiFromUriAsync(Uri uri);
        }

        public class ImageFileCache : IEnableLogging, IPresentationService, IImageFileCache
        {
            readonly TimeSpan _defaultCacheTime = TimeSpan.FromDays(30);
            readonly IImageCacheManager _downloader;
            // TODO: Check if valid image file before passing it along?
            // 

            public ImageFileCache(IImageCacheManager downloader) {
                _downloader = downloader;
            }

            public async Task<BitmapSource> BmiFromUrlAsync(Uri uri, CancellationToken token) {
                Contract.Requires<ArgumentNullException>(uri != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(uri.ToString()));

                var image = await BmiFromUrlAsync(uri).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return image;
            }

            public async Task<BitmapSource> BmiFromUrlAsync(Uri uri, double width, double height,
                CancellationToken token) {
                var image =
                    await
                        BmiFromUrlAsync(uri,
                            new DesiredImageSize(!double.IsNaN(width) ? (float?) width : null,
                                !double.IsNaN(height) ? (float?) height : null)).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return image;
            }

            public Task<BitmapSource> BmiFromUriAsync(Uri uri) {
                Contract.Requires<ArgumentNullException>(uri != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(uri.ToString()));

                return BmiFromUrlAsync(uri);
            }

            async Task<BitmapSource> BmiFromUrlAsync(Uri uri) {
                using (var bitMap = await Download(uri).ConfigureAwait(false))
                    return GetBitMapFromMemoryStream(bitMap);
            }

            async Task<BitmapSource> BmiFromUrlAsync(Uri uri, DesiredImageSize desiredSize) {
                using (var bitMap = await Download(uri, desiredSize).ConfigureAwait(false))
                    return GetBitMapFromMemoryStream(bitMap);
            }

            async Task<IBitmap> Download(Uri uri) {
                return await _downloader.GetImage(uri, _defaultCacheTime);
            }

            async Task<IBitmap> Download(Uri uri, DesiredImageSize desiredSize) {
                return await _downloader.GetImage(uri, _defaultCacheTime, desiredSize);
            }

            static BitmapSource GetBitMapFromMemoryStream(IBitmap bitMap) {
                return bitMap.ToNative();
            }
        }
    }
}