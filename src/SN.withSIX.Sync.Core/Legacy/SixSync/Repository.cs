// <copyright company="SIX Networks GmbH" file="Repository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;

namespace SN.withSIX.Sync.Core.Legacy.SixSync
{
    public class Repository : IEnableLogging
    {
        public const string DefaultArchiveFormat = ".gz";
        public const string ConfigFileName = "config.yml";
        public static string VersionFileName = ".repository.yml";
        public static string RepoFolderName = ".rsync";
        public static string PackFolderName = ".pack";
        public static readonly RepositoryTools RepoTools = new RepositoryTools();
        public static readonly RepositoryFactory Factory =
            new RepositoryFactory(new ZsyncMake(Tools.ProcessManager, Tools.FileUtil.Ops));
        public static readonly string[] IgnoredExtensions = {
            ".zsync", ".zs-old", Tools.GenericTools.TmpExtension,
            ".part"
        };
        public static readonly string[] TempExtensions = {".zs-old", Tools.GenericTools.TmpExtension, ".part"};
        public static string[] ArchiveFormats = {DefaultArchiveFormat, ".7z"};
        public static string[] RsyncableArchiveFormats = {DefaultArchiveFormat};
        readonly IZsyncMake _zsyncMake;
        public bool KeepCompressedFiles = true;
        public Repository(IZsyncMake zsyncMake, string folder = ".") : this(zsyncMake, new StatusRepo(), folder) {}

        public Repository(IZsyncMake zsyncMake, StatusRepo statusRepo, string folder = ".") {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(folder));
            _zsyncMake = zsyncMake;
            Folder = RepoTools.GetRootedPath(folder);
            RsyncFolder = Folder.GetChildDirectoryWithName(RepoFolderName);

            if (!RsyncFolder.Exists)
                throw new Exception("Not a SixSync repository");

            ConfigFile = GetRsyncFile(ConfigFileName);
            RepoName = Folder.DirectoryName;
            StatusRepo = statusRepo;
            Output = "print";
            MultiThreadingSettings = new MultiThreadingSettings();
            CollectTransferLogs = true;

            LoadConfig(true);
            LoadVersions();
        }

        protected FileDownloadManager DownloadManager { get; set; }
        public bool CollectTransferLogs { get; set; }
        public static RepoMainConfig MainConfig { get; set; }
        public RepoVersion PackVersion { get; set; }
        public RepoVersion WdVersion { get; set; }
        public string Output { get; set; }
        public RepoConfig Config { get; set; }
        public IAbsoluteDirectoryPath Folder { get; set; }
        public MultiThreadingSettings MultiThreadingSettings { get; set; }
        public string RepoName { get; set; }
        public IAbsoluteDirectoryPath RsyncFolder { get; set; }
        public IAbsoluteDirectoryPath PackFolder { get; set; }
        public IAbsoluteFilePath WdVersionFile { get; set; }
        public IAbsoluteFilePath PackVersionFile { get; set; }
        public long? RequiredVersion { get; set; }
        public string RequiredGuid { get; set; }
        public StatusRepo StatusRepo { get; protected set; }
        protected string ArchiveFormat
        {
            get { return PackVersion.ArchiveFormat; }
            set
            {
                Contract.Requires<ArchiveFormatUnsupported>(ArchiveFormats.Any(x => x == value));

                PackVersion.ArchiveFormat = value;
                WdVersion.ArchiveFormat = value;
            }
        }
        protected IAbsoluteFilePath ConfigFile { get; set; }
        public bool AllowFullTransferFallBack { get; set; }

        public static void InitializeConfig(IAbsoluteDirectoryPath path) {
            var configFile = path.GetChildFileWithName(ConfigFileName);
            MainConfig = configFile.Exists
                ? YamlExtensions.NewFromYamlFile<RepoMainConfig>(configFile)
                : new RepoMainConfig();
        }

        public virtual void CreatePackZsyncFile(string fileName) {
            Contract.Requires<ArgumentNullException>(fileName != null);
            Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(fileName));

