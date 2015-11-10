// <copyright company="SIX Networks GmbH" file="KeyboardSelectionHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Helpers
{
    public static class KeyboardSelectionHelper
    {
        public static bool IsNavigateBack(this KeyEventArgs e) {
            return (((e.Key == Key.Prior) || (e.Key == Key.BrowserBack)) ||
                    ((e.KeyboardDevice.Modifiers == ModifierKeys.Alt) && (e.SystemKey == Key.Left)));
        }

        public static bool IsNavigatingPastBounds(this Key key, int selectedIndex, int count) {
            return ((count <= 0) ||
                    (((key == Key.Up) && (selectedIndex == 0)) || ((key == Key.Down) && (selectedIndex == (count - 1)))));
        }

        public static void MoveDown(this Selector listBox) {
            if ((listBox.Items.Count != 0) && (listBox.Items.Count > (listBox.SelectedIndex + 1)))
                listBox.SelectedIndex++;
        }

        public static void MoveUp(this Selector listBox) {
            if ((listBox.Items.Count != 0) && (listBox.SelectedIndex > 0))
                listBox.SelectedIndex--;
        }
    }
}