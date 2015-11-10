// <copyright company="SIX Networks GmbH" file="TextBoxWithFolderBrowser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Presentation.Wpf.Commands;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class TextBoxWithFolderBrowser : TextBox
    {
        static TextBoxWithFolderBrowser() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TextBoxWithFolderBrowser),
                new FrameworkPropertyMetadata(typeof (TextBoxWithFolderBrowser)));

            CommandManager.RegisterClassCommandBinding(typeof (TextBoxWithFolderBrowser), new CommandBinding(
                TextBoxBrowserCommands.BrowseCommand,
                (obj, e) => {
                    var theObj = ((TextBoxWithFolderBrowser) obj);
                    theObj.ShowFolderBrowser();
                    theObj.Focus();
                    e.Handled = true;
                },
                (obj, e) => { e.CanExecute = true; }));

            CommandManager.RegisterClassCommandBinding(typeof (TextBoxWithFolderBrowser), new CommandBinding(
                TextBoxBrowserCommands.ClearCommand,
                (obj, e) => {
                    var theObj = ((TextBoxWithFolderBrowser) obj);
                    theObj.SetValue(TextProperty, string.Empty);
                    theObj.Focus();
                    e.Handled = true;
                },
                (obj, e) => { e.CanExecute = true; }));
        }

        public TextBoxWithFolderBrowser() {
            AddHandler(MouseDoubleClickEvent,
                new RoutedEventHandler(DoubleClicked), true);
        }

        [DoNotObfuscate]
        void DoubleClicked(object sender, RoutedEventArgs e) {
            ShowFolderBrowser();
        }

        [ReportUsage]
        public void ShowFolderBrowser() {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
                Text = dialog.SelectedPath;
        }
    }
}