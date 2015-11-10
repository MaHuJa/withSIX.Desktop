// <copyright company="SIX Networks GmbH" file="Entrypoint.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using SimpleInjector;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Assemblies;
using SN.withSIX.Core.Presentation.Wpf;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Infra.Api;
using SN.withSIX.Mini.Presentation.Wpf.Services;
using Splat;
using ILogger = Splat.ILogger;
using MainLog = SN.withSIX.Mini.Applications.Core.MainLog;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    public static class Entrypoint
    {
        static AppBootstrapper _bs;
        public static bool CommandMode { get; private set; }

        public static void MainForNode() {
            SetupRegistry();
            SetupAssemblyLoader();
            VisualExtensions.Waiter = TaskExt.WaitAndUnwrapException;
            SetupLogging();
            new AssemblyHandler().Register();
            HandlePorts();
            //HandleSquirrel(arguments);
            _bs = new AppBootstrapper(new Container(), Locator.CurrentMutable);
            _bs.Startup();
        }

        public static void ExitForNode() {
            _bs.Dispose();
        }

        static void SetupAssemblyLoader() {
            CommonBase.AssemblyLoader = new AssemblyLoader(typeof (Entrypoint).Assembly);
        }

        static void SetupRegistry() {
            var registry = new AssemblyRegistry();
            AppDomain.CurrentDomain.AssemblyResolve += registry.CurrentDomain_AssemblyResolve;
        }

        [STAThread]
        public static void Main() {
            SetupAssemblyLoader();
            SetupLogging();
            new AssemblyHandler().Register();
            var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
            MainLog.Logger.Write("Startup, processing arguments: " + arguments.CombineParameters(), LogLevel.Info);
            HandleCommandMode(arguments);
            try {
                HandlePorts();
                HandleSquirrel(arguments);
            } catch (Exception ex) {
                MainLog.Logger.Write("An error occurred during processing startup:\n" + ex.Format(), LogLevel.Error);
                throw;
            }
            if (!CommandMode)
                HandleSingleInstance();
            StartApp();
        }

        static void HandleCommandMode(string[] arguments) {
            if (arguments.Any()) {
                var firstArgument = arguments.First();
                if (!firstArgument.StartsWith("-"))
                    CommandMode = true;
            }
        }

        static void HandlePorts() {
            using (var pm = new ProcessManager()) {
                var si = new ServerInfo(pm);
                if (si.AllRegistered())
                    return;
                SquirrelUpdater.SetupApiPort(ServerInfo.value, ServerInfo.valueHttp, pm);
                si = new ServerInfo(pm); // to output
            }
        }


        static void SetupLogging() {
            SetupNlog.Initialize(Consts.ProductTitle);
            var splatLogger = new NLogSplatLogger();
            Locator.CurrentMutable.Register(() => splatLogger, typeof (ILogger));
#if DEBUG
            LogHost.Default.Level = LogLevel.Debug;
#endif
        }

        static void HandleSquirrel(IReadOnlyCollection<string> arguments) {
            new SquirrelUpdater().HandleStartup(arguments);
        }

        static void HandleSingleInstance() {
            if (
                !SingleInstance<App>.TryInitializeAsFirstInstance<App>("withSIX-Sync",
                    new[] {"-NewVersion=" + Consts.ProductVersion},
                    Path.GetFileName(Assembly.GetEntryAssembly().Location)))
                // TODO; Deal with 'another version'
                Environment.Exit(0);
        }

        static void StartApp() {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}