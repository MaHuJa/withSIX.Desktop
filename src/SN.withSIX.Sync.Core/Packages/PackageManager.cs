// <copyright company="SIX Networks GmbH" file="PackageManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Packages
{
    public class PackageManagerSettings
    {
        [Obsolete(
            "Should no longer be needed once we refactor all to install packages to a certain folder, and then use symlinks when needed"
            )]
        public IAbsoluteDirectoryPath GlobalWorkingPath { get; set; }
        public CheckoutType CheckoutType { get; set; } = CheckoutType.NormalCheckout;
    }

    public enum CheckoutType
    {
        NormalCheckout,
        CheckoutWithoutRemoval
    }

    public class PackageManager : IEnableLogging
    {
        readonly string _remote;
        public readonly Repository Repo;

        public PackageManager(Repository repo, IAbsoluteDirectoryPath workDir, bool createWhenNotExisting = false,
            string remote = null) {
            Contract.Requires<ArgumentNullException>(repo != null);
            Contract.Requires<ArgumentNullException>(workDir != null);
            WorkDir = workDir;
            Repo = repo;
            StatusRepo = new StatusRepo();
            Settings = new PackageManagerSettings();

            Repository.Factory.HandlePathRequirements(WorkDir, Repo);

            if (!WorkDir.Exists) {
                if (!createWhenNotExisting)
                    throw new Exception("Workdir doesnt exist");
                WorkDir.MakeSurePathExists();
            }

            if (!string.IsNullOrWhiteSpace(remote)) {
                var config =
                    Repository.DeserializeJson<RepositoryConfigDto>(
                        FetchString(Tools.Transfer.JoinUri(new Uri(remote), "config.json")));
                if (config.Uuid == Guid.Empty)
                    throw new Exception("Invalid remote, does not contain an UUID");
                Repo.AddRemote(config.Uuid, remote);
                Repo.Save();
            }

            Repository.Log("Opening repository at: {0}. Working directory at: {1}", Repo.RootPath, WorkDir);
            _remote = remote;
        }

        public PackageManagerSettings Settings { get; }
        public StatusRepo StatusRepo { get; set; }
        public IAbsoluteDirectoryPath WorkDir { get; }

        [Obsolete("Workaround for classic callers")]
        public static async Task<PackageManager> Create(Repository repo, IAbsoluteDirectoryPath workDir,
            bool createWhenNotExisting = false,
            string remote = null) {
            var pm = new PackageManager(repo, workDir, createWhenNotExisting, remote);
            await repo.RefreshRemotes().ConfigureAwait(false);
            return pm;
        }

        void HandleRemotes(string[] remotes) {
            var config = GetConfigFromRemotes(remotes);
            Repo.AddRemote(config.Uuid, remotes);
            Repo.Save();
        }

        RepositoryConfigDto GetConfigFromRemotes(IEnumerable<string> remotes) {
            RepositoryConfigDto config = null;
            foreach (var r in remotes) {
                try {
                    config = TryGetConfigFromRemote(r);
                    break;
                } catch (Exception e) {
                    this.Logger().FormattedWarnException(e, "failure to retrieve config.json");
                }
            }
            if (config == null)
                throw new Exception("Could not get a valid config from any remote");
            return config;
        }

        static RepositoryConfigDto TryGetConfigFromRemote(string r) {
            var config = Repository.DeserializeJson<RepositoryConfigDto>(
                FetchString(Tools.Transfer.JoinUri(new Uri(r), "config.json")));
            if (config.Uuid == Guid.Empty)
                throw new Exception("Invalid remote, does not contain an UUID");
            return config;
        }

        static string FetchString(Uri uri) {
            return SyncEvilGlobal.StringDownloader.Download(uri);
        }

        public async Task UpdateRemotes() {
            await Repo.RefreshRemotes(_remote).ConfigureAwait(false);
            await Repo.UpdateRemotes().ConfigureAwait(false);
        }

        // TODO: Async
        public Package[] Checkout(IReadOnlyCollection<string> packageNames, bool? useFullNameOverride = null) {
            StatusRepo.Reset(RepoStatus.CheckOut, packageNames.Count());
            return packageNames.Select(package => Checkout(package, useFullNameOverride)).ToArray();
        }

        Package Checkout(string packageName, bool? useFullNameOverride = null) {
            var useFullName = Repo.Config.UseVersionedPackageFolders;

            if (useFullNameOverride.HasValue)
                useFullName = useFullNameOverride.Value;

            var depInfo = Repo.ResolvePackageName(packageName);
            if (depInfo == null)
                throw new Exception("Could not resolve package " + packageName);

            var resolvedPackageName = depInfo.GetFullName();

            IAbsoluteDirectoryPath dir;
            if (Repo.Config.OperationMode == RepositoryOperationMode.SinglePackage)
                dir = WorkDir;
            else {
                var name = useFullName ? depInfo.GetFullName() : depInfo.Name;
                var repoRoot = Repo.RootPath.ParentDirectoryPath;
                //dir = Path.Combine(repoRoot, name);
                var d = Tools.FileUtil.IsPathRootedIn(WorkDir, repoRoot, true) ? repoRoot : WorkDir;
                dir = d.GetChildDirectoryWithName(name);
            }

            Repository.Log("\nChecking out {0} into {1}, please be patient...", resolvedPackageName, dir);
            var package = Package.Factory.Open(Repo, dir, resolvedPackageName);
            package.StatusRepo = StatusRepo;
            package.Checkout();

            Repository.Log("\nSuccesfully checked out {0}", package.MetaData.GetFullName());

            return package;
        }

        public Task<Package[]> ProcessPackage(SpecificVersion package, bool? useFullNameOverride = null,
            bool noCheckout = false, bool skipWhenFileMatches = true) {
            return ProcessPackage(package.ToDependency(), useFullNameOverride, noCheckout, skipWhenFileMatches);
        }

        public Task<Package[]> ProcessPackage(Dependency package, bool? useFullNameOverride = null,
            bool noCheckout = false, bool skipWhenFileMatches = true) {
            return ProcessPackages(new[] {package}, useFullNameOverride, noCheckout, skipWhenFileMatches);
        }

        public Task<Package[]> ProcessPackages(IEnumerable<SpecificVersion> packageNames,
            bool? useFullNameOverride = null,
            bool noCheckout = false, bool skipWhenFileMatches = true) {
            return ProcessPackages(packageNames.Select(x => x.ToDependency()), useFullNameOverride, noCheckout,
                skipWhenFileMatches);
        }

        public async Task<Package[]> ProcessPackages(IEnumerable<Dependency> packageNames,
            bool? useFullNameOverride = null,
            bool noCheckout = false, bool skipWhenFileMatches = true) {
            if (Repo.Config.OperationMode == RepositoryOperationMode.SinglePackage)
                throw new Exception("Cannot process repository in SinglePackage mode");
            var useFullName = Repo.Config.UseVersionedPackageFolders;

            if (useFullNameOverride.HasValue)
                useFullName = useFullNameOverride.Value;

            // Add this package, then add it's dependencies, and so on
            // The list should be unique based on package (name + version + branch)
            // If there is a conflict, the process should be aborted. A conflict can arise when one package needs a version locked on X, and another on Y
            // First resolve all dependencies
            // So that we can determine conflicting dependencies, remove double dependencies, etc.
            // Let the package Download itself, by providing it with sources (remotes)
            var packages = await GetDependencyTree(packageNames, noCheckout, useFullName).ConfigureAwait(false);

            StatusRepo.Reset(RepoStatus.Processing, packages.Count);

            await TrySyncObjects(noCheckout, skipWhenFileMatches, packages).ConfigureAwait(false);

            await
                CleanPackages(packages.Select(x => x.MetaData.ToSpecificVersion()).Distinct().ToArray(),
                    packages.Select(x => x.MetaData.Name).Distinct().ToArray()).ConfigureAwait(false);

            await Repo.SaveAsync().ConfigureAwait(false);

            return packages.ToArray();
        }

        public async Task<Package> DownloadPackage(string packageName, bool? useFullNameOverride = null) {
            var depInfo = ResolvePackageName(packageName);
            if (depInfo == null)
                throw new Exception("Could not resolve package " + packageName);

            var useFullName = Repo.Config.UseVersionedPackageFolders;
            var name = depInfo.GetFullName();

            if (useFullNameOverride.HasValue)
                useFullName = useFullNameOverride.Value;

            await GetAndAddPackage(depInfo).ConfigureAwait(false);
            var package = Package.Factory.Open(Repo,
                Repo.Config.OperationMode == RepositoryOperationMode.Default
                    ? WorkDir.GetChildDirectoryWithName(useFullName ? name : depInfo.Name)
                    : WorkDir,
                name);

            await
                UpdateMultiple(package.GetNeededObjects().ToArray(), new[] {package}, FindRemotesWithPackage(name))
                    .ConfigureAwait(false);
            return package;
        }

        async Task<List<Package>> GetDependencyTree(IEnumerable<Dependency> dependencies, bool noCheckout,
            bool useFullName) {
            var list = new List<string>();
            var list2 = new List<string>();
            var packages = new List<Package>();
            StatusRepo.Reset(RepoStatus.Resolving, dependencies.Count());

            foreach (var dep in dependencies) {
                var status = new Status(dep.GetFullName(), StatusRepo) {
                    Action = RepoStatus.Processing,
                    RealObject = "packages/" + dep + ".json"
                };
                await
                    ResolveDependencies(list, list2, packages, dep, useFullName, noCheckout)
                        .ConfigureAwait(false);
                status.EndOutput();
            }

            foreach (var package in packages)
                package.StatusRepo = StatusRepo;
            return packages;
        }

        async Task TrySyncObjects(bool noCheckout, bool skipWhenFileMatches, IReadOnlyCollection<Package> packages) {
            var syncedPackages = new List<Package>();
            try {
                await
                    ProcessSynqObjects(noCheckout, skipWhenFileMatches, packages, syncedPackages).ConfigureAwait(false);
            } finally {
                Repo.AddPackage(syncedPackages.Select(x => x.MetaData.GetFullName()).ToArray());
            }
        }

        async Task ProcessSynqObjects(bool noCheckout, bool skipWhenFileMatches, IReadOnlyCollection<Package> packages,
            ICollection<Package> syncedPackages) {
            var objects = new List<FileObjectMapping>();
            var allRemotes = new List<Uri>();

            foreach (var package in packages) {
                var name = package.MetaData.GetFullName();
                var remotes = FindRemotesWithPackage(name).ToArray();
                Console.WriteLine(String.Empty);
                if (!remotes.Any())
                    throw new NoSourceFoundException("No source found with " + name);
                allRemotes.AddRange(remotes);
                Repository.Log("Processing package: {0}", name);
                objects.AddRange(package.GetNeededObjects(skipWhenFileMatches));
            }

            var objectsToFetch = objects.DistinctBy(x => x.Checksum).ToArray();
            await
                UpdateMultiple(objectsToFetch, packages,
                    allRemotes.Distinct().ToArray()).ConfigureAwait(false);

            StatusRepo.Reset(RepoStatus.CheckOut, packages.Count);
            foreach (var package in packages) {
                syncedPackages.Add(package);
                if (noCheckout)
                    continue;
                if (Settings.GlobalWorkingPath != null)
                    package.SetWorkingPath(Settings.GlobalWorkingPath.ToString());
                Repository.Log("\nChecking out {0} into {1}, please be patient...",
                    package.MetaData.GetFullName(), package.WorkingPath);
                switch (Settings.CheckoutType) {
                case CheckoutType.NormalCheckout:
                    await package.CheckoutAsync().ConfigureAwait(false);
                    break;
                case CheckoutType.CheckoutWithoutRemoval:
                    await package.CheckoutWithoutRemovalAsync().ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }

            if (!noCheckout && skipWhenFileMatches) {
                Repo.DeleteObject(objectsToFetch.Select(x => new ObjectInfo(x.Checksum, x.Checksum)));
                // nasty using checksum for packed checksum or ? not needed here anyway??
            }
        }

        async Task<string[]> UpdateMultiple(IReadOnlyCollection<FileObjectMapping> objects,
            IReadOnlyCollection<Package> packages,
            IEnumerable<Uri> remotes) {
            if (!objects.Any()) {
                Repository.Log("No remote objects to resolve");
                return new string[0];
            }
            Repository.Log("Resolving {0} remote objects for {1} packages from {2} remotes, please be patient..",
                objects.Count(), packages.Count(), remotes.Count());

            var relObjects = objects.OrderByDescending(x => Tools.FileUtil.SizePrediction(x.FilePath))
                .Select(x => new FileFetchInfo(Repo.GetObjectSubPath(x), x.FilePath))
                .ToArray();

            StatusRepo.Reset(RepoStatus.Downloading, objects.Count());
            StatusRepo.ProcessSize(GetExistingObjects(objects, packages), Repo.ObjectsPath, GetPackedSize(packages));
            await Package.DownloadObjects(remotes, StatusRepo, relObjects, Repo.ObjectsPath).ConfigureAwait(false);
            Repo.ReAddObject(objects.Select(x => x.Checksum).ToArray());

            return relObjects.Select(x => x.FilePath).ToArray();
        }

        // Not accurate when there are duplicate objects between packages as they are de-duplicated. But not too important.
        static long GetPackedSize(IEnumerable<Package> packages) {
            return packages.Sum(x => x.MetaData.SizePacked);
        }

        IEnumerable<string> GetExistingObjects(IEnumerable<FileObjectMapping> objects, IEnumerable<Package> packages) {
            return
                packages.SelectMany(x => x.GetMetaDataFilesOrderedBySize().Select(y => y.Checksum))
                    .Except(objects.Select(x => x.Checksum))
                    .Select(x => Repo.GetObjectSubPath(x));
        }

        Task GetPackage(SpecificVersion package) {
            var packageName = package.GetFullName();
            var remotes = FindRemotesWithPackage(package.Name).ToArray();
            Repository.Log("\nFetching package: {0} ({1})", package.Name, packageName);
            if (!remotes.Any())
                throw new NoSourceFoundException("No source found with " + package.Name);

            return Repo.DownloadPackage(packageName, remotes, StatusRepo.CancelToken);
        }

        public async Task GetAndAddPackage(SpecificVersion package) {
            //Somewhere here we will need to specify what we will be doing with the package.
            await GetPackage(package).ConfigureAwait(false);
            Repo.AddPackage(package.GetFullName());
        }

        SpecificVersion ResolvePackageName(string packageName) {
            var depInfo = new Dependency(packageName);
            //var remotes = FindRemotesWithPackage(packageName);
            return Repo.Remotes.Select(x => x.Index.GetPackage(depInfo))
                .Where(x => x != null)
                .OrderBy(x => x.Version)
                .LastOrDefault();
        }

        IEnumerable<Uri> FindRemotesWithPackage(string packageName) {
            return
                Repo.Remotes.Where(x => x.Index.HasPackage(new Dependency(packageName)))
                    .SelectMany(x => x.GetRemotes()).Distinct();
        }

        async Task ResolveDependencies(List<string> list, List<string> list2, List<Package> packages,
            Dependency depInfo, bool useFullName = false, bool noCheckout = false) {
            var pn = depInfo.GetFullName();
            if (!noCheckout && list.Contains(depInfo.Name)) {
                Repository.Log("Conflicting package, not resolving {0}", pn);
                return;
            }
            var dp = ResolvePackageName(pn);
            if (dp == null)
                throw new NoSourceFoundException(String.Format("No source found with {0}", pn));
            var name = dp.GetFullName();
            if (list2.Contains(name)) {
                Repository.Log("Duplicate package, skipping {0}", name);
                return;
            }
            list2.Add(name);

            await GetAndAddPackage(dp).ConfigureAwait(false);

            var package = Package.Factory.Open(Repo,
                WorkDir.GetChildDirectoryWithName(useFullName ? name : depInfo.Name), name);
            list.Add(depInfo.Name);
            packages.Add(package);

            foreach (var dep in package.MetaData.GetDependencies())
                await ResolveDependencies(list, list2, packages, dep, useFullName, noCheckout);

            OrderPackageLast(list, packages, package);
        }

        static void OrderPackageLast(ICollection<string> list, ICollection<Package> packages, Package package) {
            list.Remove(package.MetaData.Name);
            list.Add(package.MetaData.Name);
            packages.Remove(package);
            packages.Add(package);
        }

        public Task<IEnumerable<string>> List(bool remote = false) {
            return Repo.ListPackages(remote);
        }

        public Task<IEnumerable<string>> List(string remote) {
            if (remote == null)
                return List();
            return remote == String.Empty ? List(true) : Repo.ListPackages(remote);
        }

        public void DeletePackages(IEnumerable<SpecificVersion> packages, bool inclWorkFiles = false,
            bool inclDependencies = false) {
            Repo.DeletePackage(packages, inclWorkFiles, inclDependencies);
            Repo.RemoveObsoleteObjects();
            Repo.Save();
        }

        public void DeletePackagesThatExist(IEnumerable<SpecificVersion> packages, bool inclWorkFiles = false,
            bool inclDependencies = false) {
            Repo.DeletePackage(packages.Where(x => Repo.HasPackage(x)), inclWorkFiles, inclDependencies);
            Repo.RemoveObsoleteObjects();
            Repo.Save();
        }

        public void DeletePackages(IEnumerable<string> packages, bool inclWorkFiles = false,
            bool inclDependencies = false) {
            DeletePackages(packages.Select(x => new SpecificVersion(x)), inclWorkFiles, inclDependencies);
        }

        public void DeletePackage(SpecificVersion package, bool inclWorkFiles = false,
            bool inclDependencies = false) {
            Repo.DeletePackage(package, inclWorkFiles, inclDependencies);
            Repo.RemoveObsoleteObjects();
            Repo.Save();
        }

        public void DeletePackageIfExists(Dependency package, bool inclWorkFiles = false,
            bool inclDependencies = false) {
            if (Repo.HasPackage(package))
                DeletePackage(Repo.Index.GetPackage(package), inclWorkFiles, inclDependencies);
        }

        public void DeletePackageIfExists(SpecificVersion package, bool inclWorkFiles = false,
            bool inclDependencies = false) {
            if (Repo.HasPackage(package))
                DeletePackage(package, inclWorkFiles, inclDependencies);
        }

        public void DeleteBundle(IEnumerable<string> collections, bool inclPackages = false,
            bool inclDependencies = false, bool inclPackageWorkFiles = false) {
            Repo.DeleteBundle(collections);
        }

        public IEnumerable<PackageMetaData> GetPackages(bool useFullName = false) {
            return
                Repo.GetPackagesListAsVersions()
                    .Select(GetMetaData);
        }

        public Dictionary<string, SpecificVersion[]> GetPackagesAsVersions(bool remote = false) {
            var packages = remote
                ? Repo.Remotes.SelectMany(x => x.Index.GetPackagesListAsVersions()).Distinct().ToArray()
                : Repo.GetPackagesListAsVersions();

            var dic = new Dictionary<string, List<SpecificVersion>>();
            foreach (var i in packages) {
                if (!dic.ContainsKey(i.Name))
                    dic[i.Name] = new List<SpecificVersion>();
                dic[i.Name].Add(i);
            }

            return dic.ToDictionary(x => x.Key, x => x.Value.Select(y => y).Reverse().ToArray());
        }

        public PackageMetaData GetMetaData(SpecificVersion arg) {
            return Package.Load(GetPackageMetadataPath(arg));
        }

        IAbsoluteFilePath GetPackageMetadataPath(SpecificVersion arg) {
            return Repo.PackagesPath.GetChildFileWithName(arg.GetFullName() + Repository.PackageFormat);
        }

        public Task<IReadOnlyCollection<SpecificVersion>> CleanPackages(
            IReadOnlyCollection<SpecificVersion> keepVersions, params string[] packages) {
            StatusRepo.Reset(RepoStatus.Cleaning, packages.Length);
            return Repo.CleanPackageAsync(packages, keepVersions);
        }

        public Task<IReadOnlyCollection<SpecificVersion>> CleanPackages(int limit,
            IReadOnlyCollection<SpecificVersion> keepVersions, params string[] packages) {
            StatusRepo.Reset(RepoStatus.Cleaning, packages.Length);
            return Repo.CleanPackageAsync(packages, keepVersions, limit);
        }
    }
}