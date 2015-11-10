// <copyright company="SIX Networks GmbH" file="TextBoxWithFileDialog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Presentation.Wpf.Commands;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class TextBoxWithFileDialog : TextBox
    {
        static TextBoxWithFileDialog() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TextBoxWithFileDialog),
                new FrameworkPropertyMetadata(typeof (TextBoxWithFileDialog)));
            CommandManager.RegisterClassCommandBinding(typeof (TextBoxWithFileDialog), new CommandBinding(
                TextBoxBrowserCommands.BrowseCommand,
                (obj, e) => {
                    var theObj = ((TextBoxWithFileDialog) obj);
                    theObj.ShowFileOpenDialog();
                    theObj.Focus();
                    e.Handled = true;
                },
                (obj, e) => { e.CanExecute = true; }));
            CommandManager.RegisterClassCommandBinding(typeof (TextBoxWithFileDialog), new CommandBinding(
                TextBoxBrowserCommands.ClearCommand,
                (obj, e) => {
                    var theObj = ((TextBoxWithFileDialog) obj);
                    theObj.SetValue(TextProperty, string.Empty);
                    theObj.Focus();
                    e.Handled = true;
                },
                (obj, e) => { e.CanExecute = true; }));
        }

        public TextBoxWithFileDialog() {
            AddHandler(MouseDoubleClickEvent,
                new RoutedEventHandler(DoubleClicked), true);
        }

        [DoNotObfuscate]
        void DoubleClicked(object sender, RoutedEventArgs e) {
            ShowFileOpenDialog();
        }

        [ReportUsage]
        public void ShowFileOpenDialog() {
            var dialog = new OpenFileDialog {
                DefaultExt = "*.*"
            };
            if (dialog.ShowDialog() == true)
                Text = dialog.FileName;
        }
    }
}