// <copyright company="SIX Networks GmbH">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using FakeItEasy;
using NLog;
using NLog.Config;
using Six.Core;
using Six.Core.Applications.Connect;
using Six.Core.Logging;
using Six.Core.Options;
using Six.Foundation.Interfaces.Domain;


namespace SN.withSIX.Play.Tests.Core.Support
{
    public class SharedSupport
    {
        public static readonly SharedSupport Instance = new SharedSupport();

        SharedSupport() {
            SimpleConfigurator.ConfigureForConsoleLogging(LogLevel.Info); // Doesnt seem to work anymore?!
        }

        // All shared initialization
        public void Init() {
            var curDir = Directory.GetCurrentDirectory();
            var roamingData = Path.Combine(curDir, "RoamingData");
            var localData = Path.Combine(curDir, "LocalData");
            var temp = Path.Combine(curDir, "Temp");

            // Cleanup data from previous tests
            if (Directory.Exists(roamingData))
                Tools.FileUtil.Delete(roamingData);
            if (Directory.Exists(localData))
                Tools.FileUtil.Delete(localData);
            if (Directory.Exists(temp))
                Tools.FileUtil.Delete(temp);

            roamingData.MakeSurePathExists();
            localData.MakeSurePathExists();
            temp.MakeSurePathExists();

            // Use to Reset the various common instances
            // Normally the EA instance is created in the AppBootStrapper, and Dependency injected into ShellViewModel
            Tools.RegisterFactoryServices(new ProcessManager(GetAssemblyLoader()), new Lazy<WCFClient>(),
                new Lazy<ISaveETag>());
            Common.App = new Common.AppCommon();
            Logging.LogManager = new DefaultLogManager();
            Common.Paths = new PathConfiguration();
            // Must set AppPath to curdir, otherwise we end up somewhere in test heaven
            // Also keep the test configuration and temp data separate from production
            Common.App.Init("Test Runner", curDir, roamingData, localData, temp);
            Common.App.Events = new EventAggregator();
            UserSettings.Reload();
            // TODO
            //SyncManager.InitAsync(false).WaitAndUnwrapException(); // Without loading from ...Disk... not sure.

            ReSetupTools();

            //LocalMachineInfo.Current.Update();
            HandleGameSet(); // Initialization order workaround, hail lazy/singleton behavior :S
            //UserSettings.Current.ActiveGame.CalculatedSettings.Update();
        }

        void ReSetupTools() {
            Tools.Generic = new Tools.GenericTools();
            Tools.Dialog = new Tools.DialogTools();
            Tools.HashEncryption = new Tools.HashEncryptionTools();
            Tools.Processes = new Tools.ProcessesTools();
            Tools.Transfer = new Tools.TransferTools(new ProcessManager(GetAssemblyLoader()), new Lazy<ISaveETag>());
        }

        void HandleGameSet() {
            // TODO
            //UserSettings.Current.ActiveGame = GameSetList.Instance.SelectedItem; // TODO
        }

        public void HandleTools() {
            var curDir = Directory.GetCurrentDirectory();
            var localData = Path.Combine(curDir, "LocalData");
            var solutionDir = new DirectoryInfo(curDir).Parent.Parent.Parent.FullName; // Solution/Project/bin/debug

            Tools.FileUtil.CopyDirectory(Path.Combine(solutionDir, "tools"), Path.Combine(localData, "tools"));
        }

        public IAssemblyLoader GetAssemblyLoader() {
            //When class used Assembly.GetEntryAssembly() run in unit test, the Assembly.GetEntryAssembly() is null.
            var loader = A.Fake<IAssemblyLoader>();
            A.CallTo(() => loader.GetEntryAssembly())
                .Returns(Assembly.LoadFrom("Six.Core.dll"));

            return loader;
        }

        public T CreatePrivateObject<T>(object[] pars, int ctorIdx = 0) {
            return (T) typeof (T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[ctorIdx]
                .Invoke(pars);
        }
    }

    public class TestUser : User {}

    public class TestGroup : Group {}

    public class TestModelBase : ModelBase {}

    public class TestEntity : Entity
    {
        public override void VisitProfile() {}
    }
}