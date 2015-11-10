// <copyright company="SIX Networks GmbH" file="SynqInstallerSession.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Infra.Api.WebApi;
using SN.withSIX.Sync.Core;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Transfer;
using YamlDotNet.Core;
using CheckoutType = SN.withSIX.Mini.Core.Games.CheckoutType;
using Repository = SN.withSIX.Sync.Core.Repositories.Repository;

namespace SN.withSIX.Mini.Presentation.Wpf.Services
{
    class SynqInstallerSession : IInstallerSession
    {
        readonly IInstallContentAction<IInstallableContent> _action;
        readonly IContentEngine _contentEngine;
        readonly ICollection<Dependency> _installedPackages = new List<Dependency>();
        readonly bool _isPremium;
        readonly Status _status;
        readonly Func<double, double, Task> _statusChange;
        readonly IToolsInstaller _toolsInstaller;
        PackageManager _pm;
        IReadOnlyCollection<CustomRepo> _repositories;
        Repository _repository;

        public SynqInstallerSession(IInstallContentAction<IInstallableContent> action, IToolsInstaller toolsInstaller,
            bool isPremium, Func<double, double, Task> statusChange, IContentEngine contentEngine) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (toolsInstaller == null)
                throw new ArgumentNullException(nameof(toolsInstaller));
            if (statusChange == null)
                throw new ArgumentNullException(nameof(statusChange));
            _action = action;
            _toolsInstaller = toolsInstaller;
            _isPremium = isPremium;
            _statusChange = statusChange;
            _contentEngine = contentEngine;
            _status = new Status(_action.Content.Count);
        }

        public async Task Install(IEnumerable<IContentSpec<IPackagedContent>> content) {
            // Let's prevent us from reinstalling the same packages over and over (even if it's fairly quick to check the files..)
            // TODO: Consider if instead we want to Concat up the Content, for a later all-at-once execution instead...
            // but then it would be bad to have an Install command on Content class, but not actually finish installing before the method ends
            // so instead we should have a Query on content then.

            var toInstall =
                content.ToDictionary(x => x.Content, x => {
                    var fullyQualifiedName = x.Content.PackageName.ToLower();
                    if (x.Constraint != null)
                        fullyQualifiedName += "-" + x.Constraint;
                    return new Dependency(fullyQualifiedName);
                })
                    .Where(x => !_installedPackages.Contains(x.Value))
                    .ToDictionary(x => x.Key, x => x.Value);

            var initialState = ItemState.Installing;
            foreach (var c in toInstall.Select(x => x.Key))
                await new ContentStatusChanged(c, initialState).RaiseEvent().ConfigureAwait(false);
            foreach (var c in _action.Content)
                await new ContentStatusChanged(c.Content, initialState).RaiseEvent().ConfigureAwait(false);

            var finalState = ItemState.UpdateAvailable; // TODO: Current state
            try {
                await TryInstallContent(toInstall).ConfigureAwait(false);
                finalState = ItemState.Uptodate;
            } finally {
                foreach (var c in toInstall.Select(x => x.Key))
                    await new ContentStatusChanged(c, finalState).RaiseEvent().ConfigureAwait(false);
                foreach (var c in _action.Content)
                    await new ContentStatusChanged(c.Content, finalState).RaiseEvent().ConfigureAwait(false);

            }
        }

        public async Task Synchronize() {
            // TODO: Install Tools as part of the Setup process instead? Also include in Setup package..
            await InstallToolsIfNeeded().ConfigureAwait(false);

            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(_action.Game.InstalledState.Directory);
            if (_action.GlobalWorkingPath != null)
                Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(_action.GlobalWorkingPath);
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(_action.Paths.Path);
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(_action.Paths.RepositoryPath);

            using (_repository = new Repository(GetRepositoryPath(), true)) {
                SetupPackageManager();
                using (_pm.StatusRepo = new StatusRepo())
                using (new RepoWatcher(_pm.StatusRepo))
                using (new StatusRepoMonitor(_pm.StatusRepo, (Func<double, double, Task>) StatusChange)) {
                    await UpdateRemotes().ConfigureAwait(false);
                    await InstallContent().ConfigureAwait(false);
                }
            }
        }

        Task InstallContent() {
            // TODO: Rethink our strategy here. We convert it to a temp collection so that we can install all the content at once.
            // this is because we relinquish control to the content.Install method, and that will build the dependency tree and call the installer actions with it..
            return InstallContent(ConvertToTemporaryCollectionSpec());
        }

        InstallContentSpec ConvertToTemporaryCollectionSpec() {
            return new InstallContentSpec(ConvertToTemporaryCollection());
        }

        LocalCollection ConvertToTemporaryCollection() {
            return new LocalCollection(_action.Game.Id, "$$temp",
                _action.Content.Select(x => new ContentSpec((Content) x.Content, x.Constraint)).ToList());
        }

