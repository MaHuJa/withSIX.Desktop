// <copyright company="SIX Networks GmbH" file="PopupControl.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class PopupControl : ContentControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof (string),
                typeof (PopupControl),
                new PropertyMetadata(null));
        public static readonly DependencyProperty ReverseProperty = DependencyProperty.Register("Reverse", typeof (bool),
            typeof (PopupControl), new PropertyMetadata(default(bool)));

        static PopupControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PopupControl),
                new FrameworkPropertyMetadata(typeof (PopupControl)));
        }

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public bool Reverse
        {
            get { return (bool) GetValue(ReverseProperty); }
            set { SetValue(ReverseProperty, value); }
        }
    }
}