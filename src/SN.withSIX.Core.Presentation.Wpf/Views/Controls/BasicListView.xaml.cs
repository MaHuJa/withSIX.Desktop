// <copyright company="SIX Networks GmbH" file="BasicListView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class BasicListView : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText",
            typeof (string), typeof (BasicListView), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items",
            typeof (IEnumerable),
            typeof (BasicListView), new PropertyMetadata(default(IEnumerable)));
        public static readonly DependencyProperty AddItemCommandProperty = DependencyProperty.Register(
            "AddItemCommand", typeof (ICommand), typeof (BasicListView),
            new PropertyMetadata(default(ICommand)));
        public static readonly DependencyProperty ListItemTemplateSelectorProperty =
            DependencyProperty.Register("ListItemTemplateSelector", typeof (DataTemplateSelector),
                typeof (BasicListView), new PropertyMetadata(default(DataTemplateSelector)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
            typeof (object), typeof (BasicListView),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty EnableHeaderProperty = DependencyProperty.Register("EnableHeader",
            typeof (bool), typeof (BasicListView), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowEditButtonsProperty =
            DependencyProperty.Register("ShowEditButtons", typeof (bool), typeof (BasicListView),
                new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText",
            typeof (string), typeof (BasicListView),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public BasicListView() {
            InitializeComponent();
        }

        public string HeaderText
        {
            get { return (string) GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public IEnumerable Items
        {
            get { return (IEnumerable) GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public ICommand AddItemCommand
        {
            get { return (ICommand) GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }
        public DataTemplateSelector ListItemTemplateSelector
        {
            get { return (DataTemplateSelector) GetValue(ListItemTemplateSelectorProperty); }
            set { SetValue(ListItemTemplateSelectorProperty, value); }
        }
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public bool EnableHeader
        {
            get { return (bool) GetValue(EnableHeaderProperty); }
            set { SetValue(EnableHeaderProperty, value); }
        }
        public bool ShowEditButtons
        {
            get { return (bool) GetValue(ShowEditButtonsProperty); }
            set { SetValue(ShowEditButtonsProperty, value); }
        }
        public string FilterText
        {
            get { return (string) GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }
    }
}