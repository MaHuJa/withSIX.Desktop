// <copyright company="SIX Networks GmbH" file="SixRichTextBox.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    public partial class SixRichTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (SixRichTextBox),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof (ScrollBarVisibility),
                typeof (SixRichTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

        public SixRichTextBox() {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility) GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }
    }
}