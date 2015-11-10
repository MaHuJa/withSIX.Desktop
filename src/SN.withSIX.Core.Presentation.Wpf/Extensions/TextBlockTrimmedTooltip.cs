// <copyright company="SIX Networks GmbH" file="TextBlockTrimmedTooltip.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    /// <summary>
    ///     Attached property provider which adds the read-only attached property
    ///     <c>TextBlockService.IsTextTrimmed</c> to the framework's <see cref="TextBlock" /> control.
    /// </summary>
    public class TextBlockTrimmedTooltip
    {
        static TextBlockTrimmedTooltip() {
            // Register for the SizeChanged event on all TextBlocks, even if the event was handled.
            EventManager.RegisterClassHandler(
                typeof (TextBlock),
                FrameworkElement.SizeChangedEvent,
                new SizeChangedEventHandler(OnTextBlockSizeChanged),
                true);
        }

        /// <summary>
        ///     Event handler for TextBlock's SizeChanged routed event. Triggers evaluation of the
        ///     IsTextTrimmed attached property.
        /// </summary>
        /// <param name="sender">Object where the event handler is attached</param>
        /// <param name="e">Event data</param>
        public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e) {
            var textBlock = sender as TextBlock;
            if (null == textBlock)
                return;

            SetIsTextTrimmed(textBlock, TextTrimming.None != textBlock.TextTrimming && CalculateIsTextTrimmed(textBlock));
        }

        /// <summary>
        ///     Sets the instance value of read-only dependency property <see cref="IsTextTrimmed" />.
        /// </summary>
        /// <param name="target">Associated <see cref="TextBlock" /> instance</param>
        /// <param name="value">New value for IsTextTrimmed</param>
        static void SetIsTextTrimmed(TextBlock target, Boolean value) {
            target.SetValue(IsTextTrimmedKey, value);
        }

        /// <summary>
        ///     Determines whether or not the text in <paramref name="textBlock" /> is currently being
        ///     trimmed due to width or height constraints.
        /// </summary>
        /// <remarks>Does not work properly when TextWrapping is set to WrapWithOverflow.</remarks>
        /// <param name="textBlock"><see cref="TextBlock" /> to evaluate</param>
        /// <returns><c>true</c> if the text is currently being trimmed; otherwise <c>false</c></returns>
        static bool CalculateIsTextTrimmed(TextBlock textBlock) {
            if (!textBlock.IsArrangeValid)
                return GetIsTextTrimmed(textBlock);

            var typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            var formattedText = new FormattedText(
                textBlock.Text,
                Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground) {MaxTextWidth = textBlock.ActualWidth};

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return (formattedText.Height > textBlock.ActualHeight);
        }

        #region Attached Property [TextBlockService.IsTextTrimmed]

        /// <summary>
        ///     Key returned upon registering the read-only attached property <c>IsTextTrimmed</c>.
        /// </summary>
        public static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsTextTrimmed",
            typeof (bool),
            typeof (TextBlockTrimmedTooltip),
            new PropertyMetadata(false)); // defaults to false

        /// <summary>
        ///     Identifier associated with the read-only attached property <c>IsTextTrimmed</c>.
        /// </summary>
        public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        /// <summary>
        ///     Returns the current effective value of the IsTextTrimmed attached property.
        /// </summary>
        /// <remarks>Invoked automatically by the framework when databound.</remarks>
        /// <param name="target"><see cref="TextBlock" /> to evaluate</param>
        /// <returns>Effective value of the IsTextTrimmed attached property</returns>
        [AttachedPropertyBrowsableForType(typeof (TextBlock))]
        public static Boolean GetIsTextTrimmed(TextBlock target) {
            return (Boolean) target.GetValue(IsTextTrimmedProperty);
        }

        #endregion (Attached Property [TextBlockService.IsTextTrimmed])

        #region Attached Property [TextBlockService.AutomaticToolTipEnabled]

        /// <summary>
        ///     Identifier associated with the attached property <c>AutomaticToolTipEnabled</c>.
        /// </summary>
        public static readonly DependencyProperty AutomaticToolTipEnabledProperty = DependencyProperty.RegisterAttached(
            "AutomaticToolTipEnabled",
            typeof (bool),
            typeof (TextBlockTrimmedTooltip),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits)); // defaults to true

        /// <summary>
        ///     Gets the current effective value of the AutomaticToolTipEnabled attached property.
        /// </summary>
        /// <param name="target"><see cref="TextBlock" /> to evaluate</param>
        /// <returns>Effective value of the AutomaticToolTipEnabled attached property</returns>
        [AttachedPropertyBrowsableForType(typeof (DependencyObject))]
        public static Boolean GetAutomaticToolTipEnabled(DependencyObject element) {
            if (null == element)
                throw new ArgumentNullException("element");
            return (bool) element.GetValue(AutomaticToolTipEnabledProperty);
        }

        /// <summary>
        ///     Sets the current effective value of the AutomaticToolTipEnabled attached property.
        /// </summary>
        /// <param name="target"><see cref="TextBlock" /> to evaluate</param>
        /// <param name="value"><c>true</c> to enable the automatic ToolTip; otherwise <c>false</c></param>
        public static void SetAutomaticToolTipEnabled(DependencyObject element, bool value) {
            if (null == element)
                throw new ArgumentNullException("element");
            element.SetValue(AutomaticToolTipEnabledProperty, value);
        }

        #endregion (Attached Property [TextBlockService.AutomaticToolTipEnabled])
    }
}