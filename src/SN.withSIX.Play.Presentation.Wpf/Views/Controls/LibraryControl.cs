// <copyright company="SIX Networks GmbH" file="LibraryControl.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for LibraryControl.xaml
    /// </summary>
    public class LibraryControl : Control
    {
        public static readonly DependencyProperty FilterTemplateProperty = DependencyProperty.Register("FilterTemplate",
            typeof (DataTemplate), typeof (LibraryControl), new PropertyMetadata(default(DataTemplate)));
        public static readonly DependencyProperty IntegratedFiltersProperty =
            DependencyProperty.Register("IntegratedFilters", typeof (bool), typeof (LibraryControl),
                new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty SearchToolTipProperty = DependencyProperty.Register("SearchToolTip",
            typeof (string), typeof (LibraryControl), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
            typeof (object), typeof (LibraryControl),
            new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof (IEnumerable), typeof (LibraryControl), new PropertyMetadata(default(IEnumerable)));
        public static readonly DependencyProperty ItemDetailTemplateProperty =
            DependencyProperty.Register("ItemDetailTemplate", typeof (DataTemplate), typeof (LibraryControl),
                new PropertyMetadata(default(DataTemplate)));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate",
            typeof (DataTemplate), typeof (LibraryControl), new PropertyMetadata(default(DataTemplate)));

        static LibraryControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (LibraryControl),
                new FrameworkPropertyMetadata(typeof (LibraryControl)));
        }

        public DataTemplate FilterTemplate
        {
            get { return (DataTemplate) GetValue(FilterTemplateProperty); }
            set { SetValue(FilterTemplateProperty, value); }
        }
        public bool IntegratedFilters
        {
            get { return (bool) GetValue(IntegratedFiltersProperty); }
            set { SetValue(IntegratedFiltersProperty, value); }
        }
        public string SearchToolTip
        {
            get { return (string) GetValue(SearchToolTipProperty); }
            set { SetValue(SearchToolTipProperty, value); }
        }
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public DataTemplate ItemDetailTemplate
        {
            get { return (DataTemplate) GetValue(ItemDetailTemplateProperty); }
            set { SetValue(ItemDetailTemplateProperty, value); }
        }
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
    }
}