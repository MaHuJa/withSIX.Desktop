// <copyright company="SIX Networks GmbH" file="GameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma
{
    public abstract class GameController : PropertyChangedBase
    {
        static readonly string[] premiumMirrors = Common.PremiumMirrors.Select(x => x + "/synq").ToArray();
        static readonly string[] defaultMirrors = Common.DefaultMirrors.Select(x => x + "/synq").ToArray();
        readonly ISupportModding _modding;
        BundleManager _bundleManager;
        bool _premium;
        RepositoryItem _repoItem;

        protected GameController(ISupportContent game) {
            Contract.Requires<ArgumentNullException>(game != null);
            Game = game;
            _modding = game as ISupportModding;
        }

        public ISupportContent Game { get; }
        public BundleManager BundleManager
        {
            get { return _bundleManager; }
            private set { SetProperty(ref _bundleManager, value); }
        }
        public PackageManager PackageManager
        {
            get { return BundleManager.PackageManager; }
        }
        protected abstract IEnumerable<KeyValuePair<Guid, Uri[]>> DefaultRemotes { get; }
        protected abstract IEnumerable<KeyValuePair<Guid, Uri[]>> PremiumRemotes { get; }

        public async Task AddPremium() {
            _premium = true;
            var bm = BundleManager;
            if (bm == null)
                return;

            var repo = bm.Repo;
            foreach (var remote in PremiumRemotes)
                repo.ReplaceRemote(remote.Key, remote.Value);
            await repo.SaveConfigAsync().ConfigureAwait(false);
            await bm.PackageManager.UpdateRemotes().ConfigureAwait(false);
        }

        public Task RemovePremium() {
            _premium = false;
            var bm = BundleManager;
            if (bm == null)
                return Task.FromResult(0);
            var repo = bm.Repo;
            foreach (var remote in PremiumRemotes)
                repo.RemoveRemote(remote.Key, remote.Value);
            return repo.SaveConfigAsync();
        }

        public async Task UpdateBundleManager() {
            var contentPaths = Game.PrimaryContentPath;
            if (!contentPaths.IsValid) {
                BundleManager = null;
                _repoItem = null;
                return;
            }

            try {
                BundleManager = await CreateRepoIfNotExistent(contentPaths, DefaultRemotes).ConfigureAwait(false);
                _repoItem = CreateRepoItem();
                // TODO: Cleanup this ultra mess....
                _repoItem.Handler.BundleManager = BundleManager;
                _repoItem.WorkingDirectory = BundleManager.WorkDir;
                _repoItem.RepositoryDirectory = BundleManager.Repo.RootPath;
                await HandlePremium().ConfigureAwait(false);
            } catch (IOException ex) {
                MainLog.Logger.FormattedWarnException(ex, "Error during setup of bundle manager..");
            }
        }

        Task HandlePremium() {
            return _premium ? AddPremium() : RemovePremium();
        }

        public void Update() {
            _repoItem = CreateRepoItem();
        }

        public PackageItem FindPackage(IMod mod) {
            return FindPackage(mod.Name);
        }

        public PackageItem FindPackage(Mission mission) {
            return FindPackage(mission.PackageName);
        }

        public virtual Task AdditionalHandleModPreRequisites() {
            return TaskExt.Default;
        }

        public PackageItem FindPackage(string searchName) {
            var packages = _repoItem.Packages.Items;
            return
                packages.FirstOrDefault(
                    x => searchName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        protected static KeyValuePair<Guid, Uri[]> GetSet(Guid repo) {
            return new KeyValuePair<Guid, Uri[]>(repo,
                defaultMirrors.Select(x => Tools.Transfer.JoinUri(new Uri(x), repo)).ToArray());
        }

        protected static KeyValuePair<Guid, Uri[]> GetPremiumSet(Guid repo) {
            return new KeyValuePair<Guid, Uri[]>(repo,
                premiumMirrors.Concat(defaultMirrors)
                    .Select(x => Tools.Transfer.JoinUri(new Uri(x), repo + "/p"))
                    .ToArray());
        }

        RepositoryItem CreateRepoItem() {
            return new RepositoryItem(null, null, BundleManager.WorkDir, BundleManager.Repo.RootPath,
                CreateHandler());
        }

        RepositoryHandler CreateHandler() {
            return new RepositoryHandler {BundleManager = BundleManager, Remote = true};
        }

        protected virtual Task<BundleManager> CreateRepoIfNotExistent(ContentPaths modPaths,
            IEnumerable<KeyValuePair<Guid, Uri[]>> remotes) {
            var modPath = modPaths.Path;
            var synqBasePath = modPaths.RepositoryPath;
            var synqPath = synqBasePath.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory);

            return RepositoryHandler.GetBundleManager(synqPath, modPath, remotes);
        }

        public bool Supports(IMod mod) {
            return _modding != null && mod.GameMatch(_modding);
        }
    }
}