// <copyright company="SIX Networks GmbH" file="BundleManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Packages
{
    public class BundleManager
    {
        static readonly TimeSpan syncInterval = TimeSpan.FromMinutes(1);
        DateTime _lastSync;

        public BundleManager(PackageManager packageManager) {
            Contract.Requires<ArgumentNullException>(packageManager != null);
            PackageManager = packageManager;
        }

        public Repository Repo
        {
            get { return PackageManager.Repo; }
        }
        public IAbsoluteDirectoryPath WorkDir
        {
            get { return PackageManager.WorkDir; }
        }
        public PackageManager PackageManager { get; }

        [Obsolete("Workaround for classic callers")]
        public static async Task<BundleManager> Create(Repository repo, IAbsoluteDirectoryPath workDir,
            bool createWhenNotExisting = false,
            string remote = null) {
            var packageManager =
                await PackageManager.Create(repo, workDir, createWhenNotExisting, remote).ConfigureAwait(false);
            return new BundleManager(packageManager);
        }

        public async Task<Package[]> Process(string bundleName, bool includeOptional = false,
            BundleScope scope = BundleScope.All, bool? useVersionedPackageFolders = null,
            bool noCheckout = false, bool skipWhenLocalMatch = false) {
            var bundle = await GetCol(bundleName).ConfigureAwait(false);
            return
                await
                    Process(bundle, includeOptional, scope, useVersionedPackageFolders, noCheckout,
                        skipWhenLocalMatch).ConfigureAwait(false);
        }

        public async Task<Package[]> Checkout(Bundle bundle, bool includeOptional = false,
            BundleScope scope = BundleScope.All, bool? useVersionedPackageFolders = null) {
            Repository.Log("Processing bundle: {0}", bundle.GetFullName());
            var packages = await GetDependencyTree(bundle, includeOptional, scope, false).ConfigureAwait(false);
            // TODO: Async
            return
                PackageManager.Checkout(
                    packages.Reverse().Select(x => new Dependency(x.Key, x.Value).GetFullName()).ToArray(),
                    useVersionedPackageFolders);
        }

        public async Task<Package[]> Process(Bundle bundle, bool includeOptional = false,
            BundleScope scope = BundleScope.All, bool? useVersionedPackageFolders = null,
            bool noCheckout = false, bool skipWhenLocalMatch = false) {
            Repository.Log("Processing bundle: {0}", bundle.GetFullName());
            var packages = await GetDependencyTree(bundle, includeOptional, scope).ConfigureAwait(false);

            return
                await
                    PackageManager.ProcessPackages(
                        packages.Reverse().Select(x => new SpecificVersion(x.Key, x.Value)).ToArray(),
                        useVersionedPackageFolders, noCheckout, skipWhenLocalMatch).ConfigureAwait(false);
        }

        async Task<Dictionary<string, string>> GetDependencyTree(Bundle bundle, bool includeOptional, BundleScope scope,
            bool remote = true) {
            var packages = new Dictionary<string, string>();
            var deps = new List<string>();
            PackageManager.StatusRepo.Reset(RepoStatus.Resolving, 1);
            await ResolveDependencies(bundle, packages, deps, scope, includeOptional, remote).ConfigureAwait(false);
            return packages;
        }

        async Task ResolveDependencies(Bundle bundle, Dictionary<string, string> packages,
            List<string> deps, BundleScope scope = BundleScope.All, bool includeOptional = false,
            bool remote = true) {
            await AddDependencies(bundle, packages, deps, scope, includeOptional, remote).ConfigureAwait(false);
            AddOwn(bundle, packages, scope, includeOptional);
        }

        static void AddOwn(Bundle bundle, Dictionary<string, string> packages, BundleScope scope, bool includeOptional) {
            foreach (var p in bundle.GetAllPackages(scope, includeOptional))
                packages[p.Key] = p.Value;
        }

        async Task AddDependencies(Bundle bundle, Dictionary<string, string> packages, List<string> deps,
            BundleScope scope, bool includeOptional, bool remote) {
            foreach (var dep in bundle.Dependencies) {
                var d = new Dependency(dep.Key, dep.Value);
                if (deps.Contains(dep.Key)) {
                    Repository.Log("Conflicting bundle dependency {0}, skipping", dep.Key);
                    continue;
                }
                deps.Add(dep.Key);
                var depCol = await GetCol(d.GetFullName(), remote).ConfigureAwait(false);
                await ResolveDependencies(depCol, packages, deps, scope, includeOptional).ConfigureAwait(false);
            }
        }

        async Task<Bundle> GetCol(string bundleName, bool remote = true) {
            var depInfo = ResolveBundleName(bundleName);
            if (depInfo == null)
                throw new NotFoundException("Could not find " + bundleName);
            if (remote)
                await GetAndAddBundle(depInfo).ConfigureAwait(false);
            return GetMetaData(depInfo);
        }

        public Task GetBundle(string bundleName) {
            return GetBundle(new SpecificVersion(bundleName));
        }

        Task GetBundle(SpecificVersion bundle) {
            var bundleName = bundle.GetFullName();
            var remotes = FindRemotesWithBundle(bundle.Name).ToArray();
            Repository.Log("Fetching bundle: {0} ({1})", bundle.Name, bundleName);
            if (!remotes.Any())
                throw new NoSourceFoundException("No source found with " + bundle.Name);

            return Repo.DownloadBundle(bundleName, remotes, PackageManager.StatusRepo.CancelToken);
        }

        public async Task GetAndAddBundle(SpecificVersion bundle) {
            await GetBundle(bundle).ConfigureAwait(false);
            Repo.AddBundle(bundle.GetFullName());
        }

        public Bundle GetMetaData(SpecificVersion arg) {
            return
                Bundle.Factory.Open(Repo.BundlesPath.GetChildFileWithName(arg.GetFullName() + Repository.PackageFormat));
        }

        SpecificVersion ResolveBundleName(string bundleName) {
            //var remotes = FindRemotesWithBundle(bundleName);
            return
                Repo.Remotes.Select(x => x.Index.GetBundle(new SpecificVersion(bundleName)))
                    .Where(x => x != null)
                    .OrderBy(x => x.Version)
                    .LastOrDefault();
        }

        IEnumerable<Uri> FindRemotesWithBundle(string bundleName) {
            return
                Repo.Remotes.Where(x => x.Index.HasBundle(new SpecificVersion(bundleName)))
                    .SelectMany(x => x.GetRemotes()).Distinct();
        }

        IAbsoluteFilePath GetPackageMetadataPath(Dependency x) {
            return Repo.PackagesPath.GetChildFileWithName(x.GetFullName() + Repository.PackageFormat);
        }

        public IEnumerable<KeyValuePair<string, SpecificVersion[]>> GetBundlesAsVersions(bool remote = false) {
            var bundles = remote
                ? Repo.Remotes.SelectMany(x => x.Index.GetBundlesListAsVersions()).ToArray()
                : Repo.GetBundlesListAsVersions();

            var dic = new Dictionary<string, List<SpecificVersion>>();
            foreach (var i in bundles) {
                if (!dic.ContainsKey(i.Name))
                    dic[i.Name] = new List<SpecificVersion>();
                dic[i.Name].Add(i);
            }

            return dic.ToDictionary(x => x.Key, x => x.Value.Select(y => y).Reverse().ToArray());
        }

        public async Task UpdateRemotesConditional() {
            if (Tools.Generic.LongerAgoThan(_lastSync, syncInterval))
                await UpdateRemotes().ConfigureAwait(false);
        }

        async Task UpdateRemotes() {
            await PackageManager.UpdateRemotes().ConfigureAwait(false);
            _lastSync = Tools.Generic.GetCurrentUtcDateTime;
        }
    }

    public class NoSourceFoundException : Exception
    {
        public NoSourceFoundException(string message) : base(message) {}
    }
}