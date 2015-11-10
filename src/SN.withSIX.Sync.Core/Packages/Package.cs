// <copyright company="SIX Networks GmbH" file="Package.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using SN.withSIX.Api.Models.Publishing;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Services;
using SN.withSIX.Sync.Core.Transfer;
using Repository = SN.withSIX.Sync.Core.Repositories.Repository;

namespace SN.withSIX.Sync.Core.Packages
{
    public class SynqSpec
    {
        public SynqSpec() {
            Processing = new Processing();
        }

        public Processing Processing { get; set; }
    }

    public class Processing
    {
        public bool? Sign { get; set; }
    }

    public class Package : IComparePK<Package>, IEnableLogging
    {
        const string SynqInfoJoiner = "\r\n";
        public const string SynqInfoFile = ".synqinfo";
        public const string SynqSpecFile = ".synqspec";
        const string Rx = "[\\s\\t]*=[\\s\\t]*\"(.*)\"[\\s\\t]*;?";
        public static readonly PackageFactory Factory = new PackageFactory();
        static readonly string[] synqInfoSeparator = {SynqInfoJoiner, "\n"};
        static readonly Regex rxCppName = new Regex("name" + Rx,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxCppDescription = new Regex("description" + Rx,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxCppAuthor = new Regex("author" + Rx,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Package(IAbsoluteDirectoryPath workingDirectory, string packageName, Repository repository) {
            Contract.Requires<ArgumentNullException>(workingDirectory != null);
            Contract.Requires<ArgumentNullException>(repository != null);

            WorkingPath = workingDirectory;
            Repository = repository;
            ConfirmPathValidity();
            MetaData = Load(Repository.GetMetaDataPath(packageName));
            ConfirmPackageValidity(packageName);
            StatusRepo = new StatusRepo();
        }

        public Package(IAbsoluteDirectoryPath workingDirectory, PackageMetaData metaData, Repository repository) {
            Contract.Requires<ArgumentNullException>(workingDirectory != null);
            Contract.Requires<ArgumentNullException>(repository != null);

            WorkingPath = workingDirectory;
            Repository = repository;
            ConfirmPathValidity();
            MetaData = metaData;
            StatusRepo = new StatusRepo();
        }

        public StatusRepo StatusRepo { get; set; }
        Repository Repository { get; }
        public IAbsoluteDirectoryPath WorkingPath { get; private set; }
        public PackageMetaData MetaData { get; private set; }

        public bool ComparePK(object other) {
            var o = other as Package;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(Package other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        public static async Task<SynqSpec> ReadSynqSpecAsync(IAbsoluteDirectoryPath path) {
            var specFile = path.GetChildFileWithName(SynqSpecFile);
            return specFile.Exists
                ? await Tools.Serialization.Json.LoadJsonFromFileAsync<SynqSpec>(specFile).ConfigureAwait(false)
                : new SynqSpec();
        }

        public static SynqSpec ReadSynqSpec(IAbsoluteDirectoryPath path) {
            var specFile = path.GetChildFileWithName(SynqSpecFile);
            return specFile.Exists ? Tools.Serialization.Json.LoadJsonFromFile<SynqSpec>(specFile) : new SynqSpec();
        }

        public void SetWorkingPath(string path) {
            WorkingPath = Legacy.SixSync.Repository.RepoTools.GetRootedPath(path);
        }

        public static SpecificVersion ReadSynqInfoFile(IAbsoluteDirectoryPath packageDir) {
            if (!packageDir.Exists)
                return null;
            var file = packageDir.GetChildFileWithName(SynqInfoFile);
            return !file.Exists ? null : new SpecificVersion(File.ReadAllText(file.ToString()));
        }

        void ConfirmPackageValidity(string packageName) {
            var fn = MetaData.GetFullName();
            if (fn != packageName)
                throw new Exception("Invalid package metadata: {0} vs {1}".FormatWith(fn, packageName));
        }

        void ConfirmPathValidity() {
            Repository.Factory.HandleRepositoryRequirements(Repository.RootPath);
            Repository.Factory.HandlePackageRequirements(WorkingPath, Repository);
        }

        public string GetFullName() {
            return MetaData.GetFullName();
        }

        public static IAbsoluteDirectoryPath GetRepoPath(string repositoryDirectory,
            IAbsoluteDirectoryPath workingDirectory) {
            return string.IsNullOrWhiteSpace(repositoryDirectory)
                ? workingDirectory.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory)
                : repositoryDirectory.ToAbsoluteDirectoryPath();
        }

        public bool Commit(string desiredVersion, bool force = false, bool downCase = true) {
            var metaData = CreateUpdatedMetaData(desiredVersion, downCase);

            if (!force && MetaData.Compare(metaData))
                return false;

            MetaData = metaData;
            Save();
            return true;
        }

        public Task<Guid> Register(IPublishingApi api, string registerInfo, string registerKey) {
            return api.Publish(BuildPublishModel(registerInfo), registerKey);
        }

        public Task<Guid> DeRegister(IPublishingApi api, string registerInfo, string registerKey) {
            return api.Publish(BuildPublishModel(registerInfo), registerKey);
        }

        PublishModModel BuildPublishModel(string registerInfo) {
            var yml = WorkingPath.GetChildDirectoryWithName(".rsync\\.pack")
                .GetChildFileWithName(".repository.yml");

            var cppInfo = GetCppInfo();
            return new PublishModModel {
                PackageName = MetaData.Name,
                Revision = yml.Exists
                    ? (int) YamlExtensions.NewFromYamlFile< RepoVersion>(yml).Version
                    : 0,
                Version = MetaData.GetVersionInfo(),
                Size = MetaData.SizePacked,
                SizeWd = MetaData.Size,
                Readme = GetReadme(),
                Changelog = GetChangelog(),
                License = GetLicense(),
                CppName = cppInfo.Item1,
                Description = cppInfo.Item2.TruncateNullSafe(500),
                Author = cppInfo.Item3,
                RegisterInfo = registerInfo
            };
        }

        Tuple<string, string, string> GetCppInfo() {
            var cppFile = WorkingPath.GetChildFileWithName("mod.cpp");
            if (!cppFile.Exists)
                return new Tuple<string, string, string>(null, null, null);
            var fileContent = File.ReadAllText(cppFile.ToString());

            var match = rxCppName.Match(fileContent);
            var name = !match.Success ? null : match.Groups[1].Value;

            match = rxCppDescription.Match(fileContent);
            var description = !match.Success ? null : match.Groups[1].Value;

            match = rxCppAuthor.Match(fileContent);
            var author = !match.Success ? null : match.Groups[1].Value;

            return Tuple.Create(name, description, author);
        }

        string GetChangelog() {
            var text = "";
            foreach (var cl in WorkingPath.DirectoryInfo.EnumerateFiles("*changelog*.txt", SearchOption.AllDirectories)) {
                text += cl.Name + "\n\n";
                text += File.ReadAllText(cl.FullName);
            }
            return text.Length == 0 ? null : text;
        }

        string GetLicense() {
            var text = "";
            foreach (var cl in WorkingPath.DirectoryInfo.EnumerateFiles("*license*.txt", SearchOption.AllDirectories)) {
                text += cl.Name + "\n\n";
                text += File.ReadAllText(cl.FullName);
            }
            return text.Length == 0 ? null : text;
        }

        string GetReadme() {
            var text = "";
            foreach (var txt in WorkingPath.DirectoryInfo.EnumerateFiles("*.txt", SearchOption.AllDirectories)
                .Where(x => !x.Name.ContainsIgnoreCase("changelog") && !x.Name.ContainsIgnoreCase("license"))) {
                text += txt.Name + "\n\n";
                text += File.ReadAllText(txt.FullName);
            }
            return text.Length == 0 ? null : text;
        }

        PackageMetaData CreateUpdatedMetaData(string desiredVersion, bool downCase) {
            return UpdateMetaData(downCase, MetaData.SpawnNewVersion(desiredVersion));
        }

        PackageMetaData UpdateMetaData(bool downCase, PackageMetaData metaData) {
            metaData.Files = Repository.Commit(WorkingPath, downCase);
            var paths = new List<string>();
            GetMetaDataFilesOrderedBySize(metaData).ForEach(x => UpdateFileMetaData(metaData, x, paths));

            if (string.IsNullOrWhiteSpace(metaData.ContentType))
                return metaData;
            Repository.SetContentType(metaData.Name, metaData.ContentType);
            Repository.Save();
            // TODO: This should be done in one go, now its done twice once at Repository.Commit and once here :S

            return metaData;
        }

        void UpdateFileMetaData(PackageMetaData metaData, FileObjectMapping x, ICollection<string> paths) {
            metaData.Size += new FileInfo(Path.Combine(WorkingPath.ToString(), x.FilePath)).Length;
            var path = Repository.GetObjectPath(x.Checksum);
            if (paths.Contains(path.ToString()))
                return;
            paths.Add(path.ToString());
            metaData.SizePacked += new FileInfo(path.ToString()).Length;
        }

        public bool Commit(bool force = false, bool downCase = true) {
            return
                Commit(
                    new Dependency(MetaData.Name, MetaData.Version.AutoIncrement().ToString(), MetaData.Branch)
                        .VersionData,
                    force, downCase);
        }

        public static PackageMetaData TryLoad(IAbsoluteFilePath metaDataPath) {
            try {
                return Repository.Load<PackageMetaDataDto, PackageMetaData>(metaDataPath);
            } catch (Exception) {
                return null;
            }
        }

        public static PackageMetaData Load(IAbsoluteFilePath metaDataPath) {
            return Repository.Load<PackageMetaDataDto, PackageMetaData>(metaDataPath);
        }

        Task SaveAsync() {
            return Repository.SavePackageAsync(this);
        }

        [Obsolete("Use SaveAsync")]
        void Save() {
            Repository.SavePackage(this);
        }

        public async Task<string[]> Update(IEnumerable<Uri> remotes, StatusRepo repo, bool skipWhenFileMatches = true) {
            var objects = GetNeededObjects(skipWhenFileMatches);
            var relObjects = objects.OrderByDescending(x => Tools.FileUtil.SizePrediction(x.FilePath))
                .Select(
                    x => new FileFetchInfo(Repository.GetObjectSubPath(x), x.FilePath))
                .ToArray();
            StatusRepo.ProcessSize(GetExistingObjects(objects), Repository.ObjectsPath, MetaData.SizePacked);

            // TODO: Abort support!
            // TODO: Progress fix??!
            await DownloadObjects(remotes, repo, relObjects, Repository.ObjectsPath).ConfigureAwait(false);

            Repository.ReAddObject(objects.Select(x => x.Checksum).ToArray());

            return relObjects.Select(x => x.FilePath).ToArray();
        }

        public Task CheckoutAsync() {
            return Task.Factory.StartNew(Checkout, TaskCreationOptions.LongRunning);
        }

        public Task CheckoutWithoutRemovalAsync() {
            return Task.Factory.StartNew(CheckoutWithoutRemoval, TaskCreationOptions.LongRunning);
        }

        public void Checkout() {
            WorkingPath.MakeSurePathExists();
            ProcessCheckout();
            WriteTag();
        }

        public void CheckoutWithoutRemoval() {
            OnCheckoutWithoutRemoval();
            UpdateTag();
        }

        void UpdateTag() {
            var tagFile = GetSynqInfoPath();

            var existing = GetInstalledPackages(tagFile);
            Tools.FileUtil.Ops.CreateText(tagFile,
                string.Join(SynqInfoJoiner, existing.Where(x => x.Name != MetaData.Name)
                    .Concat(new[] {new SpecificVersion(MetaData.GetFullName())})
                    .OrderBy(x => x.Name)));
        }

        public static IEnumerable<SpecificVersion> GetInstalledPackages(IAbsoluteDirectoryPath path) {
            return GetInstalledPackages(path.GetChildFileWithName(SynqInfoFile));
        }


        public static IEnumerable<SpecificVersion> GetInstalledPackages(IAbsoluteFilePath tagFile) {
            return tagFile.Exists
                ? ReadTagFile(tagFile).Select(x => new SpecificVersion(x))
                : Enumerable.Empty<SpecificVersion>();
        }

        static IEnumerable<string> ReadTagFile(IAbsoluteFilePath tagFile) {
            return Tools.FileUtil.Ops.ReadTextFile(tagFile).Split(synqInfoSeparator, StringSplitOptions.None);
        }

        void OnCheckoutWithoutRemoval() {
            WorkingPath.MakeSurePathExists();
            ProcessCheckout(false);
        }

        public List<FileObjectMapping> GetNeededObjects(bool skipWhenFileMatches = true) {
            var objects = GetMetaDataFilesOrderedBySize().ToList();

            var validObjects = new List<FileObjectMapping>();
            foreach (var o in objects)
                ProcessObject(skipWhenFileMatches, o, validObjects);

            objects.RemoveRange(validObjects);

            var missingObjects = GetMissingObjectMapping(objects);
            var resolvedObjects = new List<FileObjectMapping>();
            if (missingObjects.Any())
                HandleMissingObjects(missingObjects, resolvedObjects);

            Repository.Log("Local object matches {0}, left: {1}", MetaData.Files.Count - objects.Count, objects.Count);

            return objects;
        }

        IEnumerable<string> GetExistingObjects(IEnumerable<FileObjectMapping> objects) {
            return
                GetMetaDataFilesOrderedBySize()
                    .Select(x => x.Checksum)
                    .Except(objects.Select(x => x.Checksum))
                    .Select(x => Repository.GetObjectSubPath(x));
        }

        public static Task DownloadObjects(IEnumerable<Uri> remotes, StatusRepo sr,
            IEnumerable<FileFetchInfo> files, IAbsoluteDirectoryPath destination) {
            return
                SyncEvilGlobal.DownloadHelper.DownloadFilesAsync(GetObjectRemotes(remotes).ToArray(), sr,
                    GetTransferDictionary(sr, files),
                    destination);
        }

        static IEnumerable<Uri> GetObjectRemotes(IEnumerable<Uri> remotes) {
            return remotes.Select(x => Tools.Transfer.JoinUri(x, "objects"));
        }

        static IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> GetTransferDictionary(
            StatusRepo sr,
            IEnumerable<FileFetchInfo> files) {
            return files.OrderByDescending(x => Tools.FileUtil.SizePrediction(x.FilePath))
                .ToDictionary(x => new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(x.FilePath, null),
                    x => (ITransferStatus) new Status(x.DisplayName, sr) {RealObject = x.FilePath});
        }

        void HandleMissingObjects(IDictionary<FileObjectMapping, string> missingObjects,
            ICollection<FileObjectMapping> resolvedObjects) {
            var currentPackage = MetaData.GetFullName();
            var packages = Repository.GetPackagesList()
                .Where(x => !x.Equals(currentPackage))
                .OrderByDescending(x => x.StartsWith(MetaData.Name)).ToArray();
            if (packages.Any())
                ProcessMissingObjects(missingObjects, packages);

            var resolvableObjects = missingObjects.Where(x => x.Value != null).ToArray();
            StatusRepo.Reset(RepoStatus.Copying, resolvableObjects.Count());
            foreach (var o in resolvableObjects)
                ProcessResolvableObject(o, missingObjects);

            StatusRepo.Reset(RepoStatus.Packing, missingObjects.Count);

            foreach (var o in missingObjects.Select(x => x.Key))
                ProcessMissingObject(o, resolvedObjects);

            foreach (var o in resolvedObjects)
                missingObjects.Remove(o);

            Repository.Log(
                "\nFound {0} missing objects, resolved {1} candidates from other packages and {2} from uncompressed files",
                missingObjects.Count + resolvedObjects.Count + resolvableObjects.Length, resolvableObjects.Length,
                resolvedObjects.Count);
        }

        IDictionary<FileObjectMapping, string> GetMissingObjectMapping(IEnumerable<FileObjectMapping> objects) {
            return (from o in objects
                let f = Repository.GetObjectPath(o)
                where !f.Exists
                select o)
                .ToDictionary<FileObjectMapping, FileObjectMapping, string>(o => o, o => null);
        }

        void ProcessMissingObjects(IDictionary<FileObjectMapping, string> missingObjects, string[] packages) {
            var cache = new Dictionary<string, PackageMetaData>();
            foreach (var missing in missingObjects.ToDictionary(x => x.Key, x => x.Value)) {
                foreach (var package in packages)
                    ProcessMissingObjects(cache, package, missing, missingObjects);
            }
        }

        void ProcessMissingObjects(IDictionary<string, PackageMetaData> cache, string package,
            KeyValuePair<FileObjectMapping, string> missing, IDictionary<FileObjectMapping, string> missingObjects) {
            var metadata = RetrieveMetaData(cache, package);
            if (metadata == null || !metadata.Files.ContainsKey(missing.Key.FilePath))
                return;

            var match = metadata.Files[missing.Key.FilePath];
            var oPath = Repository.GetObjectPath(match);
            if (oPath.Exists)
                missingObjects[missing.Key] = match;
        }

        PackageMetaData RetrieveMetaData(IDictionary<string, PackageMetaData> cache, string package) {
            if (cache.ContainsKey(package))
                return cache[package];

            var packageMetadataPath = GetPackageMetadataPath(package);
            var metadata = !packageMetadataPath.Exists
                ? null
                : TryLoad(packageMetadataPath);
            cache.Add(package, metadata);
            return metadata;
        }

        IAbsoluteFilePath GetPackageMetadataPath(string package) {
            return Repository.PackagesPath.GetChildFileWithName(package + Repository.PackageFormat);
        }

        void ProcessResolvableObject(KeyValuePair<FileObjectMapping, string> o,
            IDictionary<FileObjectMapping, string> missingObjects) {
            var fileObjectMapping = o.Key;
            var status = new Status(fileObjectMapping.FilePath, StatusRepo) {
                Action = RepoStatus.Copying,
                RealObject = GetObjectPathFromChecksum(fileObjectMapping)
            };
            this.Logger()
                .Info("Found local previous version match for {0}. Copying {1} to {2}", fileObjectMapping.FilePath,
                    o.Value, fileObjectMapping.Checksum);
            Repository.CopyObject(o.Value, fileObjectMapping.Checksum);
            missingObjects.Remove(fileObjectMapping);
            status.EndOutput();
        }

        static string GetObjectPathFromChecksum(FileObjectMapping fileObjectMapping) {
            return "objects/" + fileObjectMapping.Checksum.Substring(0, 2) + "/" +
                   fileObjectMapping.Checksum.Substring(2);
        }

        void ProcessMissingObject(FileObjectMapping o, ICollection<FileObjectMapping> resolvedObjects) {
            var f = WorkingPath.GetChildFileWithName(o.FilePath);
            if (!f.Exists)
                return;
            var status = new Status(o.FilePath, StatusRepo) {
                Action = RepoStatus.Packing,
                RealObject = GetObjectPathFromChecksum(o)
            };
            this.Logger().Info("Found local previous version file for {0}. Compressing to {1}", o.FilePath,
                o.Checksum);
            Repository.CompressObject(f, o.Checksum);

            resolvedObjects.Add(o);
            status.EndOutput();
        }

        void ProcessObject(bool skipWhenFileMatches, FileObjectMapping o, ICollection<FileObjectMapping> validObjects) {
            if (skipWhenFileMatches) {
                // We can also skip objects that already match in the working directory so that we don't waste time on compressing or copying objects needlessly
                // this however could create more bandwidth usage in case the user in the future deletes working files, and tries to get the version again
                // in that case the objects will need to be redownloaded, or at least patched up from other possible available objects.
                var path = WorkingPath.GetChildFileWithName(o.FilePath);
                if (path.Exists
                    && Repository.GetChecksum(path).Equals(o.Checksum))
                    validObjects.Add(o);
            }
            var ob = Repository.GetObject(o.Checksum);
            if (ob == null)
                return;
            var oPath = Repository.GetObjectPath(o.Checksum);
            if (oPath.Exists
                && Repository.GetChecksum(oPath).Equals(ob.ChecksumPack))
                validObjects.Add(o);
        }

        void ProcessCheckout(bool withRemoval = true) {
            HandleDownCase();
            var mappings = GetMetaDataFilesOrderedBySize();
            var changeAg = GetInitialChangeList(withRemoval, mappings);

            TryCrippleSixSyncIfExists();

            if (withRemoval)
                HandleChangesWithRemove(mappings, changeAg);
            else
                HandleChanges(mappings, changeAg);

            ConfirmChanges(withRemoval, mappings);
        }

        void HandleDownCase() {
            Tools.FileUtil.HandleDowncaseFolder(WorkingPath);
        }

        ChangeList GetInitialChangeList(bool withRemoval, IOrderedEnumerable<FileObjectMapping> mappings) {
            var workingPathFiles = GetWorkingPathFiles(withRemoval, mappings);
            var changeAg = new ChangeList(workingPathFiles, mappings, this);

            PrintChangeOverview(workingPathFiles, mappings);
            Console.WriteLine();
            PrintDetailedChanges(changeAg, withRemoval);

            return changeAg;
        }

        void ConfirmChanges(bool withRemoval, IOrderedEnumerable<FileObjectMapping> mappings) {
            var afterChangeAg = new ChangeList(GetWorkingPathFiles(withRemoval, mappings), mappings, this);
            if (!afterChangeAg.HasChanges(withRemoval))
                return;
            PrintDetailedChanges(afterChangeAg, withRemoval);
            throw new ChecksumException("See log for details");
        }

        void TryCrippleSixSyncIfExists() {
            try {
                CrippleSixSyncIfExists();
            } catch (IOException e) {
                this.Logger().FormattedWarnException(e, "failure to clear legacy .rsync/.repository.yml");
            }
        }

        void CrippleSixSyncIfExists() {
            Tools.FileUtil.Ops.DeleteIfExists(Path.Combine(WorkingPath.ToString(),
                Legacy.SixSync.Repository.RepoFolderName,
                ".repository.yml"));
        }

        public IOrderedEnumerable<FileObjectMapping> GetMetaDataFilesOrderedBySize() {
            return GetMetaDataFilesOrderedBySize(MetaData);
        }

        static IOrderedEnumerable<FileObjectMapping> GetMetaDataFilesOrderedBySize(PackageMetaData metaData) {
            return metaData.GetFiles()
                .OrderByDescending(x => Tools.FileUtil.SizePrediction(x.FilePath));
        }

        IAbsoluteFilePath[] GetWorkingPathFiles(bool withRemoval, IOrderedEnumerable<FileObjectMapping> mappings) {
            if (!withRemoval) {
                return
                    mappings.Select(x => WorkingPath.GetChildFileWithName(x.FilePath))
                        .Where(x => x.Exists).ToArray();
            }

            var files = Repository.GetFiles(WorkingPath);
            return files
                .OrderByDescending(x => Tools.FileUtil.SizePrediction(x.FileName)).ToArray();
        }

        void HandleChanges(IEnumerable<FileObjectMapping> mappings, ChangeList changeAg) {
            HandleCopy(changeAg.Copy, changeAg.StatusDic);
            HandleChangedCase(changeAg.ChangedCase, changeAg.StatusDic);
            HandleModify(changeAg.GetModified(), changeAg.StatusDic, mappings);
        }

        void HandleChangesWithRemove(IEnumerable<FileObjectMapping> mappings, ChangeList changeAg) {
            HandleCopy(changeAg.Copy, changeAg.StatusDic);
            HandleRemove(changeAg.Remove, changeAg.StatusDic);
            HandleChangedCase(changeAg.ChangedCase, changeAg.StatusDic);
            HandleModify(changeAg.GetModified(), changeAg.StatusDic, mappings);
        }

        void HandleChangedCase(ICollection<KeyValuePair<string, string>> changedCase,
            IDictionary<string, Status> statusDic) {
            StatusRepo.ResetWithoutClearItems(RepoStatus.Renaming, changedCase.Count);
            changedCase.ForEach(x => RenameExistingObject(statusDic, x.Value, x.Key));
        }

        void RenameExistingObject(IDictionary<string, Status> statusDic, string destName, string srcName) {
            var status = statusDic[destName];
            status.Progress = 0;
            status.Action = RepoStatus.Renaming;
            RenameFilePath(WorkingPath.GetChildFileWithName(srcName), WorkingPath.GetChildFileWithName(destName));
            status.EndOutput();
        }

        static void RenameFilePath(IAbsoluteFilePath srcFile, IAbsoluteFilePath destFile) {
            destFile.ParentDirectoryPath.MakeSurePathExists();
            Tools.FileUtil.Ops.Move(srcFile, destFile);
            RenameDirectoryPath(srcFile.ParentDirectoryPath, destFile.ParentDirectoryPath);
        }

        // TODO: It is kinda bad to go deeper than the subfolders/files of the WorkingPath
        // although currently it is no problem; the WorkingPath root is the same for both source and dest
        // it seems somewhat dangerous and we should rather not go into rename operations deeper into the workingpath
        static void RenameDirectoryPath(IAbsoluteDirectoryPath srcDir, IAbsoluteDirectoryPath dstDir) {
            var srcParts = srcDir.ToString().Split('/', '\\');
            var destParts = dstDir.ToString().Split('/', '\\');
            foreach (var paths in Enumerable.Range(0, destParts.Length).Reverse()
                .Where(i => srcParts.Length > i)
                .Select(
                    i =>
                        new {
                            src = string.Join("\\", srcParts.Take(i + 1)),
                            dst = string.Join("\\", destParts.Take(i + 1))
                        })
                .Where(
                    paths =>
                        paths.src != paths.dst &&
                        paths.src.Equals(paths.dst, StringComparison.InvariantCultureIgnoreCase))) {
                Tools.FileUtil.Ops.MoveDirectory(paths.src.ToAbsoluteDirectoryPath(),
                    paths.dst.ToAbsoluteDirectoryPath());
            }
        }

        void PrintChangeOverview(IEnumerable<IAbsoluteFilePath> files, IEnumerable<FileObjectMapping> mappings) {
            var overview = new StringBuilder();
            var full = new StringBuilder();
            BuildShortLogInfo("Current files", files.Select(x => x.FileName), overview, full);
            BuildShortLogInfo("Needed files", mappings.Select(x => x.FilePath), overview, full);

            this.Logger().Info(full.ToString());
            Repository.Log(overview.ToString());
        }

        void PrintDetailedChanges(ChangeList changeAg, bool withRemoval) {
            var overview = new StringBuilder();
            var full = new StringBuilder();
            BuildLogInfos(changeAg.Equal, overview, full, changeAg.Copy, changeAg.Update,
                withRemoval ? changeAg.Remove : new List<string>(), changeAg.New, changeAg.ChangedCase);
            this.Logger().Info(full.ToString());
            Repository.Log(overview.ToString());
        }

        static void BuildLogInfos(IEnumerable<string> equal, StringBuilder overview, StringBuilder full,
            IEnumerable<KeyValuePair<string, List<string>>> copy, IReadOnlyCollection<string> update,
            IReadOnlyCollection<string> remove,
            IReadOnlyCollection<string> @new, IEnumerable<KeyValuePair<string, string>> caseChange) {
            BuildShortLogInfo("Equal", equal, overview, full);
            BuildLogInfo("CaseChange", caseChange.Select(x => x.Key + ": " + x.Value).ToList(), overview, full);
            BuildLogInfo("Copy", copy.Select(x => x.Key + ": " + string.Join(",", x.Value)).ToList(), overview, full);
            BuildLogInfo("Update", update, overview, full);
            BuildLogInfo("Remove", remove, overview, full);
            BuildLogInfo("New", @new, overview, full);
        }

        static void BuildShortLogInfo(string type, IEnumerable<string> changes, StringBuilder overview,
            StringBuilder full) {
            var info = ShortStatusInfo(type, changes);
            overview.AppendLine(info);
            full.AppendLine(info);
        }

        void HandleRemove(List<string> remove, IDictionary<string, Status> statusDic) {
            StatusRepo.ResetWithoutClearItems(RepoStatus.Removing, remove.Count());

            remove.ForEach(x => ProcessRemoved(statusDic, x));

            foreach (var d in GetEmptyDirectories(remove)) {
                Tools.FileUtil.Ops.DeleteDirectory(d);
                StatusRepo.IncrementDone();
            }
        }

        void HandleModify(ICollection<string> modify, IDictionary<string, Status> statusDic,
            IEnumerable<FileObjectMapping> mappings) {
            StatusRepo.ResetWithoutClearItems(RepoStatus.Unpacking, modify.Count);
            modify.ForEach(x => {
                ProcessModified(statusDic, x, mappings);
                StatusRepo.IncrementDone();
            });
        }

        void HandleCopy(ICollection<KeyValuePair<string, List<string>>> copy, IDictionary<string, Status> statusDic) {
            StatusRepo.ResetWithoutClearItems(RepoStatus.Copying, copy.Count);
            copy.ForEach(x => {
                x.Value.ForEach(y => CopyExistingObject(statusDic, y, x.Key));
                StatusRepo.IncrementDone();
            });
        }

        IEnumerable<IAbsoluteDirectoryPath> GetEmptyDirectories(IEnumerable<string> remove) {
            return remove.Select(Path.GetDirectoryName)
                .Distinct()
                .Select(x => WorkingPath.GetChildDirectoryWithName(x))
                .Where(x => (Tools.FileUtil.IsDirectoryEmpty(x)));
        }

        void ProcessRemoved(IDictionary<string, Status> statusDic, string x) {
            var status = statusDic[x];
            status.Progress = 0;
            status.Action = RepoStatus.Removing;
            Tools.FileUtil.Ops.DeleteWithRetry(WorkingPath.GetChildFileWithName(x).ToString());
            status.EndOutput();
        }

        void ProcessModified(IDictionary<string, Status> statusDic, string dstName,
            IEnumerable<FileObjectMapping> mappings) {
            var status = statusDic[dstName];
            status.Progress = 0;
            status.Action = RepoStatus.Unpacking;
            var destFile = WorkingPath.GetChildFileWithName(dstName);
            var fcm = mappings.First(y => y.FilePath.Equals(dstName));
            var packedFile = Repository.GetObjectPath(fcm.Checksum);
            destFile.ParentDirectoryPath.MakeSurePathExists();

            Tools.Compression.Gzip.UnpackSingleGzip(packedFile, destFile, status);
            status.EndOutput();
        }

        void CopyExistingObject(IDictionary<string, Status> statusDic, string destName, string srcName) {
            var status = statusDic[destName];
            status.Progress = 0;
            status.Action = RepoStatus.Copying;
            var srcFile = WorkingPath.GetChildFileWithName(srcName);
            var destFile = WorkingPath.GetChildFileWithName(destName);
            destFile.ParentDirectoryPath.MakeSurePathExists();
            Tools.FileUtil.Ops.CopyWithRetry(srcFile, destFile);
            status.EndOutput();
        }

        void WriteTag() {
            Tools.FileUtil.Ops.CreateText(GetSynqInfoPath(), MetaData.GetFullName());
        }

        IAbsoluteFilePath GetSynqInfoPath() {
            return WorkingPath.GetChildFileWithName(SynqInfoFile);
        }

        static void BuildLogInfo(string type, IReadOnlyCollection<string> changes, StringBuilder overview,
            StringBuilder full) {
            overview.AppendLine(ShortStatusInfo(type, changes));
            full.AppendLine(FullChangeInfo(type, changes));
        }

        static string FullChangeInfo(string type, IReadOnlyCollection<string> changes) {
            return String.Format("{0} ({2}): {1}", type, string.Join(",", changes), changes.Count);
        }

        static string ShortStatusInfo(string type, IEnumerable<string> changes) {
            return String.Format("{0}: {1}", type, changes.Count());
        }

        class ChangeList
        {
            public readonly IDictionary<string, string> ChangedCase = new Dictionary<string, string>();
            public readonly IDictionary<string, List<string>> Copy = new Dictionary<string, List<string>>();
            public readonly List<string> Equal = new List<string>();
            public readonly List<string> New = new List<string>();
            public readonly List<string> Remove = new List<string>();
            public readonly IDictionary<string, Status> StatusDic = new Dictionary<string, Status>();
            public readonly List<string> Update = new List<string>();

            public ChangeList(IReadOnlyCollection<IAbsoluteFilePath> workingPathFiles, IOrderedEnumerable<FileObjectMapping> mappings,
                Package package) {
                package.StatusRepo.Reset(RepoStatus.Summing, workingPathFiles.Count);

                GenerateChangeImage(workingPathFiles, mappings, package);
            }

            public string[] GetModified() {
                return New.Concat(Update).ToArray();
            }

            public bool HasChanges(bool withRemoval) {
                return (withRemoval && Remove.Any()) || Update.Any() || New.Any() || Copy.Any() || ChangedCase.Any();
            }

            void GenerateChangeImage(IReadOnlyCollection<IAbsoluteFilePath> workingPathFiles,
                IOrderedEnumerable<FileObjectMapping> packageFileMappings,
                Package package) {
                var changeDictionary = new Dictionary<string, string>();

                foreach (var file in workingPathFiles)
                    MakeChecksum(package, file, changeDictionary);

                foreach (var f in changeDictionary)
                    EnumerateRemovals(f.Key, packageFileMappings);

                foreach (var file in packageFileMappings)
                    EnumerateChanges(file, changeDictionary, package);

                foreach (var file in ChangedCase.Where(file => Remove.Contains(file.Key)).ToArray())
                    Remove.Remove(file.Key);
            }

            void MakeChecksum(Package package, IAbsoluteFilePath file, Dictionary<string, string> changeDictionary) {
                var f = file.ToString().Replace(package.WorkingPath + @"\", String.Empty).Replace(@"\", "/");
                var status = new Status(f, package.StatusRepo) {Action = RepoStatus.Summing};
                StatusDic[f] = status;
                changeDictionary.Add(f, package.Repository.GetChecksum(file));
                status.EndOutput();
            }

            void EnumerateChanges(FileObjectMapping file, Dictionary<string, string> changeDictionary, Package package) {
                if (!StatusDic.ContainsKey(file.FilePath))
                    CreateStatusObject(file, package);

                var found = changeDictionary.FirstOrDefault(x => x.Key.Equals(file.FilePath));
                if (found.Key != null)
                    ProcessFoundByFilePath(file, found);
                else
                    ProcessNotFoundByFilePath(file, changeDictionary);
            }

            void CreateStatusObject(FileObjectMapping file, Package package) {
                StatusDic[file.FilePath] = new Status(file.FilePath, package.StatusRepo) {
                    RealObject = GetObjectPathFromChecksum(file)
                };
            }

            void ProcessFoundByChecksum(FileObjectMapping file, string found) {
                if (found.Equals(file.FilePath, StringComparison.InvariantCultureIgnoreCase))
                    ChangedCase.Add(found, file.FilePath);
                else if (Copy.ContainsKey(found))
                    Copy[found].Add(file.FilePath);
                else
                    Copy.Add(found, new List<string> {file.FilePath});
            }

            void ProcessNotFoundByFilePath(FileObjectMapping file, Dictionary<string, string> changeDictionary) {
                var found = changeDictionary.FirstOrDefault(x => x.Value.Equals(file.Checksum));
                if (found.Key != null)
                    ProcessFoundByChecksum(file, found.Key);
                else
                    New.Add(file.FilePath);
            }

            void ProcessFoundByFilePath(FileObjectMapping file, KeyValuePair<string, string> found) {
                if (found.Value.Equals(file.Checksum))
                    Equal.Add(file.FilePath);
                else
                    Update.Add(file.FilePath);
            }

            void EnumerateRemovals(string f, IEnumerable<FileObjectMapping> mappings) {
                var o = mappings.FirstOrDefault(x => x.FilePath.Equals(f));
                if (o == null)
                    Remove.Add(f);
            }
        }
    }
}