            _zsyncMake.CreateZsyncFile(GetPackFile(fileName));
        }

        protected virtual IAbsoluteFilePath GetPackFile(string fileName) {
            return PackFolder.GetChildFileWithName(fileName);
        }

        protected virtual IAbsoluteFilePath GetWdFile(string fileName) {
            return Folder.GetChildFileWithName(fileName);
        }

        protected virtual IAbsoluteFilePath GetRsyncFile(string fileName) {
            return RsyncFolder.GetChildFileWithName(fileName);
        }

        protected virtual void LoadConfig(bool fallback = false) {
            TryLoadConfig(fallback);
            PackFolder = String.IsNullOrWhiteSpace(Config.PackPath)
                ? GetRsyncFile(".pack").ToString().ToAbsoluteDirectoryPath()
                : Path.Combine(Config.PackPath, ".pack").ToAbsoluteDirectoryPath();
            WdVersionFile = GetRsyncFile(VersionFileName);
            PackVersionFile = GetPackFile(VersionFileName);

            PackFolder.MakeSurePathExists();
            LoadHosts();
        }

        void TryLoadConfig(bool fallback) {
            try {
                Config = YamlExtensions.NewFromYamlFile<RepoConfig>(ConfigFile);
            } catch (FileNotFoundException) {
                Config = new RepoConfig();
            } catch (Exception e) {
                var msg = String.Format("An error has occurred during processing the config file [{0}]", RepoName);
                this.Logger().FormattedWarnException(e);
                if (!fallback)
                    throw new ConfigException(msg);
                Config = new RepoConfig();
            }
        }

        public virtual void LoadHosts() {
            DownloadManager = new FileDownloadManager(StatusRepo,
                Config.Hosts, MultiThreadingSettings,
                SyncEvilGlobal.FileDownloader, SyncEvilGlobal.GetHostChecker);
        }

        protected string GetNewPackPath(string path, string guid = null) {
            return RepoTools.GetNewPackPath(path, Path.GetFileName(path), guid);
        }

        protected string GetNewPackPathUnlessEqual(string path, string guid = null) {
            var pp = GetNewPackPath(path, guid);
            return pp == Path.GetDirectoryName(Config.PackPath) ? ":match" : pp;
        }

        protected virtual void LoadVersions(bool checkFormat = false, bool allowPFailure = true) {
            TryLoadWDVersion();
            TryLoadPackVersion(allowPFailure);
            CheckRepository(checkFormat);
        }

        void TryLoadPackVersion(bool allowPFailure) {
            try {
                PackVersion = YamlExtensions.NewFromYamlFile<RepoVersion>(PackVersionFile);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                if (!allowPFailure)
                    throw new Exception("Remote repository.yml malformed, cannot process");
                PackVersion = YamlExtensions.NewFromYaml<RepoVersion>(WdVersion.ToYaml());
            }
        }

        void TryLoadWDVersion() {
            try {
                WdVersion = YamlExtensions.NewFromYamlFile<RepoVersion>(WdVersionFile);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                WdVersion = new RepoVersion();
            }
        }

        protected virtual void CheckRepository(bool checkFormat) {
            if (String.IsNullOrWhiteSpace(WdVersion.ArchiveFormat))
                WdVersion.ArchiveFormat = DefaultArchiveFormat;
            if (String.IsNullOrWhiteSpace(PackVersion.ArchiveFormat))
                PackVersion.ArchiveFormat = DefaultArchiveFormat;

            if (checkFormat && WdVersion.ArchiveFormat != PackVersion.ArchiveFormat) {
                throw new Exception(
                    String.Format("Local and remote archive format does not match: {0} vs {1}",
                        WdVersion.ArchiveFormat, PackVersion.ArchiveFormat));
            }

            if (ArchiveFormats.None(x => x == PackVersion.ArchiveFormat))
                throw new ArchiveFormatUnsupported(PackVersion.ArchiveFormat);
        }

        void ConvertFormat(string format) {
            Contract.Requires<ArchiveFormatUnsupported>(ArchiveFormats.Any(x => x == format));
            if (format == ArchiveFormat)
                throw new ArgumentOutOfRangeException("Repository already of same format");

            var previous = ArchiveFormat;

            this.Logger().Info("Converting from {0} to {1}", previous, format);
            if (!WdVersion.WD.Any() && !WdVersion.Pack.Any()) {
                ArchiveFormat = format;
                return;
            }
            WdVersion = new RepoVersion();
            PackVersion = new RepoVersion();

            ArchiveFormat = format;
            SaveVersions();
            CleanupZsyncFiles();

            foreach (
                var f in Directory.EnumerateFiles(PackFolder.ToString(), "*" + previous, SearchOption.AllDirectories))
                Tools.FileUtil.Ops.DeleteWithRetry(f);

            Commit(false, false);
        }

        /*
         * TODO: Verify Pack Folder location; may not be rooted within self or another repository's working folder :S
            var nested = new DirectoryInfo(pp);
            var wd = new DirectoryInfo(Folder);
            if (nested.FullName == wd.FullName) {
                throw new Exception("PackPath cannot be rooted in the working directory");
            }
            while (nested.Parent != null) {
                var parent = nested.Parent;
                if (parent.FullName == )
            }
       */

        public virtual void MovePackPath(string path, string guid = null) {
            var pp = GetNewPackPathUnlessEqual(path, guid);
            if (String.IsNullOrWhiteSpace(pp))
                pp = null;

            var ppPath = pp == null
                ? Path.Combine(Folder.ToString(), RepoFolderName, PackFolderName)
                : Path.Combine(pp, PackFolderName);

            ppPath.MakeSurePathExists();

            Tools.FileUtil.Ops.CopyDirectoryWithRetry(PackFolder, ppPath.ToAbsoluteDirectoryPath());
            Tools.FileUtil.Ops.DeleteWithRetry(PackFolder.ToString());

            Config.PackPath = pp;
            SaveAndReloadConfig();
        }

        void CleanupZsyncFiles() {
            foreach (var f in Directory.EnumerateFiles(PackFolder.ToString(), "*.zsync", SearchOption.AllDirectories))
                Tools.FileUtil.Ops.DeleteWithRetry(f);
        }

        protected virtual void FixMissingPackFiles(string[] files, bool createZsyncFiles = true) {
            this.Logger().Info("Checking for missing pack files...");
            foreach (var f in files)
                FixWhenMissing(createZsyncFiles, f);
        }

        void FixWhenMissing(bool createZsyncFiles, string f) {
            var fil = f.EndsWith(ArchiveFormat, StringComparison.OrdinalIgnoreCase)
                ? f.Substring(0, f.Length - ArchiveFormat.Length)
                : f;

            var file = GetWdFile(fil);
            if (!file.Exists)
                return;

            var gz = fil + ArchiveFormat;
            var dstFile = GetPackFile(gz);
            if (dstFile.Exists)
                return;

            this.Logger().Info("Creating missing pack file: {0}", gz);
            Pack(file, dstFile);

            if (createZsyncFiles && ArchiveFormat != ".7z")
                CreatePackZsyncFile(gz);

            WdVersion.Pack[gz] = RepoTools.TryGetChecksum(dstFile, gz);
        }

        void Pack(IAbsoluteFilePath file, IAbsoluteFilePath destination = null) {
            RepoTools.Pack(file, destination, ArchiveFormat);
        }

        protected virtual void FixMissingZsyncFiles() {
            if (!File.Exists(GetPackFile(VersionFileName) + ".zsync"))
                CreatePackZsyncFile(VersionFileName);

            foreach (var pair in WdVersion.Pack.Where(pair => !File.Exists(GetPackFile(pair.Key) + ".zsync")))
                CreatePackZsyncFile(pair.Key);

            foreach (var zs in Directory.EnumerateFiles(PackFolder.ToString(), "*.zsync", SearchOption.AllDirectories)
                .Where(zs => !File.Exists(Tools.FileUtil.RemoveExtension(zs, ".zsync")))) {
                this.Logger().Info("Removing obsolete pack file: {0}", zs);
                Tools.FileUtil.Ops.DeleteWithRetry(zs);
            }
        }

        protected virtual bool FixGuid(bool pushIfChanged = true) {
            this.Logger().Info("Checking GUID for {0}", Folder);

            if (!String.IsNullOrWhiteSpace(WdVersion.Guid))
                return false;

            this.Logger().Info("Guid missing for {0}, fixing..", Folder);

            WdVersion.Guid = Guid.NewGuid().ToString();
            PackVersion.Guid = WdVersion.Guid;

            SaveVersions();

            if (ArchiveFormat != ".7z")
                CreatePackZsyncFile(VersionFileName);

            if (pushIfChanged)
                Push();

            return true;
        }

        bool HandleOpts(Dictionary<string, object> opts, bool save = true) {
            Contract.Requires<ArgumentNullException>(opts != null);

            var changed = false;

            if (opts.ContainsKey("include")) {
                Config.Include = (string[]) opts["include"];
                changed = true;
            }

            if (opts.ContainsKey("exclude")) {
                Config.Exclude = (string[]) opts["exclude"];
                changed = true;
            }

            if (opts.ContainsKey("allow_full_transfer_fallback"))
                AllowFullTransferFallBack = (bool) opts["allow_full_transfer_fallback"];

            if (opts.ContainsKey("hosts")) {
                Config.Hosts = (Uri[]) opts["hosts"];
                changed = true;
            }

            if (opts.ContainsKey("max_threads"))
                MultiThreadingSettings.MaxThreads = (int) opts["max_threads"];

            if (opts.ContainsKey("keep_compressed_files"))
                KeepCompressedFiles = (bool) opts["keep_compressed_files"];

            if (opts.ContainsKey("status"))
                StatusRepo = (StatusRepo) opts["status"];

            if (opts.ContainsKey("required_version"))
                RequiredVersion = (long?) opts["required_version"];

            if (opts.ContainsKey("required_guid"))
                RequiredGuid = (string) opts["required_guid"];

            if (save && changed)
                SaveAndReloadConfig();
            else
                LoadHosts();

            return changed;
        }

        Spec GetPackSpec(IStatus status) {
            return DownloadManager.GetSpec(status.Item, GetPackFile(status.Item), status);
        }

        protected virtual void RemovedWd(string change) {
            var file = GetWdFile(change);
            if (file.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(file.ToString());
            WdVersion.WD.Remove(change);
        }

        protected virtual void RemovedPack(string change) {
            var file = GetPackFile(change);
            if (file.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(file.ToString());
            var zsFile = file + ".zsync";
            if (File.Exists(zsFile))
                Tools.FileUtil.Ops.DeleteWithRetry(zsFile);
            WdVersion.Pack.Remove(change);
        }

        protected virtual void SaveAndReloadConfig(bool fallback = false) {
            Config.SaveYaml(ConfigFile);
            LoadConfig(fallback);
        }

        protected virtual void SaveVersions() {
            WdVersion.SaveYaml(WdVersionFile);
            PackVersion.SaveYaml(PackVersionFile);
        }

        protected virtual void UpdateRepoInfo() {
            WdVersion.Version = PackVersion.Version;
            WdVersion.WdSize = PackVersion.WdSize;
            WdVersion.PackSize = PackVersion.PackSize;

            if (WdVersion.PackSize == 0)
                WdVersion.PackSize = Tools.FileUtil.GetDirectorySize(PackFolder, "*.zsync");

            if (WdVersion.WdSize == 0)
                WdVersion.WdSize = Tools.FileUtil.GetDirectorySize(Folder, "*.zsync");
        }

        public virtual async Task Update(Dictionary<string, object> opts = null) {
            if (opts == null)
                opts = new Dictionary<string, object>();

            HandleOpts(opts);

            StatusRepo.Action = RepoStatus.Verifying;

            var localOnly = opts.ContainsKey("local_only") && (bool) opts["local_only"];
            if (!localOnly) {
                this.Logger().Info("Required Version: {0}, Required GUID: {1}", RequiredVersion, RequiredGuid);
                await UpdateRepository().ConfigureAwait(false);
            }

            this.Logger().Info(String.Format("Repository version: {0}, GUID: {1}{2}",
                WdVersion.Version, WdVersion.Guid, !String.IsNullOrWhiteSpace(Config.PackPath)
                    ? "Pack Path: " + Config.PackPath
                    : String.Empty));

            LoadSums(true, localOnly);

            StatusRepo.ResetWithoutClearItems(RepoStatus.Updating, StatusRepo.Total);

            var differences = CompareSums(RepositoryFileType.Wd, 1);
            var removed = differences[0];
            var changed = differences[1];

            var changesOnly = !KeepCompressedFiles;
            var changeLock = !localOnly && (changed.Any() || removed.Any()) &&
                             await ProcessPackChanges(changed.Concat(removed), changesOnly).ConfigureAwait(false);

            if (await ProcessWdChanges(differences, changesOnly).ConfigureAwait(false))
                changeLock = true;

            if (CollectTransferLogs) {}

            TryCleanupTmpFiles();

            if (changeLock) {
                this.Logger().Info("Changes detected. Verifying checksums...");
                TryConfirmChecksumValidity(localOnly || changesOnly);
            }

            WdVersion.Guid = PackVersion.Guid;
            if (changeLock || WdVersion.Version != PackVersion.Version) {
                UpdateRepoInfo();
                this.Logger().Info("New repository version: {0}, Pack Size: {1}, WD Size: {2}",
                    WdVersion.Version, WdVersion.PackSize*1024,
                    WdVersion.WdSize*1024);
            }

            WdVersion.SaveYaml(WdVersionFile);
            StatusRepo.Finish();
        }

        void TryConfirmChecksumValidity(bool localOnly) {
            try {
                ConfirmChecksumValidity(localOnly);
            } catch (ChecksumException) {
                WdVersion.SaveYaml(WdVersionFile);
                throw;
            }
        }

        void ConfirmChecksumValidity(bool localOnly) {
            var c = VerifyChecksums(localOnly);
            if (!c.Item1.Any() && !c.Item2.Any())
                return;

            throw new ChecksumException(String.Format("Failed for:\nWd: {0}\nPack: {1}",
                String.Join(", ", c.Item1), String.Join(", ", c.Item2)));
        }

        async Task UpdateRepository() {
            await FetchRepository().ConfigureAwait(false);

            LoadVersions(false, false);
            if (WdVersion.ArchiveFormat != ArchiveFormat)
                await ConvertRepositoryFormat().ConfigureAwait(false);
        }

        async Task ConvertRepositoryFormat() {
            ConvertFormat(ArchiveFormat);
            await FetchRepository().ConfigureAwait(false);
            LoadVersions(false, false);
        }

        void TryCleanupTmpFiles() {
            var di = PackFolder.DirectoryInfo;
            foreach (var fi in TempExtensions.SelectMany(ex => di.EnumerateFiles("*" + ex, SearchOption.AllDirectories))
                )
                TryDeleteFile(fi);
        }

        void TryDeleteFile(FileInfo fi) {
            try {
                Tools.FileUtil.Ops.DeleteFileSystemInfo(fi);
            } catch (Exception e) {
                this.Logger().FormattedDebugException(e);
            }
        }

        protected virtual async Task FetchRepository() {
            var repFile = GetPackFile(VersionFileName);
            var bkpFile = (repFile + ".bkp").ToAbsoluteFilePath();

            HandleBackupRepositoryFile(repFile, bkpFile);

            this.Logger().Info("Processing {0}", VersionFileName);

            await TryFetchRepo(bkpFile, repFile).ConfigureAwait(false);
        }

        IStatus CreateStatus(string file) {
            return new Status.Status(file, StatusRepo) {ZsyncHttpFallback = AllowFullTransferFallBack};
        }

        async Task TryFetchRepo(IAbsoluteFilePath bkpFile, IAbsoluteFilePath repFile) {
            var status = CreateStatus(VersionFileName);
            StartOutput(status);
            var match = false;
            try {
                var spec = GetPackSpec(status);
                while (!match) {
                    await DownloadManager.FetchFileAsync(spec).ConfigureAwait(false);
                    match = ConfirmMatch();
                    if (!match && spec.CurrentHost != null)
                        DownloadManager.HostPicker.MarkBad(spec.CurrentHost);

                    spec.Status.ResetZsyncLoopInfo();
                }
            } finally {
                if (match) {
                    EndOutput(status);
                    if (bkpFile.Exists)
                        Tools.FileUtil.Ops.DeleteWithRetry(bkpFile.ToString());
                } else {
                    FailedOutput(status);
                    if (bkpFile.Exists)
                        Tools.FileUtil.Ops.MoveWithRetry(bkpFile, repFile);
                }
            }
        }

        static void HandleBackupRepositoryFile(IAbsoluteFilePath repFile, IAbsoluteFilePath bkpFile) {
            if (repFile.Exists)
                Tools.FileUtil.Ops.CopyWithRetry(repFile, bkpFile);
            else if (bkpFile.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(bkpFile.ToString());
        }

        bool ConfirmMatch() {
            try {
                var repo = YamlExtensions.NewFromYamlFile<RepoVersion>(GetPackFile(VersionFileName));
                if ((RequiredVersion == null && RequiredGuid == null)
                    || ((RequiredVersion == null || repo.Version == RequiredVersion)
                        && (RequiredGuid == null || repo.Guid == RequiredGuid)))
                    return true;
                this.Logger()
                    .Warn("Failed, did not match expected version or GUID. Found: {0}@{1}", repo.Guid, repo.Version);
                return false;
            } catch (Exception) {
                this.Logger().Warn("Failed, did not match expected version or GUID");
                return false;
            }
        }

        protected virtual void EndOutput(IStatus status) {
            this.Logger().Info("End Processing {0}", status.Item);

            status.EndOutput(GetPackFile(status.Item).ToString());
        }

        protected virtual void FailedOutput(IStatus status) {
            this.Logger().Info("Failed Processing {0}", status.Item);
            status.FailOutput(GetPackFile(status.Item).ToString());
        }

        protected virtual void StartOutput(IStatus status) {
            this.Logger().Info("Start Processing {0}", status.Item);
            status.StartOutput(GetPackFile(status.Item).ToString());
        }

        protected virtual void LoadSums(bool ignoreDeleted = false, bool localOnly = false) {
            this.Logger().Info("Loading checksums...");
            WdVersion.WD = CalcSums(RepositoryFileType.Wd, ignoreDeleted);
            if (!localOnly)
                WdVersion.Pack = CalcSums(RepositoryFileType.Pack, ignoreDeleted);
        }

        protected virtual async Task<bool> ProcessWdChanges(string[][] differences, bool changesOnly) {
            this.Logger().Info("Processing wd changes...");
            var changeLock = false;

            var removed = differences[0];
            var changed = differences[1];
            var added = differences[2];

            if (removed.Any() || changed.Any() || added.Any())
                changeLock = true;

            this.Logger().Info("WD Changes: {0}, New: {1}, Obsolete: {2}", changed.Length, removed.Length, added.Length);

            if (added.Any()) {
                this.Logger().Info("Removing: {0}", String.Join(", ", added));
                foreach (var change in added)
                    RemovedWd(change);
            }

            var list = Tools.FileUtil.OrderBySize(removed
                .Concat(changed)
                .Distinct(), true);

            this.Logger().Info("Processing: {0}", String.Join(", ", list));
            await DownloadManager.Process(list, x => HandleWd(x, changesOnly)).ConfigureAwait(false);
            return changeLock;
        }

        protected virtual async Task<bool> ProcessPackChanges(IEnumerable<string> wdChanges, bool changesOnly = true) {
            this.Logger().Info("Processing pack changes...");
            var changeLock = false;
            StatusRepo.Restart();

            var r = CompareSums(RepositoryFileType.Pack, 1);
            var removed = r[0];
            var changed = r[1];
            var added = r[2];
            var unchanged = r[3];

            if (removed.Any() || changed.Any() || added.Any())
                changeLock = true;

            this.Logger()
                .Info("Pack Changes: {0}, New: {1}, Obsolete: {2}", changed.Length, added.Length, removed.Length);


            if (added.Any()) {
                this.Logger().Info("Removing: {0}", String.Join(", ", added));
                foreach (var change in added)
                    RemovedPack(change);
            }

            var gzWdChanges = wdChanges.Select(x => x + ArchiveFormat).ToString();
            var differences = removed.Concat(changed).Distinct();
            if (changesOnly) {
                differences = differences.Where(x => gzWdChanges.Contains(x));
                removed = removed.Where(x => gzWdChanges.Contains(x)).ToArray();
                // TODO: Must we filter Unchanged too (used for file size calc)
            }

            var list = Tools.FileUtil.OrderBySize(differences, true);
            FixMissingPackFiles(removed, false);
            if (list.Any())
                await DownloadPackChanges(unchanged, list, changesOnly).ConfigureAwait(false);

            return changeLock;
        }

        async Task DownloadPackChanges(IEnumerable<string> unchanged, string[] list, bool changesOnly) {
            var maxThreads = MultiThreadingSettings.MaxThreads > 0
                ? MultiThreadingSettings.MaxThreads
                : 1;

            if (DownloadManager.HostPicker.HostStates.Count > 0 &&
                DownloadManager.HostPicker.HostStates.Count*3 < maxThreads)
                maxThreads = DownloadManager.HostPicker.HostStates.Count*3;

            StatusRepo.ProcessSize(unchanged, PackFolder, PackVersion.PackSize);

            await
                DownloadManager.ProcessAsync(list, item => ProcessPackChange(item, changesOnly), maxThreads)
                    .ConfigureAwait(false);
            if (DownloadManager.HostPicker.ZsyncIncompatHosts.Any()) {
                this.Logger().Warn(
                    "There were several hosts  that did not support the zsync protocol, fallbacks to default downloads occurred for:\n{0}",
                    String.Join(", ", DownloadManager.HostPicker.ZsyncIncompatHosts));
            }
        }

        async Task ProcessPackChange(string item, bool changesOnly) {
            if (StatusRepo.Aborted)
                return;
            var status = CreateStatus(item);
            StartOutput(status);
            await TryProcessPackChange(status, changesOnly).ConfigureAwait(false);
        }

        async Task TryProcessPackChange(IStatus status, bool changesOnly) {
            var done = false;
            try {
                await ChangedPack(status).ConfigureAwait(false);
                try {
                    if (MultiThreadingSettings.PackInclUnpack)
                        ProcessChangedPack(status, changesOnly);
                    done = true;
                } catch (Exception e) {
                    this.Logger().FormattedWarnException(e);
                }
            } finally {
                if (done)
                    EndOutput(status);
                else
                    FailedOutput(status);
            }
        }

        void ProcessChangedPack(IStatus status, bool changesOnly) {
            var it = Tools.FileUtil.RemoveExtension(status.Item, ArchiveFormat);
            var wd = WdVersion.WD.ContainsKey(it) ? WdVersion.WD[it] : null;
            var pack = PackVersion.WD.ContainsKey(it) ? PackVersion.WD[it] : null;
            if (wd != pack)
                ChangedWd(status, it, changesOnly);
        }

        protected virtual Dictionary<string, string> CalcSums(RepositoryFileType type, bool ignoreDeleted = false) {
            var dir = type == RepositoryFileType.Pack ? PackFolder : Folder;
            var dic = new Dictionary<string, string>();

            StatusRepo.ResetWithoutClearItems(RepoStatus.Verifying, StatusRepo.Total);

            var pv = type == RepositoryFileType.Pack ? PackVersion.Pack : PackVersion.WD;

            var done = new List<string>();
            var files = Tools.FileUtil.OrderBySize(GetFiles(dir), true);

            if (MultiThreadingSettings.IsEnabled && MultiThreadingSettings.Checksums &&
                MultiThreadingSettings.MaxThreads2 > 1) {
                if (ignoreDeleted) {
                    foreach (var file in files) {
                        if (!pv.ContainsKey(file)) {
                            dic[file] = "__:DELETE:__";
                            done.Add(file);
                        }
                    }
                }
            } else {
                var i = 0;
                foreach (var file in files) {
                    dic[file] = ignoreDeleted && !pv.ContainsKey(file)
                        ? "__:DELETE:__"
                        : RepoTools.TryGetChecksum(dir.GetChildFileWithName(file), file);

                    i++;
                    StatusRepo.UpdateProgress((i/(double) files.Length)*100.0);
                }
                StatusRepo.Finish();
            }

            return dic;
        }

        IEnumerable<string> GetFiles(IAbsoluteDirectoryPath dir) {
            var dirStr = dir.ToString();
            return Directory.EnumerateFiles(dirStr, "*.*", SearchOption.AllDirectories)
                .Where(x => {
                    var ext = Path.GetExtension(x);
                    return IgnoredExtensions.None(
                        y =>
                            y.Equals(ext, StringComparison.OrdinalIgnoreCase))
                           &&
                           !Config.Exclude.Any(
                               y =>
                                   x.Equals(y,
                                       StringComparison.OrdinalIgnoreCase));
                })
                .Select(
                    x =>
                        x.Substring(x.IndexOf(dirStr) + dirStr.Length + 1)
                            .Replace("\\", "/"))
                .Where(
                    x =>
                        Path.GetFileName(x) == Package.SynqInfoFile || (!x.StartsWith(".") &&
                                                                        !Path.GetFileName(x).StartsWith(".")));
        }

        public virtual void Commit(bool changeVersion = true, bool createZsyncFiles = true) {
            LoadVersions();
            HandleCase();
            LoadSums();

            this.Logger().Info("Repository version: {0}", WdVersion.Version);
            var changeLock = false;

            // Process WD
            var r = CompareSums(RepositoryFileType.Wd, 1);
            var removed = r[0];
            var changed = r[1];
            var added = r[2];

            if (removed.Any() || changed.Any() || added.Any())
                changeLock = true;

            this.Logger().Info("WD Changes: {0}, New: {1}, Obsolete: {2}", changed.Length, added.Length, removed.Length);

            if (removed.Any()) {
                this.Logger().Info("Removing: {0}", String.Join(", ", removed));
                foreach (var change in removed) {
                    RemovedWd(change);
                    RemovedPack(change + ArchiveFormat);
                }
            }

            var list = Tools.FileUtil.OrderBySize(added
                .Concat(changed)
                .Distinct(), true);

            if (list.Any()) {
                this.Logger().Info("Processing: {0}", String.Join(", ", list));
                var i = 0;
                foreach (var change in list) {
                    this.Logger().Info("Packing: {0}", change);
                    PackWdFile(createZsyncFiles, change);

                    i++;
                    StatusRepo.UpdateProgress((i/(double) list.Length)*100.0);
                }
            }

            FixMissingPackFiles(WdVersion.WD.Keys.ToArray());

            foreach (var pair in WdVersion.Pack)
                RemoveIfObsolete(pair);

            r = CompareSums(RepositoryFileType.Pack, 1);
            removed = r[0];
            changed = r[1];
            added = r[2];

            if (removed.Any() || changed.Any() || added.Any())
                changeLock = true;

            this.Logger()
                .Info("Pack Changes: {0}, New: {1}, Obsolete: {2}", changed.Length, added.Length, removed.Length);

            if (removed.Any()) {
                this.Logger().Info("Removing: {0}", String.Join(", ", removed));
                foreach (var change in removed)
                    RemovedPack(change);
            }

            list = added.Concat(changed).Distinct().ToArray();

            this.Logger().Info("Calculating file sizes");
            WdVersion.PackSize = Tools.FileUtil.GetDirectorySize(PackFolder, "*.zsync");
            WdVersion.WdSize = Tools.FileUtil.GetDirectorySize(Folder, "*.zsync") - WdVersion.PackSize;

            PackVersion.Pack = WdVersion.Pack;
            PackVersion.WD = WdVersion.WD;
            PackVersion.PackSize = WdVersion.PackSize;
            PackVersion.WdSize = WdVersion.WdSize;

            var save = false;

            if (WdVersion.Guid == null) {
                WdVersion.Guid = Guid.NewGuid().ToString();
                PackVersion.Guid = WdVersion.Guid;
                save = true;
            }

            if (changeLock) {
                save = true;
                UpdateVersion(changeVersion, list, removed);
            }

            if (save) {
                SaveVersions();
                if (createZsyncFiles && ArchiveFormat != ".7z")
                    CreatePackZsyncFile(VersionFileName);
            }

            if (createZsyncFiles && ArchiveFormat != ".7z") {
                this.Logger().Info("Checking for missing zsync files...");
                FixMissingZsyncFiles();
            }
        }

        void UpdateVersion(bool changeVersion, string[] list, string[] removed) {
            if (changeVersion)
                WdVersion.Version++;
            PackVersion.Version = WdVersion.Version;
            PackVersion.Guid = WdVersion.Guid;
            var changedSize = GetChangedSize(list);

            if (changeVersion)
                this.Logger().Info("New Repository version: {0}", WdVersion.Version);
            this.Logger()
                .Info("New/Changed pack files: {0}, {1}. Removed: {2}", list.Length, changedSize, removed.Length);
        }

        int GetChangedSize(IEnumerable<string> list) {
            return list.Select(GetPackFile)
                .Select(file => file.Exists ? file.FileInfo.Length : 0)
                .Aggregate(0, (current, fileSize) => (int) (current + fileSize));
        }

        void RemoveIfObsolete(KeyValuePair<string, string> pair) {
            var key = Tools.FileUtil.RemoveExtension(pair.Key, ArchiveFormat);
            if (WdVersion.WD.ContainsKey(key))
                return;
            this.Logger().Info("Removing obsolete pack file: {0}", pair.Key);
            RemovedPack(pair.Key);
        }

        void PackWdFile(bool createZsyncFiles, string change) {
            var file = GetWdFile(change);
            var gz = change + ArchiveFormat;
            var dstFile = GetPackFile(gz);
            Pack(file, dstFile);

            if (createZsyncFiles && ArchiveFormat != ".7z")
                CreatePackZsyncFile(gz);

            WdVersion.WD[change] = RepoTools.TryGetChecksum(file, change);
            WdVersion.Pack[gz] = RepoTools.TryGetChecksum(dstFile, gz);
        }

        protected virtual void ChangedWd(IStatus status, string change, bool changesOnly) {
            status.Action = RepoStatus.Unpacking;
            var dstFile = GetWdFile(change);
            var changePack = change + ArchiveFormat;
            var srcFile = GetPackFile(changePack);

            if (!srcFile.Exists) {
                this.Logger().Warn("Can't find archive: {0}", changePack);
                return;
            }

            RepoTools.UnpackFile(srcFile, dstFile, status);

            if (changesOnly)
                Tools.FileUtil.Ops.DeleteWithRetry(srcFile.ToString());

            status.Action = RepoStatus.Summing;
            var sum = RepoTools.TryGetChecksum(dstFile, change);
            lock (WdVersion.WD)
                WdVersion.WD[change] = sum;
            status.Action = RepoStatus.Finished;
        }

        protected virtual void HandleWd(string item, bool changesOnly, bool silence = false) {
            var status = CreateStatus(item);
            StartOutput(status);
            status.Progress = 33;
            TryHandleWd(status, changesOnly);
        }

        void TryHandleWd(IStatus status, bool changesOnly) {
            try {
                ChangedWd(status, status.Item, changesOnly);
                var wd = WdVersion.WD.ContainsKey(status.Item) ? WdVersion.WD[status.Item] : "-1";
                var pack = PackVersion.WD.ContainsKey(status.Item) ? PackVersion.WD[status.Item] : "-1";
                if (wd != pack) {
                    throw new ChecksumException(String.Format("{0}. Got: {1}, Expected: {2}",
                        status.Item, wd, pack));
                }
                EndOutput(status);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                FailedOutput(status);
            }
        }

        protected virtual async Task ChangedPack(IStatus status) {
            var completed = false;

            while (!completed)
                completed = await TryChangedPack(status).ConfigureAwait(false);
        }

        async Task<bool> TryChangedPack(IStatus status) {
            try {
                await DownloadManager.FetchFileAsync(GetPackSpec(status)).ConfigureAwait(false);
                SumChangedPack(status);
                return true;
            } catch (HostListExhausted) {
                StatusRepo.Abort();
                throw;
            } catch (ChecksumException e) {
                this.Logger().FormattedWarnException(e);
                return false;
            }
        }

        void SumChangedPack(IStatus status) {
            status.Action = RepoStatus.Summing;
            var packFile = GetPackFile(status.Item);
            var sum = RepoTools.TryGetChecksum(packFile, status.Item);
            lock (WdVersion.Pack) {
                WdVersion.Pack[status.Item] = sum;
            }

            var wd = WdVersion.Pack.ContainsKey(status.Item) ? WdVersion.Pack[status.Item] : "-1";
            var pack = PackVersion.Pack.ContainsKey(status.Item) ? PackVersion.Pack[status.Item] : "-1";
            if (wd != pack) {
                throw new ChecksumException(String.Format("{0}. Got: {1}, Expected: {2}",
                    status.Item, wd, pack));
            }
        }

        protected virtual void HandleCase() {
            Tools.FileUtil.HandleDowncaseFolder(Folder, Config.Exclude);
        }

        protected virtual string[][] CompareSums(RepositoryFileType type, int i) {
            var wd = type == RepositoryFileType.Wd ? WdVersion.WD : WdVersion.Pack;
            var pack = type == RepositoryFileType.Wd ? PackVersion.WD : PackVersion.Pack;

            var wdDc = RepoTools.DowncaseDictionary(wd);
            var packDc = RepoTools.DowncaseDictionary(pack);

            var removed = new List<string>();
            var changed = new List<string>();
            var added = new List<string>();
            var unchanged = new List<string>();

            foreach (var pair in pack) {
                if (!IncludeMatch(pair.Key, type))
                    continue;
                var dc = pair.Key.ToLower();
                if (!wdDc.ContainsKey(dc)) {
                    removed.Add(pair.Key);
                    continue;
                }

                if (!wd.ContainsKey(pair.Key)) {
                    changed.Add(pair.Key);
                    continue;
                }

                if (!wdDc.ContainsKey(dc) || wdDc[dc] != pair.Value) {
                    changed.Add(pair.Key);
                    continue;
                }

                unchanged.Add(pair.Key);
            }

            foreach (var pair in wd) {
                if (!IncludeMatch(pair.Key, type))
                    continue;

                var dc = pair.Key.ToLower();
                if (!packDc.ContainsKey(dc)) {
                    added.Add(pair.Key);
                    continue;
                }

                if (!pack.ContainsKey(pair.Key)) {
                    added.Add(pair.Key);
                    changed.Add(dc);
                }
            }

            return new[] {
                removed.Distinct().ToArray(), changed.Distinct().ToArray(), added.Distinct().ToArray(),
                unchanged.Distinct().ToArray()
            };
        }

        protected virtual bool IncludeMatch(string key, RepositoryFileType type = RepositoryFileType.Wd) {
            if (Config.Include == null || !Config.Include.Any())
                return true;

            return true;
        }

        protected virtual Tuple<IList<string>, IList<string>> VerifyChecksums(bool localOnly = false) {
            var values = Tuple.Create((IList<string>) new List<string>(), (IList<string>) new List<string>());

            var r = CompareSums(RepositoryFileType.Wd, 1);
            var removed = r[0];
            var changed = r[1];
            var added = r[2];

            var failed = removed.Concat(changed).Concat(added).Distinct().ToArray();
            if (failed.Any()) {
                this.Logger().Info("Checksum errors (WD): {0}", String.Join(", ", failed));
                foreach (var f in failed)
                    values.Item1.Add(f);
            }

            if (localOnly)
                return values;

            r = CompareSums(RepositoryFileType.Pack, 1);
            removed = r[0];
            changed = r[1];
            added = r[2];

            failed = removed.Concat(changed).Concat(added).Distinct().ToArray();
            if (failed.Any()) {
                this.Logger().Info("Checksum errors (Pack): {0}", String.Join(", ", failed));
                foreach (var f in failed)
                    values.Item2.Add(f);
            }

            return values;
        }

        public virtual void Push(string key = null, Uri host = null) {
            if (key == null)
                key = MainConfig.Key;

            if (host == null)
                host = DownloadManager.HostPicker.PickHost();

            PushFolder(PackFolder, host, new Dictionary<string, string> {{"key", key}});
        }

        protected virtual void PushFolder(IAbsoluteDirectoryPath packFolder, Uri host,
            Dictionary<string, string> dictionary) {
            throw new NotImplementedException();
        }

        public virtual void CommitAndPush(string key = null, Uri host = null) {
            Commit();
            Push(key, host);
        }
    }
}