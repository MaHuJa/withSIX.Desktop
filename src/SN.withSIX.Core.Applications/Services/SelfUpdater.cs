// <copyright company="SIX Networks GmbH" file="SelfUpdater.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;
using NDepend.Path;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Core.Applications.Services
{
    public class SelfUpdater : ISelfUpdater, IEnableLogging, IApplicationService
    {
        static readonly bool singleExeMode = true;
        static readonly string ProgramName = "Play withSIX";
        static readonly string CompanyName = "Six Networks";
        static readonly string BasePath = CommonUrls.RemoteSoftwarePath;
        static readonly string Exe = "Play withSIX update.exe";
        static readonly string VersionInfoFilename = "versionInfo.txt";
        static readonly string VersionInfoFilenameBeta = "versionInfoBeta.txt";
        static readonly string VersionInfoFilenameNightly = "versionInfoNightly.txt";
        static readonly string SingleExeModeFilenameModifier = "Se";
        public static readonly string PlayExecutable = "withSIX-Play.exe";
        static readonly string RegistryPathBase = @"SOFTWARE\SIX Networks\";
        static readonly string applicationRegistryPath = RegistryPathBase + Common.AppCommon.ApplicationName;
        readonly IFileDownloader _downloader;
        readonly Func<bool> _enableBeta;
        readonly Version _entryAssemblyLocalVersion;
        readonly IAbsoluteFilePath _entryAssemblyLocation;
        readonly IEventAggregator _eventBus;
        readonly string _localProductCode;
        readonly IProcessManager _processManager;
        readonly IRestarter _restarter;
        readonly IAbsoluteFilePath _versionInfoFile =
            Common.Paths.LocalDataPath.GetChildFileWithName(VersionInfoFilename);
        readonly ExportFactory<IWebClient> _webClientFactory;
        volatile bool _newVersionDownloaded;
        string _remoteProductCode = string.Empty;

        public SelfUpdater(Func<bool> enableBeta, IEventAggregator eventBus, IProcessManager processManager,
            IFileDownloader downloader,
            ExportFactory<IWebClient> webClientFactory, IRestarter restarter) {
            _eventBus = eventBus;
            _processManager = processManager;
            _downloader = downloader;
            _webClientFactory = webClientFactory;
            _restarter = restarter;
            _enableBeta = enableBeta;
            Status = new TransferStatus("Play withSIX update.exe");
            Destination = Common.Paths.TempPath.GetChildFileWithName(Exe);
            _localProductCode = GetLocalProduct();
            _entryAssemblyLocation = Common.Paths.EntryLocation;
            _entryAssemblyLocalVersion = GetLocalVersion();
        }

        public static Func<Task> Extend { get; set; }

        public Version GetLocalVersion() {
            return singleExeMode
                ? CommonBase.AssemblyLoader.GetEntryVersion()
                : GetLocalRegVersion();
        }

        public ITransferProgress Status { get; }
        public IAbsoluteFilePath Destination { get; }
        public string RemoteVersion { get; set; } = string.Empty;
        public bool NewVersionDownloaded
        {
            get { return _newVersionDownloaded; }
        }
        public IAbsoluteDirectoryPath InstallPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), CompanyName, ProgramName)
            .ToAbsoluteDirectoryPath();
        public bool IsRunning { get; private set; }

        public bool ExistsAndIsValid(IAbsoluteFilePath exePath) {
            if (!Destination.Exists)
                return false;

            var versionInfoFile = Path.Combine(Common.Paths.LocalDataPath.ToString(), VersionInfoFilename);
            if (!File.Exists(versionInfoFile)) {
                Cleanup();
                return false;
            }

            string remoteHash;
            using (var myFile = new StreamReader(versionInfoFile)) {
                var remoteVersionInfo = myFile.ReadToEnd();
                var versionInfo = remoteVersionInfo.TrimEnd('\n', '\r').Split(':');
                try {
                    RemoteVersion = versionInfo[0];
                    _remoteProductCode = versionInfo[1];
                    remoteHash = versionInfo[2];
                } catch (Exception e) {
                    this.Logger().FormattedWarnException(e);
                    return false;
                }
            }

            var localV = GetAppLocalVersion(exePath);
            var remoteV = TryParseVersion(RemoteVersion);

            if (remoteV == null || (localV != null && localV >= remoteV)) {
                Cleanup();

                return false;
            }

            var localHash = Tools.HashEncryption.MD5FileHash(Destination);
            if (remoteHash != localHash) {
                Cleanup();
                throw new Exception("Self Update download failed: Hash doesn't match: " + localHash + ", vs: " +
                                    remoteHash);
                return false;
            }

            return true;
        }

        public async Task<bool> ProgramApplyUpdateIfExists(IAbsoluteFilePath exePath) {
            try {
                return await ApplyUpdateIfExists(exePath).ConfigureAwait(false);
            } catch (UnauthorizedAccessException e) {
                if (!Tools.Processes.Uac.CheckUac()) {
                    Cleanup();
                    throw;
                }
                this.Logger().FormattedWarnException(e);
                _restarter.RestartWithUacInclEnvironmentCommandLine();
                return false;
            } catch (Exception) {
                Cleanup();
                throw;
            }
        }

        public Task CheckForUpdate() {
            PublishDomainEvent(new CheckingForNewVersion());
            return TryCheckForUpdate();
        }

        public bool PerformSelfUpdate(string action = SelfUpdaterCommands.UpdateCommand, params string[] args) {
            return TryRunUpdater(Common.Paths.SelfUpdaterExePath,
                new[] {action, _entryAssemblyLocation.ToString()}.Concat(args).ToArray());
        }

        public bool IsInstalled() {
            //TODO Improve
            if (IsLegacyInstalled())
                return false;
            var localKey = Registry.LocalMachine.OpenSubKey(Common.AppCommon.ApplicationRegKey);
            return localKey != null && File.Exists(((string) localKey.GetValue("Path", "")).ToLower());
        }

        public bool IsLegacyInstalled() {
            return GetLocalProduct() != null;
        }

        public bool IsLocalUpdate(string exePath) {
            var localKey = Registry.LocalMachine.OpenSubKey(Common.AppCommon.ApplicationRegKey);
            if (localKey == null)
                return false;
            return ((string) localKey.GetValue("Path", "")).ToLower() == exePath.ToLower();
        }

        public bool ApplyInstallIfRequired(IAbsoluteFilePath exePath) {
            if (_localProductCode != null) {
                if (!UninstallLegacyMSI())
                    return false;
                CleanupLegacyRegistry();
            }
            return IsInstalled() || InstallToProgramFiles(exePath);
        }

        public bool UninstallSingleExe() {
            var localPath = GetLocalPath();
            if (localPath == null || !Tools.FileUtil.CheckFile(localPath, true, true))
                return false;
            var key = Registry.LocalMachine.OpenSubKey("Software\\SIX Networks\\");
            key.DeleteSubKey(Common.AppCommon.ApplicationName);


            Tools.FileUtil.Ops.DeleteFile(localPath.ToAbsoluteFilePath(), true);
            DeleteApplicationShortcuts();

            Tools.FileUtil.Ops.DeleteAfterRestart(AppDomain.CurrentDomain.BaseDirectory);
            return true;
        }

        async Task<bool> ApplyUpdateIfExists(IAbsoluteFilePath exePath) {
            var exePathStr = exePath.ToString();
            if (ExistsAndIsValid(exePath)) {
                await Tools.Processes.WaitForExitALittleMore(Path.GetFileName(exePathStr), 30).ConfigureAwait(false);
                var applied = Apply(exePath);
                if (IsLocalUpdate(exePathStr))
                    RegisterLocalAppKeys(exePath);
                return applied;
            }
            Cleanup();
            return false;
        }

        void Cleanup() {
            CleanupDest();
            CleanupInfo();
        }

        void CleanupDest() {
            if (Destination.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(Destination.ToString());
        }

        void CleanupInfo() {
            if (_versionInfoFile.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(_versionInfoFile.ToString());
        }

        bool Apply(IAbsoluteFilePath exePath = null) {
            return singleExeMode ? ApplySingleExeMode(exePath) : ApplyMSI();
        }

        static string GetLocalProduct() {
            return Tools.Generic.GetRegKeyValue<string>(applicationRegistryPath, "product");
        }

        async Task TryCheckForUpdate() {
            try {
                IsRunning = true;
                if (Extend == null) {
                    await
                        VersionCheckerComplete(await CheckForUpdateInternal().ConfigureAwait(false))
                            .ConfigureAwait(false);
                    // TODO: This is called multiple times overlapping apparently ;-)
                } else
                    await Extend().ConfigureAwait(false);
            } catch (Exception e) {
                PublishDomainEvent(new NoNewVersionAvailable(null));
                IsRunning = false;
                throw;
            }
        }

        void PublishDomainEvent<TEvent>(TEvent evt) where TEvent : IDomainEvent {
            _eventBus.PublishOnCurrentThread(evt);
            Common.App.PublishDomainEvent(evt);
        }

        static void RegisterLocalAppKeys(IAbsoluteFilePath exePath) {
            var key = Registry.LocalMachine.CreateSubKey(Common.AppCommon.ApplicationRegKey);
            key.SetValue("Path", exePath);
            key.SetValue("Version", Tools.FileUtil.GetVersion(exePath).ToString());
        }

        static Version GetLocalRegVersion() {
            return TryParseVersion(Tools.Generic.GetRegKeyValue<string>(applicationRegistryPath, "version"));
        }

        static Version TryParseVersion(string version) {
            if (version == null)
                return null;
            Version remoteV;
            Version.TryParse(version, out remoteV);
            return remoteV;
        }

        Version GetAppLocalVersion(IAbsoluteFilePath exePath) {
            if (!singleExeMode)
                return GetLocalRegVersion();
            return exePath.Exists ? TryGetVersionFromFile(exePath) : null;
        }

        Version TryGetVersionFromFile(IAbsoluteFilePath exePath) {
            try {
                return Tools.FileUtil.GetVersion(exePath);
            } catch (Exception e) {
                this.Logger()
                    .FormattedWarnException(e, string.Format("failed to get app local version from {0}", exePath));
                return new Version();
            }
        }

        [ReportUsage("ApplySelfUpdate")]
        bool ApplyMSI() {
            using (var p = new Process {
                StartInfo = {
                    FileName = Destination.ToString(),
                    Arguments = GetApplyMsiParameters(),
                    //WorkingDirectory = ,
                    UseShellExecute = true
                }
            }) {
                p.Start();
                p.WaitForExit();
            }
            CleanupInfo();
            return true;
        }

        string GetApplyMsiParameters() {
            return !String.IsNullOrWhiteSpace(_localProductCode) && _localProductCode == _remoteProductCode
                ? "/s /vREINSTALL=ALL /vREINSTALLMODE=vomus /v/qb"
                : "/s";
        }

        bool UninstallLegacyMSI() {
            return _localProductCode == null || TryUninstallLegacyMSI(GetUninstallParameters());
        }

        string GetUninstallParameters() {
            return "/x " + _localProductCode;
        }

        bool TryUninstallLegacyMSI(string parameters) {
            try {
                using (
                    var p = new Process {
                        StartInfo = {
                            FileName =
                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "system32",
                                    "msiexec.exe"),
                            Arguments = parameters,
                            //WorkingDirectory = ,
                            UseShellExecute = true
                        }
                    }) {
                    p.Start();
                    p.WaitForExit();
                }
                CleanupInfo();
                return true;
            } catch (Exception e) {
                Cleanup();
                this.Logger().FormattedWarnException(e);
                return false;
            }
        }

        bool TryRunUpdater(IAbsoluteFilePath executable, params string[] parameters) {
            try {
                _processManager.StartAndForget(new ProcessStartInfo(executable.ToString(),
                    parameters.CombineParameters()));
                return true;
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
                return false;
            }
        }

        [ReportUsage("ApplySelfUpdateSingle")]
        bool ApplySingleExeMode(IAbsoluteFilePath exePath) {
            Tools.FileUtil.Ops.Move(Destination, exePath);
            CleanupInfo();
            return true;
        }

        string GetLocalPath() {
            return Tools.Generic.GetRegKeyValue<string>(applicationRegistryPath, "path");
        }

        async Task<VersionCheckCompleteEventArgs> CheckForUpdateInternal() {
            using (var exportedClient = _webClientFactory.CreateExport()) {
                var checkForUpdateClient = exportedClient.Value;
                var uri = GetVersionCheckUri(_enableBeta());
                return await TryCheckForUpdate(checkForUpdateClient, uri).ConfigureAwait(false);
            }
        }

        async Task<VersionCheckCompleteEventArgs> TryCheckForUpdate(IWebClient checkForUpdateClient, Uri uri) {
            try {
                return await DownloadAndProcessVersion(checkForUpdateClient, uri).ConfigureAwait(false);
            } catch (Exception) {
                return OnComplete(null, null, null, false, false);
            }
        }

        async Task<VersionCheckCompleteEventArgs> DownloadAndProcessVersion(IWebClient checkForUpdateClient, Uri uri) {
            var result = await checkForUpdateClient.DownloadStringTaskAsync(uri).ConfigureAwait(false);
            var versionInfo = result.TrimEnd('\n', '\r').Split(':');
            return TryParseVersionInfo(versionInfo);
        }

        VersionCheckCompleteEventArgs TryParseVersionInfo(params string[] versionInfo) {
            try {
                Version serverVersion;
                if (!Version.TryParse(versionInfo[0], out serverVersion))
                    return OnComplete(null, null, null, false, false);

                var thisVersion = _entryAssemblyLocalVersion;
                var isNew = thisVersion == null || serverVersion > thisVersion;

                return OnComplete(versionInfo[0], versionInfo[1], versionInfo[2], isNew,
                    versionInfo[1] == _localProductCode);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
                return OnComplete(null, null, null, false, false);
            }
        }

        static VersionCheckCompleteEventArgs OnComplete(string newVersion, string newProductCode, string hash,
            bool isNew,
            bool isSameProductCode) {
            return new VersionCheckCompleteEventArgs {
                Version = newVersion,
                ProductCode = newProductCode,
                Hash = hash,
                IsNew = isNew,
                IsSameProductCode = isSameProductCode
            };
        }

        static Uri GetVersionCheckUri(bool beta = false) {
            var fileString = Common.AppCommon.Type < ReleaseType.Beta
                ? VersionInfoFilenameNightly
                : (beta
                    ? VersionInfoFilenameBeta
                    : VersionInfoFilename);

            if (singleExeMode)
                fileString = SingleExeModeFilenameModifier + fileString;

            return Tools.Transfer.JoinUri(CommonUrls.CdnUrl2, BasePath, fileString);
        }

        async Task VersionCheckerComplete(VersionCheckCompleteEventArgs args) {
            if (args.IsNew) {
                if (_versionInfoFile.Exists)
                    Tools.FileUtil.Ops.DeleteWithRetry(_versionInfoFile.ToString());

                using (var file = new StreamWriter(_versionInfoFile.ToString()))
                    file.WriteLine(args.Version + ":" + args.ProductCode + ":" + args.Hash);

                PublishDomainEvent(new NewVersionAvailable(TryParseVersion(args.Version)));

                await DownloadUpdate(args).ConfigureAwait(false);
            } else {
                PublishDomainEvent(new NoNewVersionAvailable(TryParseVersion(args.Version)));
                IsRunning = false;
            }
        }

        async Task DownloadUpdate(VersionCheckCompleteEventArgs a) {
            if (Destination.Exists) {
                if (Tools.HashEncryption.MD5FileHash(Destination) == a.Hash) {
                    _newVersionDownloaded = true;
                    PublishDomainEvent(new NewVersionDownloaded(TryParseVersion(a.Version)));
                    return;
                }
                Tools.FileUtil.Ops.DeleteWithRetry(Destination.ToString());
            }

            var filePath = BasePath + "/";
            if (singleExeMode)
                filePath += SingleExeModeFilenameModifier;

            var nightly = 
#if NIGHTLY_RELEASE
                " nightly";
#else
                "";
#endif

            filePath += String.Format("Play withSIX v{0}{1} update.exe", a.Version, nightly);

            await TryDownloadUpdate(a, filePath).ConfigureAwait(false);
        }

        async Task TryDownloadUpdate(VersionCheckCompleteEventArgs a, string filePath) {
            try {
                await
                    _downloader.DownloadAsync(Tools.Transfer.JoinUri(CommonUrls.CdnUrl2, filePath), Destination,
                        Status).ConfigureAwait(false);
                _newVersionDownloaded = true;
                PublishDomainEvent(new NewVersionDownloaded(TryParseVersion(a.Version)));
            } catch (Exception e) {
                Status.UpdateOutput("Error downloading: " + e.Message);
                this.Logger().FormattedWarnException(e);
            } finally {
                IsRunning = false;
            }
        }

        bool InstallToProgramFiles(IAbsoluteFilePath exeFile) {
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                CompanyName, ProgramName).ToAbsoluteDirectoryPath();
            var installExe = InstallPath.GetChildFileWithName(PlayExecutable);

            if (Tools.FileUtil.ComparePathsOsCaseSensitive(exeFile.ToString(), installExe.ToString()))
                return false;

            InstallPath.MakeSurePathExists();
            Tools.FileUtil.Ops.CopyWithRetry(exeFile, installExe);
            shortcutPath.MakeSurePathExists();
            CreateShortcut(shortcutPath, installExe);
            CreateShortcut(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).ToAbsoluteDirectoryPath(),
                installExe);
            RegisterLocalAppKeys(installExe);

            return true;
        }

        void CreateShortcut(IAbsoluteDirectoryPath destinationPath, IAbsoluteFilePath installExe) {
            Tools.FileUtil.CreateShortcut(new ShortcutInfo(destinationPath, "Play withSIX", installExe) {
                WorkingDirectory = InstallPath,
                Description = "Play withSIX"
            });
        }

        static void CleanupLegacyRegistry() {
            var key = Registry.LocalMachine.CreateSubKey(Common.AppCommon.ApplicationRegKey);
            key.DeleteValue("version", false);
            key.DeleteValue("product", false);
            key.DeleteValue("upgrade", false);
            key.DeleteValue("path", false);
            key.DeleteValue("version", false);
        }

        static void DeleteApplicationShortcuts() {
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                CompanyName, ProgramName);
            Tools.FileUtil.Ops.DeleteFile(Path.Combine(shortcutPath, "Play withSIX.lnk").ToAbsoluteFilePath(), true);
            Tools.FileUtil.Ops.DeleteFile(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Play withSIX.lnk")
                    .ToAbsoluteFilePath(),
                true);
        }
    }

    public class CheckingForNewVersion : IDomainEvent {}

    public class NewVersionDownloaded : IDomainEvent
    {
        public NewVersionDownloaded(Version version) {
            Version = version;
        }

        public Version Version { get; private set; }
    }

    public class NewVersionAvailable : IDomainEvent
    {
        public NewVersionAvailable(Version version) {
            Version = version;
        }

        public Version Version { get; private set; }
    }

    public class NoNewVersionAvailable : IDomainEvent
    {
        public NoNewVersionAvailable(Version version) {
            Version = version;
        }

        public Version Version { get; private set; }
    }

    public class VersionCheckCompleteEventArgs : EventArgs
    {
        public string Version { get; set; }
        public string ProductCode { get; set; }
        public string Hash { get; set; }
        public bool IsNew { get; set; }
        public bool IsSameProductCode { get; set; }
    }
}