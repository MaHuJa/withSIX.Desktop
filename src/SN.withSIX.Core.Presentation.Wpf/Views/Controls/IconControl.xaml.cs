// <copyright company="SIX Networks GmbH" file="IconControl.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for IconControl.xaml
    /// </summary>
    public partial class IconControl : UserControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof (object),
            typeof (IconControl), new PropertyMetadata(default(object)));

        public IconControl() {
            InitializeComponent();
        }

        public object Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
    }
}