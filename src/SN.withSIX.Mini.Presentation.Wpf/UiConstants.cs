// <copyright company="SIX Networks GmbH" file="UiConstants.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using System.Windows;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    public static class UiConstants
    {
        public const double ContentColumnWidth = 340;
        public const double StandardMargin = 8;
        public const double ContentColumnWidthInclMargins = ContentColumnWidth + StandardMargin;
        public static readonly GridLength StandardMarginGrid = new GridLength(StandardMargin);
        public static readonly Thickness StandardMarginControl = new Thickness(StandardMargin);
        static bool? _inDesignMode;
        /// <summary>
        ///     Gets a value that indicates whether the process is running in design mode.
        /// </summary>
        public static bool InDesignMode
        {
            get
            {
                if (_inDesignMode == null) {
#if WinRT
                    inDesignMode = DesignMode.DesignModeEnabled;
#elif SILVERLIGHT
                    inDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var descriptor = DependencyPropertyDescriptor.FromProperty(
                        DesignerProperties.IsInDesignModeProperty, typeof (FrameworkElement));
                    _inDesignMode = (bool) descriptor.Metadata.DefaultValue;
#endif
                }

                return _inDesignMode.GetValueOrDefault(false);
            }
        }
    }
}