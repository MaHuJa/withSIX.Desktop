// <copyright company="SIX Networks GmbH" file="PlayApp.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Presentation.Wpf;

namespace SN.withSIX.Play.Presentation.Wpf
{
    public partial class PlayApp : SingleInstanceApp
    {
        static PlayApp() {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof (UIElement),
                new FrameworkPropertyMetadata(30.Seconds()));
        }

        public static void Launch() {
            var application = new PlayApp();
            application.InitializeComponent();
            application.Run();
        }
    }
}