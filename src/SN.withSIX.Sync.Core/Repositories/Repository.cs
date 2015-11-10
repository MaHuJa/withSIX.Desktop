// <copyright company="SIX Networks GmbH" file="Repository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using MoreLinq;
using NDepend.Path;
using Newtonsoft.Json;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Services;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace SN.withSIX.Sync.Core.Repositories
{
    public class Repository : IEnableLogging, IDisposable
    {
        public const string DefaultRepoRootDirectory = ".synq";
        const string ObjectsDirectory = "objects";
        public const string PackagesDirectory = "packages";
        const string BundlesDirectory = "bundles";
        const string RemotesDirectory = "remotes";
        const string Format = ".json";
        public const string PackageFormat = Format;
        public const string ObjectIndexFile = ObjectsDirectory + Format;
        public const string PackageIndexFile = PackagesDirectory + Format;
        public const string CollectionIndexFile = BundlesDirectory + Format;
        public const string SerialFile = "serial";
        public const string LockFile = "lock.lock";
        public const string ConfigFile = "config" + Format;
        const int DefaultKeepPackages = 2;
        // TODO: Perhaps better to just exclude all folders starting with a dot?!
        static readonly string[] badRootDirectories = {
            DefaultRepoRootDirectory, ".sixarchive", Legacy.SixSync.Repository.RepoFolderName,
            ".git", ".svn"
        };
        // TODO: Perhaps better to just exclude all files starting with a dot?!
        static readonly string[] badRootFiles = {Package.SynqInfoFile, Package.SynqSpecFile};
        public static readonly RepositoryTools RepoTools = Legacy.SixSync.Repository.RepoTools;
        public static readonly RepositoryFactory Factory = new RepositoryFactory();
        public static readonly MappingEngine MappingEngine;
        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            Error = OnError
        }.SetDefaultSettings();
        readonly IAbsoluteFilePath _collectionIndexPath;
        readonly IAbsoluteFilePath _configPath;
        readonly IAbsoluteFilePath _lockFilePath;
        readonly IAbsoluteFilePath _objectIndexPath;
        readonly IAbsoluteFilePath _packageIndexPath;
        readonly SemaphoreSlim _remoteLock = new SemaphoreSlim(1);
        readonly IAbsoluteDirectoryPath _remotesPath;
        readonly object _saveLock = new Object();
        readonly IAbsoluteFilePath _serialPath;
        public readonly IAbsoluteDirectoryPath BundlesPath;
        public readonly IAbsoluteDirectoryPath ObjectsPath;
        public readonly IAbsoluteDirectoryPath PackagesPath;
        public readonly IAbsoluteDirectoryPath RootPath;
        bool _disposed;
        FileStream _lockFileStream;

        static Repository() {
            MappingEngine = GetMappingEngine();
        }

        public Repository(IAbsoluteDirectoryPath directory, bool createWhenNotExisting = false) {
            RootPath = directory;
            _configPath = RootPath.GetChildFileWithName(ConfigFile);
            _remotesPath = RootPath.GetChildDirectoryWithName(RemotesDirectory);
            ObjectsPath = RootPath.GetChildDirectoryWithName(ObjectsDirectory);
            PackagesPath = RootPath.GetChildDirectoryWithName(PackagesDirectory);
            BundlesPath = RootPath.GetChildDirectoryWithName(BundlesDirectory);
            _objectIndexPath = RootPath.GetChildFileWithName(ObjectIndexFile);
            _packageIndexPath = RootPath.GetChildFileWithName(PackageIndexFile);
            _collectionIndexPath = RootPath.GetChildFileWithName(CollectionIndexFile);
            _serialPath = RootPath.GetChildFileWithName(SerialFile);
            _lockFilePath = RootPath.GetChildFileWithName(LockFile);

            if (createWhenNotExisting)
                VerifyNewOrExistingRepository();
            else
                VerifyExistingRepository();

            Lock();

            Config = createWhenNotExisting
                ? TryLoad<RepositoryConfigDto, RepositoryConfig>(_configPath)
                : Load<RepositoryConfigDto, RepositoryConfig>(_configPath);
            Index = LoadIndex();
            Serial = LoadSerial();
        }

        public long Serial { get; set; }
        public RepositoryConfig Config { get; protected set; }
        public RepositoryStore Index { get; protected set; }
        public RepositoryRemote[] Remotes { get; private set; }

        public void Dispose() {
            Dispose(true);
        }

        long LoadSerial() {
            return _serialPath.Exists ? File.ReadAllText(_serialPath.ToString()).TryLong() : 0;
        }

        void Lock() {
            try {
                _lockFileStream = File.Open(_lockFilePath.ToString(),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None);
            } catch (IOException ex) {
                throw new RepositoryLockException("The repository appears to be locked", ex);
            }
        }

        void Unlock() {
            _lockFileStream.Dispose();
            _lockFilePath.FileInfo.Delete();
        }

        protected virtual void Dispose(bool d) {
            if (_disposed)
                return;
            _disposed = true;
            if (d) {
                File.WriteAllText(_serialPath.ToString(), Serial.ToString());
                Unlock();
            }
        }

        void VerifyExistingRepository() {
            VerifyIsNotFile();
            VerifyDirectoryExists();
            VerifyIndexFilesExist();
        }

        void VerifyNewOrExistingRepository() {
            VerifyIsNotFile();
            RootPath.MakeSurePathExists();
            VerifyDirectoryExists();
            CreateDirectoriesWhenMissing();
        }

        void VerifyIndexFilesExist() {
            foreach (var p in new[] {_objectIndexPath, _packageIndexPath, _collectionIndexPath}
                .Where(p => !p.Exists))
                throw new NotARepositoryException("file does not exist: " + p);
        }

        void CreateDirectoriesWhenMissing() {
            ObjectsPath.MakeSurePathExists();
            PackagesPath.MakeSurePathExists();
            BundlesPath.MakeSurePathExists();
        }

        void VerifyDirectoryExists() {
            if (!RootPath.Exists)
                throw new NotARepositoryException("directory does not exist: " + RootPath);
        }

        void VerifyIsNotFile() {
            if (File.Exists(RootPath.ToString()) && !Directory.Exists(RootPath.ToString()))
                throw new NotARepositoryException("path is file, not directory: " + RootPath);
        }

        static void OnError(object sender, ErrorEventArgs args) {
            if (args.ErrorContext.Path == "authors")
                args.ErrorContext.Handled = true;
        }

        static T FromDto<T>(object obj) {
            return MappingEngine.Map<T>(obj);
        }

        static MappingEngine GetMappingEngine() {
            var mappingConfig = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            mappingConfig.SetupConverters();

            // TODO: Should not allow Version or Branch to be changed after construct?
            mappingConfig.CreateMap<PackageMetaDataDto, PackageMetaData>()
                .ConstructUsing(src => new PackageMetaData(src.Name))
                .ForMember(x => x.Additional, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Dependencies, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Date, opt => opt.NullSubstitute(Tools.Generic.GetCurrentDateTime))
                .ForMember(x => x.Version, opt => opt.NullSubstitute(SpecificVersion.DefaultV))
                .ForMember(x => x.FullName, opt => opt.NullSubstitute(MetaDataBase.TodoFullName))
                .ForMember(x => x.Description, opt => opt.NullSubstitute(MetaDataBase.TodoDesc))
                .ForMember(x => x.Summary, opt => opt.NullSubstitute(MetaDataBase.TodoSummary))
                .ForMember(x => x.Authors,
                    opt => opt.NullSubstitute(MetaDataBase.TodoAuthors.ToDictionary(x => x.Key, x => x.Value)));

            mappingConfig.CreateMap<PackageMetaData, PackageMetaDataDto>()
                .ForMember(x => x.Additional, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Dependencies, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Date, opt => opt.NullSubstitute(Tools.Generic.GetCurrentDateTime))
                .ForMember(x => x.Version, opt => opt.NullSubstitute(SpecificVersion.DefaultV))
                .ForMember(x => x.FullName, opt => opt.NullSubstitute(MetaDataBase.TodoFullName))
                .ForMember(x => x.Description, opt => opt.NullSubstitute(MetaDataBase.TodoDesc))
                .ForMember(x => x.Summary, opt => opt.NullSubstitute(MetaDataBase.TodoSummary))
                .ForMember(x => x.Authors,
                    opt => opt.NullSubstitute(MetaDataBase.TodoAuthors.ToDictionary(x => x.Key, x => x.Value)));

            mappingConfig.CreateMap<PackageMetaData, PackageMetaData>()
                .ConstructUsing(src => new PackageMetaData(src.ToSpecificVersion()));
            mappingConfig.CreateMap<PackageMetaDataDto, PackageMetaDataDto>();

            mappingConfig.CreateMap<PackagesStoreCustomConfigs, PackagesStoreCustomConfigsDto>()
                .ForMember(x => x.KeepSpecificBranches, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.KeepSpecificVersions, opt => opt.Condition(src => !src.IsSourceValueNull));

            mappingConfig.CreateMap<PackagesStoreCustomConfigsDto, PackagesStoreCustomConfigs>()
                .ForMember(x => x.KeepSpecificBranches, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.KeepSpecificVersions, opt => opt.Condition(src => !src.IsSourceValueNull));


            mappingConfig.CreateMap<BundleDto, Bundle>()
                .ConstructUsing(src => new Bundle(src.Name))
                .ForMember(x => x.Additional, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Dependencies, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Date, opt => opt.NullSubstitute(Tools.Generic.GetCurrentDateTime))
                .ForMember(x => x.Version, opt => opt.NullSubstitute(SpecificVersion.DefaultV))
                .ForMember(x => x.FullName, opt => opt.NullSubstitute(MetaDataBase.TodoFullName))
                .ForMember(x => x.Description, opt => opt.NullSubstitute(MetaDataBase.TodoDesc))
                .ForMember(x => x.Summary, opt => opt.NullSubstitute(MetaDataBase.TodoSummary))
                .ForMember(x => x.Authors,
                    opt => opt.NullSubstitute(MetaDataBase.TodoAuthors.ToDictionary(x => x.Key, x => x.Value)));

            mappingConfig.CreateMap<Bundle, BundleDto>()
                .ForMember(x => x.Additional, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Dependencies, opt => opt.Condition(src => !src.IsSourceValueNull))
                .ForMember(x => x.Date, opt => opt.NullSubstitute(Tools.Generic.GetCurrentDateTime))
                .ForMember(x => x.Version, opt => opt.NullSubstitute(SpecificVersion.DefaultV))
                .ForMember(x => x.FullName, opt => opt.NullSubstitute(MetaDataBase.TodoFullName))
                .ForMember(x => x.Description, opt => opt.NullSubstitute(MetaDataBase.TodoDesc))
                .ForMember(x => x.Summary, opt => opt.NullSubstitute(MetaDataBase.TodoSummary))
                .ForMember(x => x.Authors,
                    opt => opt.NullSubstitute(MetaDataBase.TodoAuthors.ToDictionary(x => x.Key, x => x.Value)));

            mappingConfig.CreateMap<RepositoryStore, RepositoryStorePackagesDto>()
                .ForMember(x => x.PackagesContentTypes,
                    opt =>
                        opt.MapFrom(src => src.PackagesContentTypes.OrderBy(x => x.Key, StringComparer.InvariantCulture)
                            .ToDictionary(x => x.Key,
                                x => x.Value.OrderBy(y => y).ToList())))
                .ForMember(x => x.Packages,
                    opt => opt.MapFrom(src => src.Packages.OrderBy(x => x.Key, StringComparer.InvariantCulture)
                        .ToDictionary(x => x.Key,
                            x => x.Value.OrderBy(y => new SpecificVersion(x.Key, y).Version).ToList())))
                .ForMember(x => x.PackagesCustomConfigs,
                    opt =>
                        opt.MapFrom(
                            src => src.PackagesCustomConfigs.OrderBy(x => x.Key, StringComparer.InvariantCulture)
                                .ToDictionary(x => x.Key,
                                    x => x.Value)));

            mappingConfig.CreateMap<RepositoryStore, RepositoryStoreObjectsDto>()
                .ForMember(x => x.Objects,
                    opt => opt.MapFrom(src => src.Objects.OrderBy(x => x.Key, StringComparer.InvariantCulture)
                        .ToDictionary(x => x.Key, x => x.Value)));

            mappingConfig.CreateMap<RepositoryStore, RepositoryStoreBundlesDto>()
                .ForMember(x => x.Bundles,
                    opt => opt.MapFrom(src => src.Bundles.OrderBy(x => x.Key, StringComparer.InvariantCulture)
                        .ToDictionary(x => x.Key,
                            x => x.Value.OrderBy(y => new SpecificVersion(x.Key, y).Version).ToList())));


            mappingConfig.CreateMap<Bundle, Bundle>()
                .ConstructUsing(src => new Bundle(src.ToSpecificVersion()));
            mappingConfig.CreateMap<BundleDto, BundleDto>();

            mappingConfig.CreateMap<RepositoryConfig, RepositoryConfigDto>();
            mappingConfig.CreateMap<RepositoryConfigDto, RepositoryConfig>();
            mappingConfig.CreateMap<RepositoryConfig, RepositoryConfig>();
            mappingConfig.CreateMap<RepositoryConfigDto, RepositoryConfigDto>();

            return new MappingEngine(mappingConfig);
        }

        public IDictionary<string, string[]> GetPackages() {
            return Index.GetPackages();
        }

        public IEnumerable<string> GetPackagesList() {
            return Index.GetPackagesList();
        }

        public IEnumerable<SpecificVersion> GetPackagesListAsVersions() {
            return Index.GetPackagesListAsVersions();
        }

        public IEnumerable<string> GetBundlesList() {
            return Index.GetBundlesList();
        }

        public IEnumerable<SpecificVersion> GetBundlesListAsVersions() {
            return Index.GetBundlesListAsVersions();
        }

        public IEnumerable<SpecificVersion> GetPackageVersions(string package) {
            return Index.GetPackageVersions(package).Select(x => new SpecificVersion(package, x));
        }

        public bool HasPackage(string package) {
            return Index.HasPackage(package);
        }

        public bool HasPackage(Dependency package) {
            return Index.HasPackage(package);
        }

        public bool HasPackage(SpecificVersion package) {
            return Index.HasPackage(package);
        }

        public bool DeletePackage(SpecificVersion package, bool inclWorkFiles = false, bool inclDependencies = false) {
            if (!HasPackage(package))
                throw new Exception("Package not found: " + package);

            if (inclDependencies)
                throw new NotImplementedException();

            if (inclWorkFiles)
                throw new NotImplementedException();

            RemovePackage(package);
            var pp = GetLocalPackagePath(package);
            if (pp.Exists) {
                DeletePackageFilesIfExists(pp);
                return true;
            }
            return false;
        }

        public IEnumerable<SpecificVersion> DeletePackage(IEnumerable<SpecificVersion> packages,
            bool inclWorkFiles = false,
            bool inclDependencies = false) {
            return packages.Where(package => DeletePackage(package, inclWorkFiles, inclDependencies)).ToArray();
        }

        public RepositoryRemote AddRemote(Guid id, string commaSeperatedUrls) {
            return AddRemote(id, commaSeperatedUrls.Split(','));
        }

        public void AddRemotes(IEnumerable<KeyValuePair<Guid, Uri[]>> remotes) {
            foreach (var remote in remotes)
                AddRemote(remote.Key, remote.Value);
        }

        public void ClearRemotes() {
            lock (Config.Remotes)
                Config.Remotes.Clear();
        }

        public RepositoryRemote AddRemote(Guid id, IEnumerable<Uri> uris) {
            var array = uris.ToArray();
            lock (Config.Remotes) {
                if (Config.Remotes.ContainsKey(id)) {
                    array = Config.Remotes[id].Concat(array).Distinct().ToArray();
                    Config.Remotes[id] = array;
                } else
                    Config.Remotes.Add(id, array);
            }
            return new RepositoryRemote(array, id);
        }

        public RepositoryRemote ReplaceRemote(Guid id, IEnumerable<Uri> uris) {
            var array = uris.ToArray();
            lock (Config.Remotes) {
                if (Config.Remotes.ContainsKey(id))
                    Config.Remotes[id] = array;
                else
                    Config.Remotes.Add(id, array);
            }
            return new RepositoryRemote(array, id);
        }

        public RepositoryRemote AddRemote(Guid id, IEnumerable<string> urls) {
            return AddRemote(id, urls.Select(x => new Uri(x)));
        }

        public bool RemoveRemote(Guid id, IEnumerable<Uri> uris) {
            lock (Config.Remotes) {
                if (Config.Remotes.ContainsKey(id)) {
                    Config.Remotes[id] = Config.Remotes[id].Except(uris).ToArray();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveRemote(Guid id) {
            lock (Config.Remotes) {
                if (Config.Remotes.ContainsKey(id)) {
                    Config.Remotes.Remove(id);
                    return true;
                }
            }
            return false;
        }

        public async Task<IReadOnlyCollection<RepositoryRemote>> LoadRemotesAsync(string remote = null) {
            await _remoteLock.WaitAsync().ConfigureAwait(false);
            try {
                var remotes = GetRepositoryRemotes(remote).ToArray();
                await Task.WhenAll(remotes.Select(x => x.LoadAsync())).ConfigureAwait(false);
                return remotes;
            } finally {
                _remoteLock.Release();
            }
        }

        IEnumerable<RepositoryRemote> GetRepositoryRemotes(string remote) {
            return FreezeRemotes(remote).Select(x => new RepositoryRemote(x.Value, x.Key) {
                Path = _remotesPath.GetChildDirectoryWithName(x.Key.ToString())
            });
        }

        IReadOnlyCollection<KeyValuePair<Guid, Uri[]>> FreezeRemotes(string remote) {
            KeyValuePair<Guid, Uri[]>[] remotes;
            lock (Config.Remotes)
                remotes = GetRemotes(remote).ToArray();
            return remotes;
        }

        public async Task RefreshRemotes(string remote = null) {
            Remotes =
                (await LoadRemotesAsync(remote).ConfigureAwait(false)).Where(
                    x => x.Config.Uuid != Config.Uuid).ToArray();
        }

        public async Task UpdateRemotes() {
            await _remoteLock.WaitAsync().ConfigureAwait(false);
            try {
                var remotes = Remotes;
                if (!remotes.Any())
                    throw new Exception("No remotes found");
                foreach (var remote in remotes)
                    await remote.Update().ConfigureAwait(false);
            } finally {
                _remoteLock.Release();
            }
        }

        IEnumerable<KeyValuePair<Guid, Uri[]>> GetRemotes(string remote) {
            var remotes = (IEnumerable<KeyValuePair<Guid, Uri[]>>) Config.Remotes;
            if (remote != null) {
                remotes =
                    remotes.Where(
                        x => x.Key.ToString().Equals(remote) || x.Value.Any(y => y.ToString().Equals(remote)));
            }
            return remotes;
        }

        public IEnumerable<IAbsoluteFilePath> GetFiles(IAbsoluteDirectoryPath workingPath, string fileType = "*.*") {
            return
                Tools.FileUtil.GetFiles(workingPath, fileType, badRootDirectories, badRootFiles)
                    .Select(x => x.FullName.ToAbsoluteFilePath());
        }

        public Dictionary<string, string> Commit(IAbsoluteDirectoryPath workingPath, bool downCase = true) {
            var dictionary = new Dictionary<string, string>();
            var updated = GetFiles(workingPath)
                .Aggregate(0, (current, file) => UpdateOrAddObject(workingPath, downCase, file, current, dictionary));

            this.Logger().Info("Added/Updated {0} objects", updated);
            ++Serial;
            Save();

            return dictionary;
        }

        int UpdateOrAddObject(IAbsoluteDirectoryPath workingPath, bool downCase, IAbsoluteFilePath file, int updated,
            IDictionary<string, string> dictionary) {
            var r = AddObject(file);
            if (r.Item2)
                updated++;
            var oFileName = file.ToString().Replace(workingPath + @"\", String.Empty).Replace(@"\", "/");
            dictionary.Add(downCase ? oFileName.ToLower() : oFileName, r.Item1.Checksum);
            return updated;
        }

        public async Task<IEnumerable<string>> ListPackages(bool remote = false) {
            return remote
                ? (await LoadRemotesAsync().ConfigureAwait(false)).SelectMany(x => x.Index.GetPackagesList()).Distinct()
                : Index.GetPackagesList();
        }

        public Task<IEnumerable<string>> ListPackages(string commaSeparatedRemoteUuids) {
            return ListPackages(commaSeparatedRemoteUuids.Split(',').Select(x => new Guid(x)));
        }

        async Task<IEnumerable<string>> ListPackages(IEnumerable<Guid> remoteUuids) {
            return
                (await LoadRemotesAsync(remoteUuids.ToArray()).ConfigureAwait(false)).SelectMany(
                    x => x.Index.GetPackagesList()).Distinct();
        }

        async Task<IEnumerable<RepositoryRemote>> LoadRemotesAsync(params Guid[] remoteUuids) {
            return (await LoadRemotesAsync().ConfigureAwait(false)).Where(x => remoteUuids.Contains(x.Config.Uuid));
        }

        IEnumerable<string> GetLocalPackages() {
            return Directory.EnumerateFiles(PackagesPath.ToString(), "*.json")
                .Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        string[] GetInvalidPackages() {
            return (from p in GetLocalPackages()
                let depInfo = new SpecificVersion(p)
                let package = Package.TryLoad(PackagesPath.GetChildFileWithName(p + ".json"))
                where package == null || package.GetFullName() != depInfo.GetFullName()
                select p).ToArray();
        }

        async Task<string[]> GetStalePackages() {
            var listPackages = await ListPackages().ConfigureAwait(false);
            return listPackages
                .Except(GetLocalPackages())
                .ToArray();
        }

        async Task<string[]> RemoveStalePackages() {
            var stalePackages = await GetStalePackages().ConfigureAwait(false);
            Index.RemovePackage(stalePackages);

            return stalePackages;
        }

        async Task<string[]> AddUnknownPackages() {
            var unknownPackages = await GetUnknownPackages().ConfigureAwait(false);
            var packages = Index.AddPackage(unknownPackages);
            await SaveAsync().ConfigureAwait(false);
            return packages;
        }

        async Task<IEnumerable<string>> GetUnknownPackages() {
            var listPackages = await ListPackages().ConfigureAwait(false);
            return GetLocalPackages()
                .Except(listPackages)
                .ToArray();
        }

        IEnumerable<ObjectInfo> GetStaleObjects() {
            var objects = Index.GetObjects();
            return objects
                .Where(x => !GetObjectPath(x.Key).Exists)
                .Select(x => new ObjectInfo(x.Key, x.Value))
                .ToArray();
        }

        ObjectInfo[] RemoveStaleObjects() {
            var objects = GetStaleObjects().Where(o => Index.RemoveObject(o)).ToArray();
            if (objects.Any())
                SaveIndex();
            return objects;
        }

        ObjectInfo[] GetObsoleteObjects() {
            var list = GetObjectsFromAllPackages();
            var objects = Index.GetObjects();
            return objects
                .Where(x => !list.Contains(x.Key))
                .Select(x => new ObjectInfo(x.Key, x.Value))
                .ToArray();
        }

        IEnumerable<string> GetObjectsFromAllPackages() {
            return GetPackagesListAsVersions()
                .SelectMany(TryGetObjects).Distinct().ToArray();
        }

        IEnumerable<string> TryGetObjects(SpecificVersion x) {
            var package = Package.TryLoad(GetLocalPackagePath(x));
            return package == null ? Enumerable.Empty<string>() : package.Files.Select(y => y.Value);
        }

        public ObjectInfo[] GetInvalidObjects() {
            var objects = Index.GetObjects();
            return objects
                .Where(x => {
                    var objP = GetObjectPath(x.Key);
                    return !objP.Exists || GetChecksum(objP) != x.Value;
                })
                .Select(x => new ObjectInfo(x.Key, x.Value))
                .ToArray();
        }

        string[] GetUnknownObjects() {
            var objects = Index.GetObjects();
            return GetFiles(ObjectsPath, "*.")
                .Select(x => GetObjectHash(x.ToString().Replace(ObjectsPath.ToString(), String.Empty).Replace(@"\", "/")))
                .Except(objects.Keys)
                .ToArray();
        }

        string[] RemoveUnknownObjects() {
            var objects = GetUnknownObjects();
            foreach (var o in objects)
                DeleteObjectFilesIfExists(o);
            return objects;
        }

        public ObjectInfo[] RemoveObsoleteObjects() {
            var objects = GetObsoleteObjects();
            foreach (var o in objects)
                DeleteObject(o);
            if (objects.Any())
                SaveIndex();
            return objects;
        }

        public string GetChecksum(IAbsoluteFilePath fileName) {
            return Tools.HashEncryption.SHA1FileHash(fileName);
        }

        public string[] AddPackage(IEnumerable<string> packages) {
            var added = Index.AddPackage(packages);
            if (added.Any())
                SaveIndex();
            return added;
        }

        public string[] AddBundle(IEnumerable<string> bundles) {
            var added = Index.AddBundle(bundles);
            if (added.Any())
                SaveIndex();
            return added;
        }

        public string[] RemovePackage(params string[] packages) {
            return RemovePackage((IEnumerable<string>) packages);
        }

        public string[] RemovePackage(IEnumerable<string> packages) {
            var removed = Index.RemovePackage(packages);
            if (removed.Any())
                SaveIndex();
            return removed;
        }

        public bool AddPackage(string package) {
            var added = Index.AddPackage(package);
            if (added)
                SaveIndex();
            return added;
        }

        public bool AddBundle(string collection) {
            var added = Index.AddBundle(collection);
            if (added)
                SaveIndex();
            return added;
        }

        public bool RemovePackage(string package) {
            return RemovePackage(new SpecificVersion(package));
        }

        public bool RemovePackage(SpecificVersion package) {
            var removed = Index.RemovePackage(package);
            if (removed)
                SaveIndex();
            return removed;
        }

        public Tuple<ObjectInfo, bool> AddObject(IAbsoluteFilePath filePath) {
            var hash = GetChecksum(filePath);
            var o = GetObject(hash);
            var added = false;
            if (o == null) {
                var packedHash = CompressObject(filePath, hash);
                o = Index.AddObject(hash, packedHash);
                added = true;
            }
            return Tuple.Create(o, added);
        }

        public bool DeleteObject(ObjectInfo info) {
            if (!Index.RemoveObject(info))
                return false;

            DeleteObjectFilesIfExists(info);
            return true;
        }

        public bool DeleteObject(IEnumerable<ObjectInfo> infos) {
            var done = false;
            foreach (var o in infos) {
                if (DeleteObject(o))
                    done = true;
            }
            return done;
        }

        void DeleteObjectFilesIfExists(ObjectInfo info) {
            DeleteObjectFilesIfExists(info.Checksum);
        }

        void DeleteObjectFilesIfExists(string unpackedHash) {
            var path = GetObjectPath(unpackedHash);

            if (path.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(path.ToString());
            var zsyncFile = path + ".zsync";
            if (File.Exists(zsyncFile))
                Tools.FileUtil.Ops.DeleteWithRetry(zsyncFile);

            var dir = path.ParentDirectoryPath;
            if (Tools.FileUtil.IsDirectoryEmpty(dir))
                Tools.FileUtil.Ops.DeleteWithRetry(dir.ToString());
        }

        static void DeletePackageFilesIfExists(IAbsoluteFilePath path) {
            if (path.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(path.ToString());
            var zsyncFile = path + ".zsync";
            if (File.Exists(zsyncFile))
                Tools.FileUtil.Ops.DeleteWithRetry(zsyncFile);
        }

        public string CompressObject(IAbsoluteFilePath filePath, string hash) {
            Tools.Compression.Gzip.GzipAuto(filePath);
            var tempDestFileName = (filePath + ".gz").ToAbsoluteFilePath();
            var packedHash = GetChecksum(tempDestFileName);

            var objectPath = GetObjectPath(hash);
            objectPath.ParentDirectoryPath.MakeSurePathExists();
            Tools.FileUtil.Ops.MoveWithRetry(tempDestFileName, objectPath);

            return packedHash;
        }

        public void CopyObject(string hash1, string hash2) {
            var dest = GetObjectPath(hash2);
            dest.ParentDirectoryPath.MakeSurePathExists();
            Tools.FileUtil.Ops.CopyWithRetry(GetObjectPath(hash1), dest);
        }

        void ReAddObject(string hash) {
            var filePath = GetObjectPath(hash);
            if (!filePath.Exists)
                throw new Exception("File not found");
            var packedHash = GetChecksum(filePath);
            var o = GetObject(hash);
            if (o != null && o.ChecksumPack == packedHash)
                return;
            Index.AddObject(hash, packedHash);
        }

        public void ReAddObject(params string[] hashes) {
            hashes.ForEach(ReAddObject);
            SaveIndex();
        }

        public ObjectInfo GetObject(string unpackedHash) {
            return Index.GetObject(unpackedHash);
        }

        public ObjectInfo GetObjectByPacked(string packedHash) {
            return Index.GetObjectByPack(packedHash);
        }

        public IAbsoluteFilePath GetObjectPath(string unpackedHash) {
            return ObjectsPath.GetChildFileWithName(GetObjectSubPath(unpackedHash));
        }

        public IAbsoluteFilePath GetObjectPath(ObjectInfo o) {
            return ObjectsPath.GetChildFileWithName(GetObjectSubPath(o));
        }

        public string GetObjectSubPath(string unpackedHash) {
            return Tools.Transfer.JoinPaths(unpackedHash.Substring(0, 2), unpackedHash.Substring(2));
        }

        public string GetObjectSubPath(FileObjectMapping o) {
            return GetObjectSubPath(o.Checksum);
        }

        public IAbsoluteFilePath GetObjectPath(FileObjectMapping o) {
            return GetObjectPath(o.Checksum);
        }

        string GetObjectSubPath(ObjectInfo o) {
            return GetObjectSubPath(o.Checksum);
        }

        string GetObjectHash(string path) {
            return String.Join(String.Empty, path.Split('/'));
        }

        public IAbsoluteFilePath GetMetaDataPath(string packageName) {
            return PackagesPath.GetChildFileWithName(packageName + ".json");
        }

        public SpecificVersion ResolvePackageName(string packageName) {
            return ResolvePackageName(new Dependency(packageName));
        }

        public SpecificVersion ResolvePackageName(Dependency package) {
            return Index.GetPackage(package);
        }

        [Obsolete("Use SaveAsync")]
        public void Save() {
            lock (_saveLock) {
                SaveConfig();
                SaveIndex();
            }
        }

        public Task SaveAsync() {
            return Task.Run(() => Save());
        }

        public static void SaveDto(object dto, IAbsoluteFilePath path) {
            Tools.Serialization.Json.SaveJsonToDiskThroughMemory(dto, path, true);
        }

        [Obsolete("Use SaveConfigAsync")]
        public void SaveConfig() {
            lock (_saveLock)
                SaveDto(new RepositoryConfigDto(Config), _configPath);
        }

        public Task SaveConfigAsync() {
            return Task.Run(() => SaveConfig());
        }

        RepositoryStore LoadIndex() {
            return RepositoryStore.FromSeparateStores(TryLoad<RepositoryStoreObjectsDto>(_objectIndexPath),
                TryLoad<RepositoryStorePackagesDto>(_packageIndexPath),
                TryLoad<RepositoryStoreBundlesDto>(_collectionIndexPath));
        }

        public static T TryLoad<T>(IAbsoluteFilePath path) where T : class, new() {
            return HandleLoadExceptions(Load<T>, path);
        }

        static T Load<T>(IAbsoluteFilePath path) where T : class {
            Contract.Requires<ArgumentNullException>(path != null);
            var document = Tools.Serialization.Json.LoadTextFromFile(path);
            if (string.IsNullOrWhiteSpace(document))
                throw new ConfigurationException("empty document " + path);
            var loaded = DeserializeJson<T>(document);
            if (loaded == null)
                throw new ConfigurationException("json object should not be null: " + path);
            return loaded;
        }

        public static T TryLoad<TIn, T>(IAbsoluteFilePath path) where T : class, new() where TIn : class {
            Contract.Requires<ArgumentNullException>(path != null);
            return HandleLoadExceptions(Load<TIn, T>, path);
        }

        public static T Load<TIn, T>(IAbsoluteFilePath path) where TIn : class {
            Contract.Requires<ArgumentNullException>(path != null);
            return FromDto<T>(Load<TIn>(path));
        }

        public static TIn DeserializeJson<TIn>(string path) where TIn : class {
            Contract.Requires<ArgumentNullException>(path != null);
            return Tools.Serialization.Json.LoadJson<TIn>(path, JsonSettings);
        }

        static T HandleLoadExceptions<T>(Func<IAbsoluteFilePath, T> func, IAbsoluteFilePath path) where T : class, new() {
            try {
                return func(path);
            } catch (FileNotFoundException) {
                return new T();
            } catch (ConfigurationException e) {
                MainLog.Logger.FormattedWarnException(e, "Warning during loading json file: " + path);
                return new T();
            } catch (JsonReaderException e) {
                MainLog.Logger.FormattedWarnException(e, "Warning during loading json file: " + path);
                return new T();
            }
        }

        void SaveIndex() {
            lock (_saveLock) {
                SaveDto(MappingEngine.Map<RepositoryStoreObjectsDto>(Index), _objectIndexPath);
                SaveDto(MappingEngine.Map<RepositoryStorePackagesDto>(Index), _packageIndexPath);
                SaveDto(MappingEngine.Map<RepositoryStoreBundlesDto>(Index), _collectionIndexPath);
            }
        }

        public Task DownloadPackage(string packageName, IEnumerable<Uri> remotes, CancellationToken token) {
            return DownloadAndConfirm<PackageMetaDataDto>(GetRemotePackagePath(packageName), remotes, token);
        }

        public Task DownloadBundle(string collectionName, IEnumerable<Uri> remotes, CancellationToken token) {
            return DownloadAndConfirm<BundleDto>(GetRemoteBundlePath(collectionName), remotes, token);
        }

        Task DownloadAndConfirm<TIn>(string file, IEnumerable<Uri> remotes, CancellationToken token) where TIn : class {
            return SyncEvilGlobal.DownloadHelper.DownloadFileAsync(file, RootPath, remotes.ToArray(), token, 10,
                ConfirmValidity<TIn>);
        }

        public static bool ConfirmValidity<TIn>(IAbsoluteFilePath filePath) where TIn : class {
            try {
                Load<TIn>(filePath);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public Dictionary<string, string[]> GetBundles() {
            return Index.GetBundles();
        }

        public void DeleteBundle(IEnumerable<string> bundles, BundleScope scope = BundleScope.All,
            bool inclPackages = false, bool inclDependencies = false, bool inclPackageWorkFiles = false) {
            foreach (var collection in bundles)
                DeleteBundle(collection, scope, inclPackages, inclDependencies, inclPackageWorkFiles);
        }

        void DeleteBundle(string bundle, BundleScope scope = BundleScope.All, bool inclPackages = false,
            bool inclDependencies = false, bool inclPackageWorkFiles = false) {
            if (!inclPackages && inclPackageWorkFiles)
                throw new Exception("Include Packges wasn't specified, but PackageWorkFiles was");

            if (!Index.HasBundle(bundle))
                throw new Exception("Unknown bundle: " + bundle);

            var col = Bundle.Factory.Open(BundlesPath.GetChildFileWithName(bundle + PackageFormat));
            if (inclPackages)
                DeletePackage(col.GetAllPackages(scope).Select(x => new SpecificVersion(x.Key, x.Value)));
        }

        public async Task Repair() {
            await RemovePackagesNotExistingOnDisk().ConfigureAwait(false);
            await AddPackagesOnDiskNotAvailableInIndex().ConfigureAwait(false);
            ListPackagesWithInvalidVersionMetadata();
            RemoveObjectsThatAreNotReferencedByAnyPackages();
            RemoveObjectsNotExistingOnDisk();
            RemoveObjectsFromDiskNotInIndex();
            ListPackagesWithObjectsMissing();
            RebuildContentTypeIndex();
        }

        void RebuildContentTypeIndex() {
            Index.PackagesContentTypes = new Dictionary<string, List<string>>();
            var failed = new List<string>();
            foreach (
                var metaData in
                    Index.Packages.Select(
                        package =>
                            Package.Load(
                                GetMetaDataPath(new SpecificVersion(package.Key, package.Value.Last()).GetFullName())))
                ) {
                if (string.IsNullOrWhiteSpace(metaData.ContentType))
                    failed.Add(metaData.Name);
                else
                    SetContentType(metaData.Name, metaData.ContentType);
            }
            SaveIndex();
            Log("Rebuilt content type index.\nPackages without type: {0}", string.Join(", ", failed));
        }

        public static void Log(string message, params object[] pars) {
            MainLog.Logger.Info(message, pars);
            Console.WriteLine(message, pars);
        }

        void ListPackagesWithObjectsMissing() {
            var incompletePackages = GetIncompletePackages();
            Log("Incomplete Packages ({0}): {1}", incompletePackages.Count,
                String.Join(", ",
                    incompletePackages.Select(
                        x => x.Key + ": " + string.Join(", ", x.Value.Select(fileObject => fileObject.Checksum)))));
        }

        void RemoveObjectsFromDiskNotInIndex() {
            var unknownO = RemoveUnknownObjects();
            Log("Removed Unknown Objects ({0}): {1}", unknownO.Length, String.Join(", ", unknownO));
        }

        void RemoveObjectsNotExistingOnDisk() {
            var staleO = RemoveStaleObjects();
            Log("Removed Stale Objects ({0}): {1}", staleO.Length,
                String.Join(", ", staleO.Select(x => x.Checksum)));
        }

        void RemoveObjectsThatAreNotReferencedByAnyPackages() {
            var obsolete = RemoveObsoleteObjects();
            Log("Removed Obsolete Objects ({0}): {1}", obsolete.Length,
                String.Join(", ", obsolete.Select(x => x.Checksum)));
        }

        void ListPackagesWithInvalidVersionMetadata() {
            var invalidPackages = GetInvalidPackages();
            Log("Invalid Packages ({0}): {1}", invalidPackages.Length,
                String.Join(", ", invalidPackages));
        }

        async Task AddPackagesOnDiskNotAvailableInIndex() {
            var unknownPackages = await AddUnknownPackages().ConfigureAwait(false);
            Log("Added Unknown Packages ({0}): " + String.Join(", ", unknownPackages),
                unknownPackages.Length);
        }

        async Task RemovePackagesNotExistingOnDisk() {
            var removedStalePackages = await RemoveStalePackages().ConfigureAwait(false);
            Log("Removed Stale Packages ({0}): " + String.Join(", ", removedStalePackages),
                removedStalePackages.Length);
        }

        IDictionary<string, FileObjectMapping[]> GetIncompletePackages() {
            var dict = new Dictionary<string, FileObjectMapping[]>();
            foreach (var package in LoadLocalPackages()) {
                var missingFiles = GetMissingFiles(package);
                if (missingFiles.Any())
                    dict.Add(package.GetFullName(), missingFiles.ToArray());
            }
            return dict;
        }

        IEnumerable<PackageMetaData> LoadLocalPackages() {
            return GetLocalPackages()
                .Select(p => Package.TryLoad(PackagesPath.GetChildFileWithName(p + ".json"))).Where(x => x != null);
        }

        List<FileObjectMapping> GetMissingFiles(PackageMetaData package) {
            return package.GetFiles().Where(file => !GetObjectPath(file).Exists).ToList();
        }

        public bool HasBundle(SpecificVersion bundle) {
            return Index.HasBundle(bundle);
        }

        public Task SavePackageAsync(Package package) {
            return Task.Run(() => SavePackage(package));
        }

        [Obsolete("Use SavePackageAsync")]
        public void SavePackage(Package package) {
            var fullName = package.GetFullName();
            var metaDataPath = GetMetaDataPath(fullName);
            SaveDto(MappingEngine.Map<PackageMetaDataDto>(package.MetaData), metaDataPath);
            AddPackage(fullName);
        }

        public void SetContentType(string name, string contentType) {
            lock (Index.PackagesContentTypes) {
                if (Index.PackagesContentTypes.ContainsKey(contentType)) {
                    if (!Index.PackagesContentTypes[contentType].Contains(name))
                        Index.PackagesContentTypes[contentType].Add(name);
                } else
                    Index.PackagesContentTypes.Add(contentType, new[] {name}.ToList());
            }
        }

        IEnumerable<SpecificVersion> CleanPackageBasedOnSetting(string packageName,
            IEnumerable<SpecificVersion> keepVersions) {
            var keep = CalculateKeepPackages(packageName);
            return keep == -1 ? Enumerable.Empty<SpecificVersion>() : CleanPackage(packageName, keepVersions, keep);
        }

        int CalculateKeepPackages(string packageName) {
            return
                (GetPackageKeepValue(packageName) ?? GetGlobalKeepPackageValue()).GetValueOrDefault(DefaultKeepPackages);
        }

        IEnumerable<SpecificVersion> CleanPackage(string packageName, IEnumerable<SpecificVersion> keepVersions,
            int limit) {
            return DeletePackage(GetOlderPackages(packageName, limit).Where(x => ShouldDeletePackage(x, keepVersions)));
        }

        int? GetPackageKeepValue(string packageName) {
            if (!Index.PackagesCustomConfigs.ContainsKey(packageName))
                return null;
            var customPackageInfo = Index.PackagesCustomConfigs[packageName];
            return customPackageInfo.KeepLatestVersions;
        }

        int? GetGlobalKeepPackageValue() {
            return Config.KeepVersionsPerPackage;
        }

        bool ShouldDeletePackage(SpecificVersion package, IEnumerable<SpecificVersion> keepVersions = null) {
            if (keepVersions != null && keepVersions.Contains(package))
                return false;
            if (!Index.PackagesCustomConfigs.ContainsKey(package.Name))
                return true;
            var customPackageInfo = Index.PackagesCustomConfigs[package.Name];
            return customPackageInfo.KeepSpecificVersions.Contains(package.Version) ||
                   customPackageInfo.KeepSpecificBranches.Contains(package.Branch);
        }

        IEnumerable<SpecificVersion> GetOlderPackages(string package) {
            return GetPackageVersions(package).Reverse();
        }

        IEnumerable<SpecificVersion> GetOlderPackages(string package, int limit) {
            return GetOlderPackages(package).Skip(limit);
        }

        async Task<IReadOnlyCollection<SpecificVersion>> CleanPackages(IReadOnlyCollection<string> packages,
            IReadOnlyCollection<SpecificVersion> keepVersions, int? limit = null) {
            var cleaned = new List<SpecificVersion>();
            var notExistentPackages = packages.Where(x => !HasPackage(x)).ToArray();
            if (notExistentPackages.Any()) {
                throw new PackageNotFoundException("Did not find the following packages: " +
                                          string.Join(", ", notExistentPackages));
            }

            if (limit.HasValue && limit.Value == -1)
                return cleaned;

            cleaned.AddRange(limit.HasValue
                ? packages.SelectMany(p => CleanPackage(p, keepVersions, limit.Value)).ToArray()
                : packages.SelectMany(p => CleanPackageBasedOnSetting(p, keepVersions)).ToArray());

            if (cleaned.Any())
                await Repair().ConfigureAwait(false);

            return cleaned;
        }

        public Task<IReadOnlyCollection<SpecificVersion>> CleanPackageAsync(IReadOnlyCollection<string> packages,
            IReadOnlyCollection<SpecificVersion> keepVersions = null,
            int? limit = null) {
            return Task.Run(() => CleanPackages(packages, keepVersions, limit));
        }

        IAbsoluteFilePath GetLocalPackagePath(SpecificVersion package) {
            return GetLocalPackagePath(package.ToString());
        }

        IAbsoluteFilePath GetLocalPackagePath(string package) {
            return PackagesPath.GetChildFileWithName(package + PackageFormat);
        }

        static string GetRemotePackagePath(string package) {
            return Path.Combine(PackagesDirectory, package + PackageFormat);
        }

        IAbsoluteFilePath GetLocalBundlePath(string bundle) {
            return BundlesPath.GetChildFileWithName(bundle + PackageFormat);
        }

        static string GetRemoteBundlePath(string bundle) {
            return Path.Combine(BundlesDirectory, bundle + PackageFormat);
        }

        public async Task<SpecificVersion> Yank(Dependency def, IAbsoluteDirectoryPath workDir) {
            //  Contract.Requires<ArgumentNullException>(def != null);
            //  Contract.Requires<ArgumentNullException>(workDir != null);

            var packages = GetPackages();
            var versions = packages[def.Name].ToList();
            if (versions.Count < 2) {
                throw new InvalidOperationException(def.Name +
                                                    " has less than 2 versions available. Use RmCommand instead");
            }

            var yankPackage = def.VersionData != null ? def : new Dependency(def.Name, versions.Last());
            DeletePackage(yankPackage.ToSpecificVersion());
            await SaveAsync().ConfigureAwait(false);

            versions.Remove(versions.Last());

            var nextInline = new SpecificVersion(def.Name, versions.Last());
            var package = Package.Factory.Open(this, workDir, nextInline.ToString());
            await package.CheckoutAsync().ConfigureAwait(false);

            return nextInline;
        }

        public async Task YankAndDeregister(Dependency def, IAbsoluteDirectoryPath workDir, IPublishingApi publishingApi,
            string registerKey) {
            //            Contract.Requires<ArgumentNullException>(def != null);
            //            Contract.Requires<ArgumentNullException>(workDir != null);
            //            Contract.Requires<ArgumentNullException>(publishingApi != null);
            //            Contract.Requires<ArgumentNullException>(registerKey != null);

            var nextInline = await Yank(def, workDir).ConfigureAwait(false);
            await publishingApi.Deversion(nextInline, registerKey).ConfigureAwait(false);
        }

        public async Task ReversionAndRegister(Dependency dependency, string newVersion, IAbsoluteDirectoryPath workDir,
            IPublishingApi publishingApi, string registerKey) {
            //            Contract.Requires<ArgumentNullException>(dependency != null);
            //            Contract.Requires<ArgumentNullException>(newVersion != null);
            //            Contract.Requires<ArgumentNullException>(workDir != null);
            //            Contract.Requires<ArgumentNullException>(publishingApi != null);
            //            Contract.Requires<ArgumentNullException>(registerKey != null);

            var package = await Reversion(dependency, workDir, newVersion).ConfigureAwait(false);
            await package.Register(publishingApi, null, registerKey).ConfigureAwait(false);
        }

        public async Task<Package> Reversion(Dependency def, IAbsoluteDirectoryPath workDir, string newVersion) {
            //            Contract.Requires<ArgumentNullException>(def != null);
            //          Contract.Requires<ArgumentNullException>(newVersion != null);
            //        Contract.Requires<ArgumentNullException>(workDir != null);

            var packages = GetPackages();
            var versions = packages[def.Name].ToList();

            var reversionPackage = def.VersionData != null ? def : new Dependency(def.Name, versions.Last());

            var package = Package.Factory.Open(this, workDir, reversionPackage.ToString());

            var newDep = new SpecificVersion(def.Name, newVersion);
            DeletePackage(reversionPackage.ToSpecificVersion());
            package.MetaData.Version = newDep.Version;
            package.MetaData.Branch = newDep.Branch;
            await SavePackageAsync(package).ConfigureAwait(false);
            await SaveAsync().ConfigureAwait(false);
            return package;
        }
    }

    public class RepositoryLockException : Exception
    {
        public RepositoryLockException(string theRepositoryAppearsToBeLocked, IOException ioException)
            : base(theRepositoryAppearsToBeLocked, ioException) {}
    }

    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) {}
    }

    [DoNotObfuscate]
    class FileExistsException : Exception
    {
        public FileExistsException(string rootPath) : base(rootPath) {}
    }

    [DoNotObfuscate]
    public class NotARepositoryException : Exception
    {
        public NotARepositoryException(string folderDoesNotExist) : base(folderDoesNotExist) {}
    }
}