        async Task UpdateRemotes() {
            await
                RepositoryHandler.ReplaceRemotes(GetRemotes(_action.RemoteInfo), _repository)
                    .ConfigureAwait(false);
            await _pm.UpdateRemotes().ConfigureAwait(false);
            await HandleRepositories().ConfigureAwait(false);
            _pm.StatusRepo.Reset(RepoStatus.Processing, 0);
        }

        public void Abort() {
            _pm?.StatusRepo.Abort();
        }

        public void RunCE(IPackagedContent content) {
            if (_contentEngine.ModHasScript(content.Id)) {
                _contentEngine.LoadModS(new ContentEngineContent(content.Id, content.Id, true,
                    _action.Paths.Path.GetChildDirectoryWithName(content.PackageName),
                    content.GameId)).processMod();
            }
        }

        async Task TryInstallContent(IDictionary<IPackagedContent, Dependency> toInstall) {
            var repoContent =
                toInstall.Where(x => _repositories.Any(r => r.HasMod(x.Value.Name)))
                    .ToDictionary(x => x.Key, x => x.Value);
            var packageContent = toInstall.Except(repoContent).ToDictionary(x => x.Key, x => x.Value);
            // TODO: Do we actually process dependencies specified on custom repo mods? :-)
            // TODO: here we should only installPackages packagesContent that actually exists on the network, otherwise consider it as a local content??
            await InstallPackages(packageContent).ConfigureAwait(false);
            await InstallRepoContent(repoContent.Values.ToArray()).ConfigureAwait(false);
            await PerformPostInstallTasks(toInstall.Keys).ConfigureAwait(false);
        }

        async Task PerformPostInstallTasks(IEnumerable<IPackagedContent> toInstall) {
            foreach (var c in toInstall)
                await c.PostInstall(this, _action.CancelToken).ConfigureAwait(false);
        }

        async Task InstallToolsIfNeeded() {
            if (await _toolsInstaller.ConfirmToolsInstalled(true).ConfigureAwait(false))
                return;
            using (var repo = new StatusRepo {Action = RepoStatus.Downloading})
                //using (new RepoWatcher(repo))
                //using (new StatusRepoMonitor(repo, (Func<double, double, Task>)StatusChange))
                await _toolsInstaller.DownloadAndInstallTools(repo).ConfigureAwait(false);
        }

