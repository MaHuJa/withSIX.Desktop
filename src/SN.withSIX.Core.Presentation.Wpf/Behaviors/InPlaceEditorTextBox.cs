// <copyright company="SIX Networks GmbH" file="InPlaceEditorTextBox.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public class InPlaceEditorTextBox : TextBox
    {
        public void POnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
            OnMouseDown(mouseButtonEventArgs);
        }

        public void POnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
            OnMouseUp(mouseButtonEventArgs);
        }

        public void POnMouseEnter(object sender, MouseEventArgs e) {
            OnMouseEnter(e);
        }
    }
}