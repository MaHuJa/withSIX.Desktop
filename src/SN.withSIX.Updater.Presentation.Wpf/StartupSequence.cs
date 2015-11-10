// <copyright company="SIX Networks GmbH" file="StartupSequence.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using System.Windows;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Assemblies;

namespace SN.withSIX.Updater.Presentation.Wpf
{
    public class StartupSequence
    {
        public static void PreInit(string appName) {
            new SplashScreen(@"six_updater_splash.png").Show(false);
            new RuntimeCheck().Check();
            SetupNlog.Initialize(appName);
            new AssemblyHandler().Register();
            Common.App.Init(appName);
        }

        public static void Start(IWpfStartupManager startupManager) {
            startupManager.RegisterServices();
        }

        public static Task Exit(IStartupManager startupManager) {
            return startupManager.Exit();
        }
    }
}