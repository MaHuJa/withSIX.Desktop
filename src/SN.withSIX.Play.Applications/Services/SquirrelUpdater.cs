// <copyright company="SIX Networks GmbH" file="SquirrelUpdater.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SN.withSIX.Core;
using Splat;
using Squirrel;

namespace SN.withSIX.Play.Applications.Services
{
    public class SquirrelUpdater
    {
        static readonly Uri CdnUrl2 = new Uri("http://cdn2.withsix.com");

        static SquirrelUpdater() {
            // TODO: Read beta from the executable informationalversion... ?
            var releaseInfo = GetReleaseInfo();
            Info = new SquirrelInfo {
                Uri = new Uri(CdnUrl2, "/software/withSIX/drop/rel" + releaseInfo.Folder),
                Package = "PlaywithSIX" + releaseInfo.Txt
            };
        }

        static SquirrelInfo Info { get; }

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

        public async Task<UpdateInfo> CheckForUpdates() {
            using (var mgr = GetUpdateManager())
                return await mgr.CheckForUpdate().ConfigureAwait(false);
        }

        public async Task<ReleaseEntry> UpdateApp(Action<int> progressAction) {
            using (var mgr = GetUpdateManager())
                return await mgr.UpdateApp(progressAction).ConfigureAwait(false);
        }

        public static Squirrel.UpdateManager GetUpdateManager() {
            return new Squirrel.UpdateManager(
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