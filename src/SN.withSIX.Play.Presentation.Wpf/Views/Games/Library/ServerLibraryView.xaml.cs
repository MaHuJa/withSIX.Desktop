// <copyright company="SIX Networks GmbH" file="ServerLibraryView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Presentation.Wpf.Views.Controls;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for ServerLibraryView.xaml
    /// </summary>
    [DoNotObfuscate]
    public partial class ServerLibraryView : LibraryControl
    {
        public static readonly DependencyProperty ShowPingAsNumberProperty =
            DependencyProperty.Register("ShowPingAsNumber", typeof (bool), typeof (ServerLibraryView),
                new PropertyMetadata(default(bool)));

        public ServerLibraryView() {
            InitializeComponent();
        }

        public bool ShowPingAsNumber
        {
            get { return (bool) GetValue(ShowPingAsNumberProperty); }
            set { SetValue(ShowPingAsNumberProperty, value); }
        }
    }
}