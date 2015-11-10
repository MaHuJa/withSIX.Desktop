// <copyright company="SIX Networks GmbH" file="SquirrelUpdater.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Core;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Infra.Api;
using Splat;
using Squirrel;

namespace SN.withSIX.Mini.Presentation.Wpf.Services
{
    public class PlaySquirrel : IPlaySquirrel, IPresentationService
    {
        public async Task<Version> GetNewVersion() {
            var updateInfo = await new SquirrelUpdater().CheckForUpdates().ConfigureAwait(false);
            return NotEqualVersions(updateInfo) && HasFutureReleaseEntry(updateInfo)
                ? updateInfo.FutureReleaseEntry.Version.Version
                : null;
        }

        static bool HasFutureReleaseEntry(UpdateInfo updateInfo) {
            return updateInfo.FutureReleaseEntry != null;
        }

        static bool NotEqualVersions(UpdateInfo updateInfo) {
            return updateInfo.FutureReleaseEntry != updateInfo.CurrentlyInstalledVersion;
        }
    }

    public class SquirrelUpdater : ISquirrelUpdater, IPresentationService
    {
        static readonly Uri CdnUrl2 = new Uri("http://cdn2.withsix.com");

        static SquirrelUpdater() {
            // TODO: Read beta from the executable informationalversion... ?
            var releaseInfo = GetReleaseInfo();
            Info = new SquirrelInfo {
                Uri = new Uri(CdnUrl2, "/software/withSIX/drop/sync" + releaseInfo.Folder),
                Package = "sync" + releaseInfo.Txt
            };
        }

        static SquirrelInfo Info { get; }

        public async Task<UpdateInfo> CheckForUpdates() {
            using (var mgr = GetUpdateManager())
                return await mgr.CheckForUpdate().ConfigureAwait(false);
        }

        public async Task<ReleaseEntry> UpdateApp(Action<int> progressAction) {
            using (var mgr = GetUpdateManager())
                return await mgr.UpdateApp(progressAction).ConfigureAwait(false);
        }

        public void HandleStartup(IReadOnlyCollection<string> arguments) {
            try {
                HandleArguments(arguments).WaitAndUnwrapException();
            } catch (Exception ex) {
                MainLog.Logger.Write(ex.Format(), LogLevel.Error);
            }

            using (var mgr = GetUpdateManager())
            {
                // Note, in most of these scenarios, the app exits after this method
                // completes!
                SquirrelAwareApp.HandleEvents(
                  onInitialInstall: v => InitialInstall(mgr),
                  onAppUpdate: v => Update(mgr),
                  onAppUninstall: v => mgr.RemoveShortcutForThisExe(),
                  onFirstRun: () => Consts.FirstRun = true);
            }
        }

        static void Update(UpdateManager mgr) {
            mgr.CreateShortcutForThisExe();
        }

        static void InitialInstall(UpdateManager mgr) {
            RunVcRedist();
            mgr.CreateShortcutForThisExe();
        }

