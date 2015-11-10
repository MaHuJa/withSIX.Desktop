// <copyright company="SIX Networks GmbH" file="UiConstants.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Presentation.Wpf
{
    public static class UiConstants
    {
        public const double DefaultMargin = 8;
        public const double SectionMargin = DefaultMargin*2;
        const int ContentRowBottomBorder = 4;
        const double WidthMargin = DefaultMargin;
        const double HeightMargin = DefaultMargin - ContentRowBottomBorder;
        public const double ContentColumnWidth = 344 + (DefaultMargin*2);
        public const double ContentColumnWidthInclMargins = ContentColumnWidth + WidthMargin;
        public const double ContentColumnWideWidth = 534 + (DefaultMargin*2);
        public const double ContentColumnWideWidthInclMargins = ContentColumnWideWidth + WidthMargin;
        public const double ContentRowHeight = 48 + ContentRowBottomBorder + (DefaultMargin*2);
        public const double ContentRowHeightInclMargins = ContentRowHeight + HeightMargin;
    }
}