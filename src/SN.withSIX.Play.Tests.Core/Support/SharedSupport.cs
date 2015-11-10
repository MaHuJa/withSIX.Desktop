// <copyright company="SIX Networks GmbH" file="SharedSupport.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Caliburn.Micro;
using FakeItEasy;
using NDepend.Path;
using NLog;
using NLog.Config;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Presentation.Assemblies;
using SN.withSIX.Core.Presentation.Services;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Sync.Core.Transfer;
using Action = System.Action;

namespace SN.withSIX.Play.Tests.Core.Support
{
    public static class SharedSupport
    {
        // All shared initialization
        public static void Init() {
            SimpleConfigurator.ConfigureForConsoleLogging(LogLevel.Info); // Doesnt seem to work anymore?!
            SetupNlog.Initialize("Test Runner");
            Common.AssemblyLoader = GetAssemblyLoader();
            // Use to Reset the various common instances
            // Normally the EA instance is created in the AppBootStrapper, and Dependency injected into ShellViewModel
            Common.App = new Common.AppCommon();
            Common.Paths = new PathConfiguration();
            // Must set AppPath to curdir, otherwise we end up somewhere in test heaven
            // Also keep the test configuration and temp data separate from production
            Common.App.InitLocalWithCleanup("Test Runner");
            Common.App.Events = new EventAggregator();
            DomainEvilGlobal.Settings = new UserSettings();
            Tools.RegisterServices(new ToolsServices(new ProcessManager(),
                new Lazy<IWCFClient>(() => new WCFClient(Common.App.Events)),
                new Lazy<IGeoIpService>(() => new GeoIpService(new ResourceService()))));
            ReSetupTools();

            if (!SingleSetup) {
                new AssemblyHandler().Register();
                SingleSetup = true;
            }
        }

        static bool SingleSetup;

        public static ExportFactory<IWebClient> CreateWebClientExportFactory() {
            return new ExportFactory<IWebClient>(OnExportLifetimeContextCreator);
        }

        public static ExportFactory<IWebClient> CreateFakeWebClientExportFactory(IWebClient webClient = null) {
            return new ExportFactory<IWebClient>(() => OnExportLifetimeFakeContextCreator(webClient));
        }

        static Tuple<IWebClient, Action> OnExportLifetimeFakeContextCreator(IWebClient webClient = null) {
            if (webClient == null)
                webClient = A.Fake<IWebClient>();
            return new Tuple<IWebClient, Action>(webClient, webClient.Dispose);
        }

        static Tuple<IWebClient, Action> OnExportLifetimeContextCreator() {
            var wc = new WebClient();
            return new Tuple<IWebClient, Action>(wc, wc.Dispose);
        }

        static void ReSetupTools() {
            Tools.Generic = new Tools.GenericTools();
            Tools.HashEncryption = new Tools.HashEncryptionTools();
            Tools.Processes = new Tools.ProcessesTools();
            Tools.Transfer = new Tools.TransferTools();
        }

        public static void HandleTools() {
            var curDir = Directory.GetCurrentDirectory();
            var localData = Path.Combine(curDir, "LocalData");
            var solutionDir = new DirectoryInfo(curDir).Parent.Parent.Parent.FullName; // Solution/Project/bin/debug

            Tools.FileUtil.Ops.CopyDirectoryWithRetry(Path.Combine(solutionDir, "tools").ToAbsoluteDirectoryPath(),
                Path.Combine(localData, "tools").ToAbsoluteDirectoryPath());
        }

        public static IAssemblyLoader GetAssemblyLoader() {
            //When class used Assembly.GetEntryAssembly() run in unit test, the Assembly.GetEntryAssembly() is null.
            var loader = A.Fake<IAssemblyLoader>();
            var scAs = Assembly.LoadFrom("SN.withSIX.Core.dll");
            A.CallTo(() => loader.GetEntryLocation())
                .Returns(scAs.Location);
            A.CallTo(() => loader.GetEntryPath())
                .Returns(Path.GetDirectoryName(loader.GetEntryLocation()));

            return loader;
        }
    }
}