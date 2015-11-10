// <copyright company="SIX Networks GmbH" file="WindowCloseBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    /// <summary>
    ///     Attached behavior that keeps the window on the screen
    /// </summary>
    public static class WindowCloseBehavior
    {
        /// <summary>
        ///     KeepOnScreen Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty EscapeClosesWindowProperty = DependencyProperty.RegisterAttached(
            "EscapeClosesWindow",
            typeof (bool),
            typeof (WindowCloseBehavior),
            new FrameworkPropertyMetadata(false, OnEscapeClosesWindowChanged));

        /// <summary>
        ///     Gets the EscapeClosesWindow property.  This dependency property
        ///     indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> to get the property from</param>
        /// <returns>The value of the EscapeClosesWindow property</returns>
        public static bool GetEscapeClosesWindow(DependencyObject d) {
            return (bool) d.GetValue(EscapeClosesWindowProperty);
        }

        /// <summary>
        ///     Sets the EscapeClosesWindow property.  This dependency property
        ///     indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetEscapeClosesWindow(DependencyObject d, bool value) {
            d.SetValue(EscapeClosesWindowProperty, value);
        }

        /// <summary>
        ///     Handles changes to the EscapeClosesWindow property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
        static void OnEscapeClosesWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            Window target = (Window) d;
            if (target != null)
                target.PreviewKeyDown += Window_PreviewKeyDown;
        }

        /// <summary>
        ///     Handle the PreviewKeyDown event on the window
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="KeyEventArgs" /> that contains the event data.</param>
        static void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            Window target = (Window) sender;

            // If this is the escape key, close the window
            if (e.Key == Key.Escape)
                target.Close();
        }
    }
}