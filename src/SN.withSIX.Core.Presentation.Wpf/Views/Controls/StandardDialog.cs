// <copyright company="SIX Networks GmbH" file="StandardDialog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class StandardDialog : ContentControl
    {
        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register("ButtonContent",
            typeof (FrameworkElement), typeof (StandardDialog), new PropertyMetadata(default(FrameworkElement)));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string),
            typeof (StandardDialog), new PropertyMetadata(default(string)));

        static StandardDialog() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (StandardDialog),
                new FrameworkPropertyMetadata(typeof (StandardDialog)));
        }

        public StandardDialog() {
            SetBinding(TitleProperty, new Binding("DisplayName") {Mode = BindingMode.TwoWay});
        }

        public FrameworkElement ButtonContent
        {
            get { return (FrameworkElement) GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }
        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
    }
}