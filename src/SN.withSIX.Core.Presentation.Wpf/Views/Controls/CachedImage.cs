// <copyright company="SIX Networks GmbH" file="CachedImage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Caliburn.Micro;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Wpf.Services;
using WpfAnimatedGif;
using Action = System.Action;
using Timer = System.Timers.Timer;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    // Divisable by 8! Keep in sync with JS: W6.imageSizes
    public static class ImageConstants
    {
        public const string MissionOriginalFileName = "original.jpg";
        [Obsolete] public static readonly ImageSize MissionThumbnailSize = new ImageSize(160, 100);
        public static readonly ImageSize SmallSquare = new ImageSize(48, 48);
        public static readonly ImageSize SmallRectangle = new ImageSize(384, 216);
        public static readonly ImageSize BigRectangle = new ImageSize(1024, 576);
        public static readonly ImageSize[] DesiredSizes = {
            SmallRectangle, BigRectangle,
            SmallSquare
        };
    }

    public class Rectangle : IEquatable<Rectangle>
    {
        public Rectangle(int width, int height) {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public bool Equals(Rectangle other) {
            if (ReferenceEquals(null, other))
                return false;
            return ReferenceEquals(this, other) || (other.Width == Width && other.Height == Height);
        }

        public override int GetHashCode() {
            unchecked {
                return (Width*397) ^ Height; // ToString().GetHashCode();
            }
        }

        public override bool Equals(object other) {
            return Equals(other as Rectangle);
        }

        public override string ToString() {
            return string.Format("{0}x{1}", Width, Height);
        }
    }

    public class ImageSize : Rectangle
    {
        public ImageSize(int width, int height) : base(width, height) {}
    }


    public class CachedImage : Image, IEnableLogging
    {
        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register("ImageUrl", typeof (string), typeof (CachedImage),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.None,
                    OnImageUrlChanged
                    ));
        public static readonly DependencyProperty DefaultImageSourceProperty =
            DependencyProperty.Register("DefaultImageSource", typeof (ImageSource), typeof (CachedImage),
                new FrameworkPropertyMetadata(default(ImageSource), OnDefaultImageSourceChanged));
        protected CancellationTokenSource CTS = new CancellationTokenSource();

        static CachedImage() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (CachedImage),
                new FrameworkPropertyMetadata(typeof (CachedImage)));
        }

        public string ImageUrl
        {
            get { return (string) GetValue(ImageUrlProperty); }
            set
            {
                if (value != ImageUrl)
                    SetValue(ImageUrlProperty, value);
            }
        }
        public ImageSource DefaultImageSource
        {
            get { return (ImageSource) GetValue(DefaultImageSourceProperty); }
            set { SetValue(DefaultImageSourceProperty, value); }
        }

        static void OnDefaultImageSourceChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var obj = (CachedImage) dependencyObject;
            obj.Source = (ImageSource) dependencyPropertyChangedEventArgs.NewValue;
        }

        protected virtual Task<BitmapSource> FetchImage(string url) {
            return String.IsNullOrWhiteSpace(url)
                ? null
                : Cache.ImageFiles.BmiFromUrlAsync(new Uri(url), CTS.Token);
        }

        async void TryGetImage(string url) {
            CTS.Cancel(); // TODO: Dispose?
            CTS.Dispose();
            CTS = new CancellationTokenSource();

            SetImage(null, DefaultImageSource);
            if (string.IsNullOrWhiteSpace(url))
                return;

            await FetchAndSetImage(url).ConfigureAwait(false);
        }

        async Task FetchAndSetImage(string url) {
            try {
                SetImage(url, await FetchImage(url) ?? DefaultImageSource);
            } catch (OperationCanceledException) {
                // Silently eat canceledex
                SetImage(url, DefaultImageSource);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e, "Image download failed from: " + url);
                SetImage(url, DefaultImageSource);
            }
        }

        static void OnImageUrlChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) {
            if (Execute.InDesignMode)
                return;
            var imageSource = (CachedImage) source;
            imageSource.TryGetImage((string) args.NewValue);
        }

        protected virtual void SetImage(string url, ImageSource image) {
            Dispatcher.BeginInvoke(new Action(() => Source = image), DispatcherPriority.DataBind);
        }
    }

    public class CachedImageWithSizeChanger : CachedImage
    {
        protected override Task<BitmapSource> FetchImage(string url) {
            return String.IsNullOrWhiteSpace(url)
                ? null
                : Cache.ImageFiles.BmiFromUrlAsync(new Uri(url), Width, Height, CTS.Token);
        }
    }

    public class CachedImageWithContentSizeChanger : CachedImage
    {
        protected override Task<BitmapSource> FetchImage(string url) {
            return String.IsNullOrWhiteSpace(url)
                ? null
                : Cache.ImageFiles.BmiFromUrlAsync(new Uri(url), ImageConstants.SmallRectangle.Height,
                    ImageConstants.SmallRectangle.Width, CTS.Token);
        }
    }

    public class CachedImageWithAnimatedGifSupport : CachedImage
    {
        ImageSource _image;
        Timer _timer;

        protected override void SetImage(string url, ImageSource image) {
            if (!string.IsNullOrWhiteSpace(url) && url.Contains(".gif")) {
                _image = image;

                // Workaround for animated gif issues - using a timer. This does have resizing issues for some gifs
                var timer = _timer;
                try {
                    if (timer != null)
                        timer.Dispose();
                } catch (ObjectDisposedException) {}

                // 250 would immediately cause the problems again! - So probably quite unstable. 750-1000 seems to work - so far.
                // TODO: What about async void + await Task.Delay + cancellation token instead of timer?
                _timer = new TimerWithElapsedCancellation(750, () => {
                    var img = _image;
                    _image = null;
                    Dispatcher.BeginInvoke(new Action(() => TrySetAnimatedSource(image, img)),
                        DispatcherPriority.DataBind);
                    _timer = null;
                    return false;
                });
            } else
                base.SetImage(url, image);
        }

        void TrySetAnimatedSource(ImageSource image, ImageSource img) {
            try {
                Source = null;
                ImageBehavior.SetAnimatedSource(this, img);
            } catch (Exception) {
                ImageBehavior.SetAnimatedSource(this, null);
                Source = image;
            }
        }
    }
}