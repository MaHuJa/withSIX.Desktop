// <copyright company="SIX Networks GmbH" file="Converters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    public static class Converters
    {
        public static Visibility ReverseVisibility(bool x) {
            return VisibilityNormal(!x);
        }

        public static Visibility VisibilityNormal(bool x) {
            return x ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility VisibilityNormal(int x) {
            return VisibilityNormal(x > 0);
        }

        public static Visibility ReverseVisibility(int x) {
            return VisibilityNormal(x == 0);
        }

        public static Visibility ReverseVisibility(object x) {
            return VisibilityNormal(x == null);
        }

        public static Visibility VisibilityNormal(object x) {
            return VisibilityNormal(x != null);
        }

        public static Visibility OrVisibility(bool x, bool y) {
            return VisibilityNormal(x || y);
        }

        public static Visibility ReverseOrVisibility(bool x, bool y) {
            return OrVisibility(!x, !y);
        }

        public static string TimeAgoConverter(DateTime dt) {
            return dt.Ago();
        }

        public static string Speed(double arg) {
            return arg.FormatSpeed();
        }

        public static string Size(double arg) {
            return arg.FormatSize();
        }

        public static string Progress(double arg) {
            return String.Format("{0:0.##} %", arg);
        }
    }
}