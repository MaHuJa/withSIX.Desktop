// <copyright company="SIX Networks GmbH" file="ToolsInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Applications.Attributes;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Core.Applications.Services
{
    public class ToolsInstaller : FilesInstaller, IToolsInstaller, IApplicationService
    {
        public const string ToolsFile = "Tools.7z";
        static readonly string[] bitools = {"DSCheckSignatures.exe", "DSCreateKey.exe", "DSSignFile.exe"};
        static readonly string[] cygwinBin = {
            "cygcom_err-2.dll",
            "cygcrypt-0.dll",
            "cygcrypto-1.0.0.dll",
            "cygcurl-4.dll",
            "cygexpat-1.dll",
            "cyggcc_s-1.dll",
            "cyggssapi_krb5-2.dll",
            "cygiconv-2.dll",
            "cygidn-11.dll",
            "cygintl-8.dll",
            "cygk5crypto-3.dll",
            "cygkrb5-3.dll",
            "cygkrb5support-0.dll",
            "cyglber-2-4-2.dll",
            "cygldap-2-4-2.dll",
            "cygncursesw-10.dll",
            "cygreadline7.dll",
            "cygsasl2-3.dll",
            //"cygsqlite3-0.dll", // srsly?
            "cygssh2-1.dll",
            "cygssl-1.0.0.dll",
            "cygssp-0.dll",
            "cygstdc++-6.dll",
            "cygwin1.dll",
            "cygz.dll",
            "lftp.exe",
            "rsync.exe",
            "ssh.exe",
            "zsync.exe",
            "zsyncmake.exe"
        };
        static readonly string[] mingwBin = {
            "msys-1.0.dll",
            "msys-crypt-0.dll",
            "msys-crypto-1.0.0.dll",
            "msys-iconv-2.dll",
            "msys-intl-8.dll",
            "msys-minires.dll",
            "msys-popt-0.dll",
            "msys-ssl-1.0.0.dll",
            "msys-termcap-0.dll",
            "msys-z.dll",
            "rsync.exe",
            "ssh.exe",
            "ssh-add.exe",
            "ssh-agent.exe",
            "ssh-keygen.exe",
            "ssh-keyscan.exe",
            "tar.exe"
        };
        static readonly string[] pbodll = {
            "DePbo64.dll",
            "deOgg64.dll",
            "DeRapify.exe",
            "ExtractPbo.exe",
            "MakePbo.exe",
            "Rapify.exe"
        };
        static readonly string[] tools = {
            "gzip.exe",
            "plink.exe"
        };
        readonly IAbsoluteDirectoryPath _destination;
        Version _availableVersion;
        Version _installedVersion;

        public ToolsInstaller(IFileDownloader downloader, IRestarter restarter, IAbsoluteDirectoryPath destinationPath)
            : base(downloader, restarter) {
            _destination = destinationPath;
        }

        public async Task DownloadAndInstallTools(StatusRepo repo) {
            await
                DownloadAndInstall(repo, ToolsFile, _destination.ParentDirectoryPath).ConfigureAwait(false);

            CheckPaths();

            if (!CheckToolFilesExist()) {
                throw new ToolsFilesInstallFailedException(
                    "Something appears to have gone wrong, some tools are missing, please try again :(");
            }

            await UpdateInstalledVersionInfo().ConfigureAwait(false);
        }

        public async Task<bool> ConfirmToolsInstalled(bool thoroughCheck) {
            if (thoroughCheck) {
                if (_destination.Exists && await CheckToolsVersion()
                    && CheckToolFilesExist())
                    return true;
            } else if (_destination.Exists && await CheckToolsVersion())
                return true;
            return false;
        }

        static Version GetDesiredToolsVersion() {
            var customAttribute =
                (ToolsVersionAttribute)
                    typeof (ToolsInstaller).Assembly.GetCustomAttribute(typeof (ToolsVersionAttribute));
            return customAttribute.Version.ToVersion();
            /*_settings.AppOptions.EnableBetaUpdates
                           ? GetVersionInfoRoot().Beta
                           : GetVersionInfoRoot().Stable;*/
        }

        async Task<bool> CheckToolsVersion() {
            await UpdateInstalledVersionInfo().ConfigureAwait(false);
            if (_availableVersion == null)
                return CheckToolFilesExist();
            return _installedVersion == _availableVersion;
        }

        bool CheckToolFilesExist() {
            return _destination.Exists &&
                   VerifyAllExist(tools, _destination.ToString()) &&
                   VerifyAllExist(cygwinBin, Path.Combine(_destination.ToString(), "cygwin", "bin")) &&
                   VerifyAllExist(mingwBin, Path.Combine(_destination.ToString(), "mingw", "bin")) &&
                   VerifyAllExist(bitools, Path.Combine(_destination.ToString(), "bitools")) &&
                   VerifyAllExist(pbodll, Path.Combine(_destination.ToString(), "pbodll"));
        }

        async Task UpdateInstalledVersionInfo() {
            _availableVersion = GetDesiredToolsVersion();
            _installedVersion = await ReadInstalledVersion().ConfigureAwait(false);
        }

        Task<Version> ReadInstalledVersion() {
            var versionFile = Path.Combine(_destination.ToString(), "version.txt");
            return Task.Run(() => {
                if (!File.Exists(versionFile))
                    return null;
                var lines = File.ReadAllLines(versionFile);
                return lines.Any() ? lines[0].ToVersion() : null;
            });
        }

        static bool VerifyAllExist(IEnumerable<string> files, string path) {
            return files.All(tool => File.Exists(Path.Combine(path, tool)));
        }

        void CheckPaths() {
            CheckPathsInternal(Path.Combine(_destination.ToString(), "cygwin"));
            CheckPathsInternal(Path.Combine(_destination.ToString(), "mingw"));
        }

        static void CheckPathsInternal(string bp) {
            Path.Combine(bp, "home").MakeSurePathExists();
            Path.Combine(bp, "tmp").MakeSurePathExists();
            Path.Combine(bp, "etc").MakeSurePathExists();
        }
    }

    public class ToolsFilesInstallFailedException : Exception
    {
        public ToolsFilesInstallFailedException(string message) : base(message) {}
    }
}