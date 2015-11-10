// <copyright company="SIX Networks GmbH" file="ScrollviewerAutoScroll.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class ScrollViewerEx
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd",
                typeof (bool), typeof (ScrollViewerEx),
                new PropertyMetadata(false, HookupAutoScrollToEnd));
        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEndHandler",
                typeof (ScrollViewerAutoScrollToEndHandler), typeof (ScrollViewerEx));

        static void HookupAutoScrollToEnd(DependencyObject d,
            DependencyPropertyChangedEventArgs e) {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer == null)
                return;

            SetAutoScrollToEnd(scrollViewer, (bool) e.NewValue);
        }

        public static bool GetAutoScrollToEnd(ScrollViewer instance) {
            return (bool) instance.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScrollToEnd(ScrollViewer instance, bool value) {
            var oldHandler = (ScrollViewerAutoScrollToEndHandler) instance.GetValue(AutoScrollHandlerProperty);
            if (oldHandler != null) {
                oldHandler.Dispose();
                instance.SetValue(AutoScrollHandlerProperty, null);
            }
            instance.SetValue(AutoScrollProperty, value);
            if (value)
                instance.SetValue(AutoScrollHandlerProperty, new ScrollViewerAutoScrollToEndHandler(instance));
        }
    }

    public class ScrollViewerAutoScrollToEndHandler : DependencyObject, IDisposable
    {
        readonly ScrollViewer _scrollViewer;
        bool _doScroll = true;

        public ScrollViewerAutoScrollToEndHandler(ScrollViewer scrollViewer) {
            if (scrollViewer == null)
                throw new ArgumentNullException("scrollViewer");

            _scrollViewer = scrollViewer;
            _scrollViewer.ScrollToEnd();
            _scrollViewer.ScrollChanged += ScrollChanged;
        }

        public void Dispose() {
            _scrollViewer.ScrollChanged -= ScrollChanged;
        }

        void ScrollChanged(object sender, ScrollChangedEventArgs e) {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
                _doScroll = _scrollViewer.VerticalOffset == _scrollViewer.ScrollableHeight;

            // Content scroll event : autoscroll eventually
            if (_doScroll && e.ExtentHeightChange != 0)
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ExtentHeight);
        }
    }
}