// <copyright company="SIX Networks GmbH" file="StartupSequence.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reflection;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Assemblies;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Sync.Presentation.Console
{
    // WARNING - DO NOT SHOW NON-FATAL DIALOGS HERE
    public static class StartupSequence
    {
        public static void PreInit(string appName) {
            CommonBase.AssemblyLoader = new AssemblyLoader(Assembly.GetEntryAssembly());
            new RuntimeCheck().Check();
            SetupNlog.Initialize(appName);
            new AssemblyHandler().Register();
            Common.App.Init(appName);
        }

        public static void Start(IStartupManager startupManager) {
            startupManager.RegisterServices();
        }

        public static Task Exit(IStartupManager startupManager) {
            return startupManager.Exit();
        }
    }
}