        IAbsoluteDirectoryPath GetRepositoryPath() {
            return _action.Paths.RepositoryPath.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory);
        }

        async Task InstallRepoContent(IReadOnlyCollection<Dependency> repoContent) {
            var packPath = GetRepositoryPath().GetChildDirectoryWithName("legacy");
            foreach (var c in repoContent) {
                var repo = _repositories.First(x => x.HasMod(c.Name));
                await repo.GetMod(c.Name, _action.Paths.Path, packPath, _pm.StatusRepo).ConfigureAwait(false);
                // TODO: The versiondata has to be included as to latest version etc
                _action.Game.ContentInstalled(Tuple.Create(c.Name, c.VersionData));
            }
            _installedPackages.AddRange(repoContent);
        }

        async Task InstallPackages(IDictionary<IPackagedContent, Dependency> packages) {
            var remotePackageIndex = _pm.GetPackagesAsVersions(true);
            var localPackageIndex = _pm.GetPackagesAsVersions();
            // TODO: Only handle content that is not yet on the right version (SynqInfo/RepositoryYml etc) Unless forced / diagnose?
            // The problem with GTA is that we wipe the folders beforehand.. Something we would solve with new mod folder approach
            var toInstallPackages = packages.Where(x => {
                var syncInfo = GetInstalledInfo(x);
                var pi = remotePackageIndex.ContainsKey(x.Value.Name) ? remotePackageIndex[x.Value.Name] : null;
                // syncINfo = null: new download, VersionData not equal: new update
                return syncInfo == null ||
                       !syncInfo.VersionData.Equals(x.Value.VersionData ??
                                                    pi?.OrderByDescending(v => v).First().VersionData);
            }).ToDictionary(x => x.Key, x => x.Value);

            foreach (var p in toInstallPackages) {
                if (localPackageIndex.ContainsKey(p.Value.Name)) {
                    if (
                        localPackageIndex[p.Value.Name].Contains(
                            remotePackageIndex[p.Value.Name].OrderByDescending(v => v).First())) {
                        // already have version
                    } else {
                        HandleUpdate(p.Key);
                        // is Update
                    }
                } else {
                    HandleInstall(p.Key);
                    // is Install
                }
            }
            await _pm.ProcessPackages(toInstallPackages.Values).ConfigureAwait(false);
            _action.Game.ContentInstalled(packages.Values.Select(x => Tuple.Create(x.Name, x.VersionData)));
            _installedPackages.AddRange(packages.Values);
        }

        SpecificVersion GetInstalledInfo(KeyValuePair<IPackagedContent, Dependency> i) {
            switch (_action.CheckoutType) {
            case CheckoutType.NormalCheckout:
                return Package.ReadSynqInfoFile(_pm.WorkDir.GetChildDirectoryWithName(i.Value.Name));
            case CheckoutType.CheckoutWithoutRemoval: {
                // TODO: Cache per GlobalWorkingPath ??
                return Package.GetInstalledPackages(_pm.WorkDir).FirstOrDefault(x => x.Name.Equals(i.Value.Name));
            }
            }
            throw new NotSupportedException("Unknown " + _action.CheckoutType);
        }

        void HandleInstall(IPackagedContent key) {
            if (key is ModNetworkContent)
                _action.Status.Mods.Install.Add(key.Id);
            else if (key is MissionNetworkContent)
                _action.Status.Missions.Install.Add(key.Id);
            // TODO!
            else if (key is NetworkCollection)
                _action.Status.Collections.Install.Add(key.Id);
        }

        void HandleUpdate(IPackagedContent key) {
            if (key is ModNetworkContent)
                _action.Status.Mods.Update.Add(key.Id);
            else if (key is MissionNetworkContent)
                _action.Status.Missions.Update.Add(key.Id);
            // TODO!
            else if (key is NetworkCollection)
                _action.Status.Collections.Update.Add(key.Id);
        }

        async Task HandleRepositories() {
            _repositories =
                _action.Content.Select(x => x.Content).OfType<IHaveRepositories>()
                    .SelectMany(x => x.Repositories.Select(r => new CustomRepo(CustomRepo.GetRepoUri(new Uri(r))))).ToArray();
            foreach (var r in _repositories)
                await r.Load(SyncEvilGlobal.StringDownloader).ConfigureAwait(false);
        }


        void SetupPackageManager() {
            _pm = new PackageManager(_repository, _action.Paths.Path, true);
            Sync.Core.Packages.CheckoutType ct;
            if (!Enum.TryParse(_action.CheckoutType.ToString(), out ct))
                throw new InvalidOperationException("Unsupported checkout type");
            _pm.Settings.CheckoutType = ct;
            _pm.Settings.GlobalWorkingPath = _action.GlobalWorkingPath;
        }

        IEnumerable<KeyValuePair<Guid, Uri[]>> GetRemotes(RemoteInfoAttribute synqRemoteInfo) {
            return _isPremium ? synqRemoteInfo.PremiumRemotes : synqRemoteInfo.DefaultRemotes;
        }

        async Task InstallContent(IContentSpec<IInstallableContent> c) {
            await c.Content.Install(this, _action.CancelToken, c.Constraint).ConfigureAwait(false);
            //await StatusChange().ConfigureAwait(false); // This is already achieved by the statusrepo monitor eh??
        }

        Task StatusChange(double progress, double speed) {
            return _statusChange(_status.Completed.CalculateProgress(_status.Count, progress), speed);
        }

        Task StatusChangeIncrement() {
            _status.Increment();
            return _statusChange(_status.Completed.CalculateProgress(_status.Count), 0);
        }

        class Status
        {
            public Status(int count) {
                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "below 0");
                Count = count;
            }

            public int Count { get; }
            public int Completed { get; private set; }

            public void Increment(int count = 1) {
                if (Count > Completed + count)
                    throw new InvalidOperationException("Can't complete more items than Count");
                Completed += count;
            }
        }

        class RepoWatcher : IDisposable
        {
            const int TimerTime = 150;
            bool _disposed;
            TimerWithElapsedCancellation _timer;

            public RepoWatcher(StatusRepo repo) {
                _timer = new TimerWithElapsedCancellation(TimerTime, () => {
                    repo.UpdateTotals();
                    return true;
                });
            }

            public void Dispose() {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing) {
                if (!disposing)
                    return;

                if (_disposed)
                    return;
                _timer.Dispose();
                _timer = null;

                _disposed = true;
            }
        }

        // A cheap variant of ReactiveUI's this.WhenAny...
        // TODO: Choose if we will make all Core Domain free of PropertyChanged / RXUI (and use custom events etc)
        // or if we will give in ;-)
        class StatusRepoMonitor : IDisposable
        {
            readonly Action<double, double> _progressCallback;
            readonly StatusRepo _repo;

            internal StatusRepoMonitor(StatusRepo repo, Action<double, double> progressCallback) {
                _repo = repo;
                _progressCallback = progressCallback;
                _repo.PropertyChanged += RepoOnPropertyChanged;
            }

            internal StatusRepoMonitor(StatusRepo repo, Func<double, double, Task> progressCallback) {
                _repo = repo;
                _progressCallback = (p, s) => progressCallback(p, s).Wait(); // pff
                _repo.PropertyChanged += RepoOnPropertyChanged;
            }

            public void Dispose() {
                _repo.PropertyChanged -= RepoOnPropertyChanged;
            }

            void RepoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
                if (propertyChangedEventArgs.PropertyName == "Info")
                    _progressCallback(_repo.Info.Progress, _repo.Info.Speed);
            }
        }
    }
}