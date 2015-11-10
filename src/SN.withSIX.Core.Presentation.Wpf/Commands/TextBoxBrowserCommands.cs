// <copyright company="SIX Networks GmbH" file="TextBoxBrowserCommands.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Controls;
using System.Windows.Input;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Presentation.Wpf.Commands
{
    [DoNotObfuscateType]
    public class TextBoxBrowserCommands
    {
        public static RoutedUICommand BrowseCommand = new RoutedUICommand("Browse", "BrowseCommand", typeof (TextBox));
        public static RoutedUICommand ClearCommand = new RoutedUICommand("Clear", "ClearCommand", typeof (TextBox));
    }
}