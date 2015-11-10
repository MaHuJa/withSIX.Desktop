// <copyright company="SIX Networks GmbH" file="UpdaterApp.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;

namespace SN.withSIX.Updater.Presentation.Wpf
{
    public partial class UpdaterApp : Application
    {
        public static void Launch() {
            var application = new UpdaterApp {ShutdownMode = ShutdownMode.OnMainWindowClose};
            application.InitializeComponent();
            application.Run();
        }
    }
}