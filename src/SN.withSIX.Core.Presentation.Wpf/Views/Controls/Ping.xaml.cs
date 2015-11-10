// <copyright company="SIX Networks GmbH" file="Ping.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class Ping : UserControl
    {
        public static readonly DependencyProperty ShowPingAsNumberProperty =
            DependencyProperty.Register("ShowPingAsNumber", typeof (bool), typeof (Ping),
                new UIPropertyMetadata(false));
        public static readonly DependencyProperty PingProperty =
            DependencyProperty.Register("PingBinding", typeof (long), typeof (Ping),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty CountryProperty =
            DependencyProperty.Register("CountryBinding", typeof (string), typeof (Ping),
                new FrameworkPropertyMetadata(null));

        public Ping() {
            InitializeComponent();
        }

        public bool ShowPingAsNumber
        {
            get { return (bool) GetValue(ShowPingAsNumberProperty); }
            set { SetValue(ShowPingAsNumberProperty, value); }
        }
        public long PingBinding
        {
            get { return (long) GetValue(PingProperty); }
            set { SetValue(PingProperty, value); }
        }
        public string CountryBinding
        {
            get { return (string) GetValue(CountryProperty); }
            set { SetValue(CountryProperty, value); }
        }
    }
}