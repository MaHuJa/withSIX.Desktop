// <copyright company="SIX Networks GmbH" file="WindowLocationBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class WindowLocationBehavior
    {
        /// <summary>
        ///     KeepOnScreen Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty TaskbarLocationProperty = DependencyProperty.RegisterAttached(
            "TaskbarLocation",
            typeof (bool),
            typeof (WindowLocationBehavior),
            new FrameworkPropertyMetadata(false, OnTaskbarLocationChanged));

        /// <summary>
        ///     Gets the TaskbarLocation property.  This dependency property
        ///     indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> to get the property from</param>
        /// <returns>The value of the TaskbarLocation property</returns>
        public static bool GetTaskbarLocation(DependencyObject d) {
            return (bool) d.GetValue(TaskbarLocationProperty);
        }

        /// <summary>
        ///     Sets the TaskbarLocation property.  This dependency property
        ///     indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetTaskbarLocation(DependencyObject d, bool value) {
            d.SetValue(TaskbarLocationProperty, value);
        }

        /// <summary>
        ///     Handles changes to the TaskbarLocation property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
        static void OnTaskbarLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (Window) d;
            SetWindowPos(window);
        }

        static void WindowOnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            var window = (Window) sender;
            SetWindowPos(window);
        }

        public static void SetWindowPos(this Window window, double offSetX = 60, double offSetY = 16) {
            double left, top;
            TaskBarLocationProvider.CalculateWindowPositionByTaskbar(window, window.Width + offSetX,
                window.Height + offSetY,
                out left, out top);
            window.Left = left;
            window.Top = top;
        }
    }
}