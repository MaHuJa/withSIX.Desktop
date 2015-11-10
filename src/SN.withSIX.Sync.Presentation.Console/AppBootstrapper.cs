// <copyright company="SIX Networks GmbH" file="AppBootstrapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caliburn.Micro;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Extensions;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Infra.Data.Services;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Presentation.Console.Commands;

namespace SN.withSIX.Sync.Presentation.Console
{
    class AppBootstrapper : ConsoleAppBootstrapper<IConsoleLauncher>
    {
        IStartupManager _startupManager;

        protected override void PreStart() {
            _startupManager = Container.GetInstance<IStartupManager>();
            StartupSequence.Start(_startupManager);
        }

        protected override void OnExit(object sender, EventArgs e) {
            base.OnExit(sender, e);
            if (_startupManager != null)
                Task.Run(() => StartupSequence.Exit(_startupManager)).WaitAndUnwrapException();
            else
                MainLog.Logger.Error("StartupManager was null on exit!");
        }

        protected override void ConfigureContainer() {
            base.ConfigureContainer();
            Container.Register<IStartupManager, StartupManager>();
            Container.RegisterPlugins<BaseCommand>(AssemblySource.Instance);
            Container.Register<IConsoleLauncher, Program>();


            Container.RegisterSingleton<IUserSettingsStorage>(
                () =>
                    new UserSettingsStorage(
                        (ex, location) => Container.GetInstance<IDialogManager>().ExceptionDialogSync(ex,
                            String.Format(
                                "An error occurred while trying to load Settings from: {0}\nIf you continue you will loose your settings, but we will at least make a backup for you.",
                                location))));

            Container.RegisterSingleton(() => DomainEvilGlobal.Settings);
            Container.RegisterSingleton(() => DomainEvilGlobal.LocalMachineInfo);

            Container.RegisterSingleton<Func<ProtocolPreference>>(
                () => Container.GetInstance<UserSettings>().AppOptions.ProtocolPreference);
            Container
                .RegisterSingleton<Func<IMultiMirrorFileDownloader, ExportLifetimeContext<IFileQueueDownloader>>>(
                    x => {
                        var appOptions = Container.GetInstance<UserSettings>().AppOptions;
                        var downloader = new MultiThreadedFileQueueDownloader(appOptions.GetMaxThreads, x);
                        return new ExportLifetimeContext<IFileQueueDownloader>(downloader, TaskExt.NullAction);
                    });
        }

        protected override void AfterSetup() {
            base.AfterSetup();
            DomainEvilGlobal.SecretData = Task.Run(() => BuildSecretData()).Result;
            DomainEvilGlobal.Settings = Container.GetInstance<IUserSettingsStorage>().TryLoadSettings();
        }

        async Task<SecretData> BuildSecretData() {
            var cm = Container.GetInstance<ISecureCacheManager>();
            var authKey = "Authentication";
            var userInfoKey = "UserInfo";

            var secretData = new SecretData {
                Authentication = await cm.GetOrCreateObject(authKey, () => new AuthenticationData()),
                UserInfo = await cm.GetOrCreateObject(userInfoKey, () => new UserInfo())
            };
            secretData.Save = async () => {
                await cm.SetObject(authKey, secretData.Authentication);
                await cm.SetObject(userInfoKey, secretData.UserInfo);
            };
            return secretData;
        }

        protected override IEnumerable<Assembly> SelectAssemblies() {
            return new[] {
                Assembly.GetExecutingAssembly(),
                typeof (StartupManager).Assembly,
                typeof (UserSettingsStorage).Assembly,
                typeof (Repository).Assembly,
                typeof (ToolsInstaller).Assembly,
                typeof (WindowManager).Assembly
            }.Concat(base.SelectAssemblies()).Distinct();
        }
    }
}