// <copyright company="SIX Networks GmbH" file="RuntimeCheckWpf.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class RuntimeCheckWpf : RuntimeCheck
    {
        protected override bool FatalErrorMessage(string message, string caption) {
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes;
        }
    }
}