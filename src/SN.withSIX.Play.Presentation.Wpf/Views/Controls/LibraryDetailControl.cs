// <copyright company="SIX Networks GmbH" file="LibraryDetailControl.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for LibraryDetailControl.xaml
    /// </summary>
    public class LibraryDetailControl : ContentControl
    {
        public static readonly DependencyProperty LibraryHeaderTemplateProperty =
            DependencyProperty.Register("LibraryHeaderTemplate",
                typeof (DataTemplate), typeof (LibraryDetailControl), new PropertyMetadata(default(DataTemplate)));
        public static readonly DependencyProperty HeaderContentTemplateProperty =
            DependencyProperty.Register("HeaderContentTemplate",
                typeof (DataTemplate), typeof (LibraryDetailControl), new PropertyMetadata(default(DataTemplate)));

        static LibraryDetailControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (LibraryDetailControl),
                new FrameworkPropertyMetadata(typeof (LibraryDetailControl)));
        }

        public DataTemplate LibraryHeaderTemplate
        {
            get { return (DataTemplate) GetValue(LibraryHeaderTemplateProperty); }
            set { SetValue(LibraryHeaderTemplateProperty, value); }
        }
        public DataTemplate HeaderContentTemplate
        {
            get { return (DataTemplate) GetValue(HeaderContentTemplateProperty); }
            set { SetValue(HeaderContentTemplateProperty, value); }
        }
    }
}