        static void RunVcRedist() {
            using (var pm = new ProcessManager()) {
                pm.StartAndForget(
                    new ProcessStartInfo(
                        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "vcredist_x86.exe"),
                        "/q /norestart"));
                pm.StartAndForget(
                    new ProcessStartInfo(
                        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "vcredist_x86-2012exe"),
                        "/q /norestart"));
            }
        }

        static async Task HandleArguments(IReadOnlyCollection<string> arguments) {
            // TODO: Handle Tools Setup etc, but needs Container etc?
            /*
                new { Key = "--squirrel-install", Value = onInitialInstall ?? defaultBlock },
                new { Key = "--squirrel-updated", Value = onAppUpdate ?? defaultBlock },
                new { Key = "--squirrel-obsolete", Value = onAppObsoleted ?? defaultBlock },
                new { Key = "--squirrel-uninstall", Value = onAppUninstall ?? defaultBlock },
            if (args[0] == "--squirrel-firstrun") {
            */
/*            if (arguments.Contains("--squirrel-install")) {
                SetupApiPort();
            } else if (arguments.Contains("--squirrel-updated")) {
                SetupApiPort();
            }*/
        }

        static string GetResourcePath(Assembly assembly, string path) {
            return assembly.GetName().Name + "." +
                   path.Replace("/", ".").Replace("\\", ".");
        }

        public static void SetupApiPort(string value, string valueHttp, ProcessManager pm) {
            var tmpFolder = Path.GetTempPath().ToAbsoluteDirectoryPath().GetChildDirectoryWithName(Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpFolder.ToString());

            var encoding = Encoding.UTF8;

            var serverPfx = "server.pfx";
            var pfxFile = tmpFolder.GetChildFileWithName(serverPfx);
            var assembly = typeof(Startup).Assembly;
            using (var s = assembly.GetManifestResourceStream(GetResourcePath(assembly, serverPfx)))
            using (var f = new FileStream(pfxFile.ToString(), FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                s.CopyTo(f);

            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var acct = sid.Translate(typeof(NTAccount)) as NTAccount;

            var commands = new[] {
                "",
                "chcp 65001",
                "cd \"" + tmpFolder + "\"",
                "netsh http add urlacl url=http://" + valueHttp + "/ user=\"" + acct + "\" > install.log",
                "netsh http add urlacl url=https://" + value + "/ user=\"" + acct + "\" >> install.log",
                "certutil -p localhost -importPFX server.pfx" + " >> install.log",
                "netsh http add sslcert ipport=" + value + " appid={12345678-db90-4b66-8b01-88f7af2e36bf} certhash=fca9282c0cd0394f61429bbbfdb59bacfc7338c9" + " >> install.log"
            };

            var batFile = tmpFolder.GetChildFileWithName("install.bat");
            var commandBat = string.Join("\r\n", commands);
            MainLog.Logger.Write("account name:" + acct, LogLevel.Info);
            MainLog.Logger.Write("install.bat content:\n" + commandBat, LogLevel.Info);
            File.WriteAllText(batFile.ToString(), commandBat, encoding);

            using (var p =
                pm.Start(
                    new ProcessStartInfoBuilder(batFile) {
                        AsAdministrator = true,
                        WorkingDirectory = tmpFolder.ToString()
                    }
                        .Build
                        ())) p.WaitForExit() ;
            var logFile = tmpFolder.GetChildFileWithName("install.log");
            var output = File.ReadAllText(logFile.ToString(), encoding);

            MainLog.Logger.Write("install.bat output:\n" + output, LogLevel.Info);

            tmpFolder.DirectoryInfo.Delete(true);
        }

        static ReleaseInfo GetReleaseInfo() {
            return new ReleaseInfo(GetTxt());
        }

        static string GetTxt() {
            switch (Common.AppCommon.Type) {
            case ReleaseType.Alpha:
                return "alpha";
            case ReleaseType.Beta:
                return "beta";
            case ReleaseType.Dev:
                return "dev";
            case ReleaseType.Stable:
                return String.Empty;
            default: {
                throw new NotSupportedException("Release type unknown: " + Common.AppCommon.Type);
            }
            }
        }

        public static UpdateManager GetUpdateManager() {
            return new UpdateManager(
                Info.Uri.ToString(),
                Info.Package);
        }

        class ReleaseInfo
        {
            public ReleaseInfo(string txt) {
                Txt = txt;
                Folder = string.IsNullOrEmpty(txt) ? String.Empty : "/" + txt;
                PostFix = string.IsNullOrEmpty(txt) ? String.Empty : "-" + txt;
            }

            public string Txt { get; }
            public string Folder { get; }
            public string PostFix { get; private set; }
        }

        class SquirrelInfo
        {
            public Uri Uri { get; set; }
            public string Package { get; set; }
        }
    }


    class SetupLogLogger : ILogger, IDisposable
    {
        readonly object gate = 42;
        readonly StreamWriter inner;

        public SetupLogLogger(bool saveInTemp) {
            var dir = saveInTemp
                ? Path.GetTempPath()
                : Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var file = Path.Combine(dir, "SquirrelSetup.log");
            if (File.Exists(file))
                File.Delete(file);

            inner = new StreamWriter(file, false, Encoding.UTF8);
        }

        public void Dispose() {
            lock (gate)
                inner.Dispose();
        }

        public LogLevel Level { get; set; }

        public void Write(string message, LogLevel logLevel) {
            if (logLevel < Level)
                return;

            lock (gate)
                inner.WriteLine(message);
        }
    }
}