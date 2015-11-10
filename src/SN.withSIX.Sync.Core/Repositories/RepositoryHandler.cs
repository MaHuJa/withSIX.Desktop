// <copyright company="SIX Networks GmbH" file="RepositoryHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Packages;

namespace SN.withSIX.Sync.Core.Repositories
{
    public class RepositoryHandler
    {
        static readonly ConcurrentDictionary<IAbsoluteDirectoryPath, Repository> repositories =
            new ConcurrentDictionary<IAbsoluteDirectoryPath, Repository>();
        static readonly ConcurrentDictionary<Tuple<IAbsoluteDirectoryPath, IAbsoluteDirectoryPath>, BundleManager>
            bundleManagers =
                new ConcurrentDictionary<Tuple<IAbsoluteDirectoryPath, IAbsoluteDirectoryPath>, BundleManager>();
        static readonly SemaphoreSlim bmLock = new SemaphoreSlim(1);
        public bool Remote;
        public Repository Repository
        {
            get { return BundleManager.Repo; }
        }
        public PackageManager PackageManager
        {
            get { return BundleManager.PackageManager; }
        }
        public BundleManager BundleManager { get; set; }

        public async Task UpdateRepository(IAbsoluteDirectoryPath workDir, IAbsoluteDirectoryPath repoDir) {
            BundleManager = workDir == null ? null : await GetBundleManager(repoDir, workDir).ConfigureAwait(false);
        }

        public static async Task<BundleManager> GetBundleManager(IAbsoluteDirectoryPath synqPath,
            IAbsoluteDirectoryPath workPath,
            IEnumerable<KeyValuePair<Guid, Uri[]>> remotes = null) {
            var repo = GetRepo(synqPath);

            if (remotes != null)
                await ReplaceRemotes(remotes, repo).ConfigureAwait(false);

            return await GetCM(synqPath, workPath, repo).ConfigureAwait(false);
        }

        public static async Task AddRemotes(IEnumerable<KeyValuePair<Guid, Uri[]>> remotes, Repository repo) {
            repo.AddRemotes(remotes);
            await repo.SaveConfigAsync().ConfigureAwait(false);
        }

        public static async Task ReplaceRemotes(IEnumerable<KeyValuePair<Guid, Uri[]>> remotes, Repository repo) {
            repo.ClearRemotes();
            repo.AddRemotes(remotes);
            await repo.SaveConfigAsync().ConfigureAwait(false);
        }

        static async Task<BundleManager> GetCM(IAbsoluteDirectoryPath synqPath, IAbsoluteDirectoryPath workPath,
            Repository repo) {
            var key = Tuple.Create(synqPath, workPath);
            // We lock here because we don't want the value to ever factor multiple times
            await bmLock.WaitAsync().ConfigureAwait(false);
            try {
                if (bundleManagers.ContainsKey(key))
                    return bundleManagers[key];
                var bm = await BundleManager.Create(repo, workPath, true).ConfigureAwait(false);
                return bundleManagers[key] = bm;
            } finally {
                bmLock.Release();
            }
        }

        static Repository GetRepo(IAbsoluteDirectoryPath synqPath) {
            // We lock here because we don't want the value to ever factor multiple times
            // also using lock because of when not exist doesnt lock...
            lock (repositories) {
                if (synqPath.Exists && !RepositoryFactory.IsEmpty(synqPath))
                    return repositories.GetOrAdd(synqPath, x => OpenOrCreateRepo(synqPath));
                return (repositories[synqPath] = OpenOrCreateRepo(synqPath));
            }
        }

        static Repository OpenOrCreateRepo(IAbsoluteDirectoryPath synqPath) {
            return synqPath.Exists && !RepositoryFactory.IsEmpty(synqPath) ? Open(synqPath) : Create(synqPath);
            //Tools.Dialog.CheckUac(synqPath); // TODO
        }

        static Repository Create(IAbsoluteDirectoryPath synqPath) {
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(synqPath);
            return Repository.Factory.Init(synqPath);
        }

        static Repository Open(IAbsoluteDirectoryPath synqPath) {
            // Make sure ACL's are ok
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(synqPath);
            return new Repository(synqPath);
        }
    }
}