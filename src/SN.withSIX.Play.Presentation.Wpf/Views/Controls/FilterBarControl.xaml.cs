// <copyright company="SIX Networks GmbH" file="FilterBarControl.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for FilterBarControl.xaml
    /// </summary>
    public partial class FilterBarControl : UserControl
    {
        public static readonly DependencyProperty WaterMarkProperty = DependencyProperty.Register("WaterMark",
            typeof (string), typeof (FilterBarControl), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText",
            typeof (string), typeof (FilterBarControl), new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public FilterBarControl() {
            InitializeComponent();
        }

        public string WaterMark
        {
            get { return (string) GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
        public string FilterText
        {
            get { return (string) GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }
    }
}