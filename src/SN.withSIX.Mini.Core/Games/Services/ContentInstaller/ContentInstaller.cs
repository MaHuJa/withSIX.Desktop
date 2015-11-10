// <copyright company="SIX Networks GmbH" file="ContentInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Mini.Core.Games.Services.ContentInstaller
{
    // TODO: Deal with Path Access permissions (Elevate and set access bits for the user etc? or elevate self?)

    // TODO: cleanup installed mods, or do we do that at the Launch instead ?.. .. would probably need more control by the game then?
    // Or we eventually create an installation service per game, and a factory, like we do with the game launcher

    // TODO: Should it be the domain that enforces the single action (per game) lock, or rather the app?? Same goes for the status reporting..

    // TODO: We actually would like to support multi-installation capabilities per game, but let's start without?
    // Currently however we create a Repo object per session and therefore are locked..

    public static class StatusExtensions
    {
        public static bool IsEmpty(this InstallStatusOverview overview) {
            return overview.Collections.IsEmpty() && overview.Mods.IsEmpty() && overview.Missions.IsEmpty();
        }

        public static bool IsEmpty(this InstallStatus status) {
            return !status.Install.Any() && !status.Uninstall.Any() && !status.Update.Any();
        }
    }

    public class ContentInstaller : IContentInstallationService, IDomainService
    {
        public static readonly string SyncBackupDir = @".sync-backup";
        readonly ContentCleaner _cleaner;
        readonly Func<IDomainEvent, Task> _eventRaiser;
        readonly GameLocker _gameLocker;
        readonly IINstallerSessionFactory _sessionFactory;

        public ContentInstaller(Func<IDomainEvent, Task> eventRaiser, IINstallerSessionFactory sessionFactory) {
            _eventRaiser = eventRaiser;
            _sessionFactory = sessionFactory;
            _gameLocker = new GameLocker(eventRaiser);
            _cleaner = new ContentCleaner();
        }

        public void Abort(Guid gameId) {
            _gameLocker.Cancel(gameId);
        }

        public void Abort() {
            _gameLocker.Cancel();
        }

        public async Task Uninstall(IUninstallContentAction2<IUninstallableContent> action) {
            await _gameLocker.ConfirmLock(action.Game.Id).ConfigureAwait(false);
            try {
                await TryUninstall(action).ConfigureAwait(false);
            } finally {
                await _gameLocker.ReleaseLock(action.Game.Id).ConfigureAwait(false);
            }
        }

        public async Task Install(IInstallContentAction<IInstallableContent> action) {
            await _gameLocker.ConfirmLock(action.Game.Id).ConfigureAwait(false);
            try {
                await TryInstall(action).ConfigureAwait(false);
            } finally {
                await _gameLocker.ReleaseLock(action.Game.Id).ConfigureAwait(false);
            }
        }

        async Task TryUninstall(IUninstallContentAction2<IUninstallableContent> action) {
            var session = _sessionFactory.CreateUninstaller(action);
            foreach (var c in action.Content)
                await c.Content.Uninstall(session).ConfigureAwait(false);
            if (!action.Status.IsEmpty())
                await PostInstallStatusOverview(action.Status).ConfigureAwait(false);
        }

        async Task TryInstall(IInstallContentAction<IInstallableContent> action) {
            if (action.Cleaning.ShouldClean)
                await Clean(action).ConfigureAwait(false);
            await Synchronize(action).ConfigureAwait(false);
        }

        Task Clean(IInstallContentAction<IInstallableContent> action) {
            // TODO: .Concat exclusions based on Package info from each Content, so that we don't reinstall the desired content?
            return _cleaner.CleanAsync(action.Paths.Path, action.Cleaning.Exclusions
                .Concat(
                    new IRelativePath[]
                    {@".\.synq".ToRelativeDirectoryPath(), @".\.sync-backup".ToRelativeDirectoryPath()})
                .ToArray(), action.Cleaning.FileTypes, action.Paths.Path.GetChildDirectoryWithName(SyncBackupDir));
        }

        async Task Synchronize(IInstallContentAction<IInstallableContent> action) {
            await StatusChange(Status.Synchronizing).ConfigureAwait(false);
            try {
                await TrySynchronize(action).ConfigureAwait(false);
            } finally {
                await StatusChange(Status.Synchronized).ConfigureAwait(false);
            }
        }

        async Task TrySynchronize(IInstallContentAction<IInstallableContent> action) {
            await CreateSession(action).Synchronize().ConfigureAwait(false);
            if (!action.Status.IsEmpty())
                await PostInstallStatusOverview(action.Status).ConfigureAwait(false);
        }

        IInstallerSession CreateSession(
            IInstallContentAction<IInstallableContent> action) {
            var session = _sessionFactory.Create(action, (p, s) => StatusChange(Status.Synchronizing, p, s));
            if (action.CancelToken != default(CancellationToken))
                action.CancelToken.Register(session.Abort);
            _gameLocker.RegisterCancel(action.Game.Id, session.Abort);
            return session;
        }

        // TODO: Post this to an async Queue that processes and retries in the background instead? (and perhaps merges queued items etc??)
        // And make the errors non fatal..
        static Task<HttpResponseMessage> PostInstallStatusOverview(InstallStatusOverview statusOverview) {
            return Tools.Transfer.PostJson(statusOverview, new Uri(CommonUrls.SocialApiUrl, "/api/stats"));
        }

        Task StatusChange(Status status, double progress = 0, double speed = 0) {
            return _eventRaiser(new StatusChanged(status, progress, speed));
        }

        // NOTE: Stateful service!
        class GameLocker
        {
            readonly Func<IDomainEvent, Task> _eventRaiser;
            readonly IDictionary<Guid, CancellationTokenSource> _list = new Dictionary<Guid, CancellationTokenSource>();
            readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

            public GameLocker(Func<IDomainEvent, Task> eventRaiser) {
                _eventRaiser = eventRaiser;
            }

            public CancellationTokenRegistration RegisterCancel(Guid gameId, Action cancelAction) {
                _lock.Wait();
                try {
                    return _list[gameId].Token.Register(cancelAction);
                } finally {
                    _lock.Release();
                }
            }

            public void Cancel(Guid gameId) {
                _lock.Wait();
                try {
                    _list[gameId].Cancel();
                } finally {
                    _lock.Release();
                }
            }

            public async Task ConfirmLock(Guid gameId) {
                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    if (_list.ContainsKey(gameId))
                        throw new GameInstallationInProgressException();
                    _list.Add(gameId, new CancellationTokenSource());
                    await _eventRaiser(new GameLockChanged(gameId, true)).ConfigureAwait(false);
                } finally {
                    _lock.Release();
                }
            }

            public async Task ReleaseLock(Guid gameId) {
                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    var cts = _list[gameId];
                    cts.Dispose();
                    _list.Remove(gameId);
                    await _eventRaiser(new GameLockChanged(gameId, false)).ConfigureAwait(false);
                } finally {
                    _lock.Release();
                }
            }

            public void Cancel() {
                _lock.Wait();
                try {
                    foreach (var l in _list)
                        l.Value.Cancel();
                } finally {
                    _lock.Release();
                }
            }
        }

        // TODO: Consider improving
        // - Keep a cache per app start?
        // - Keep a persistent cache?
        // - Further optimizations?
        // - Allow user to clean once, and then just base stuff on packages?
        class ContentCleaner
        {
            public bool BackupFiles { get; } = true;

            public Task CleanAsync(IAbsoluteDirectoryPath workingDirectory,
                IReadOnlyCollection<IRelativePath> exclusions, IEnumerable<string> fileTypes,
                IAbsoluteDirectoryPath backupPath) {
                return Task.Factory.StartNew(() => Clean(workingDirectory, exclusions, fileTypes, backupPath),
                    TaskCreationOptions.LongRunning);
            }

            public void Clean(IAbsoluteDirectoryPath workingDirectory, IReadOnlyCollection<IRelativePath> exclusions,
                IEnumerable<string> fileTypes, IAbsoluteDirectoryPath backupPath) {
                Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(workingDirectory);
                Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(backupPath);
                var toRemove = GetFilesToRemove(workingDirectory, exclusions, fileTypes);
                foreach (var entry in toRemove) {
                    if (BackupFiles)
                        BackupEntry(workingDirectory, backupPath, entry);
                    else
                        entry.Delete();
                    // TODO: Optimize
                    DeleteEmptyFolders(entry.ParentDirectoryPath);
                }
            }

            static void BackupEntry(IAbsoluteDirectoryPath workingDirectory, IAbsoluteDirectoryPath backupPath,
                IAbsoluteFilePath entry) {
                var backupDestination = entry.GetRelativePathFrom(workingDirectory).GetAbsolutePathFrom(backupPath);
                DeleteDestinationIfDirectory(backupDestination);
                DeleteParentFilesIfExists(backupDestination, backupPath);
                backupDestination.MakeSureParentPathExists();
                entry.Move(backupDestination);
            }

            static void DeleteDestinationIfDirectory(IAbsoluteFilePath backupDestination) {
                if (Directory.Exists(backupDestination.ToString()))
                    backupDestination.ToString().ToAbsoluteDirectoryPath().Delete(true);
            }

            static void DeleteParentFilesIfExists(IPath path, IAbsoluteDirectoryPath backupPath) {
                while (path.HasParentDirectory) {
                    path = path.ParentDirectoryPath;
                    var s = path.ToString();
                    if (File.Exists(s)) {
                        File.Delete(s);
                        break;
                    }
                    if (path.Equals(backupPath))
                        break;
                }
            }

            static void DeleteEmptyFolders(IAbsoluteDirectoryPath path) {
                if (!path.DirectoryInfo.EnumerateFiles().Any()
                    && !path.DirectoryInfo.EnumerateDirectories().Any())
                    path.DirectoryInfo.Delete();
                while (path.HasParentDirectory) {
                    path = path.ParentDirectoryPath;
                    if (!path.DirectoryInfo.EnumerateFiles().Any()
                        && !path.DirectoryInfo.EnumerateDirectories().Any())
                        path.DirectoryInfo.Delete();
                }
            }

            static IEnumerable<IAbsoluteFilePath> GetFilesToRemove(IAbsoluteDirectoryPath workingDirectory,
                IReadOnlyCollection<IRelativePath> exclusions,
                IEnumerable<string> fileTypes) {
                var excludedDirectories = GetExcludedDirectories(workingDirectory, exclusions).ToArray();
                var excludedFiles = GetExcludedFiles(workingDirectory, exclusions).ToArray();
                return workingDirectory.GetFiles(fileTypes, SearchOption.AllDirectories)
                    .Where(x => IsNotExcluded(x, excludedDirectories, excludedFiles));
            }

            static bool IsNotExcluded(IFilePath x, IReadOnlyCollection<IAbsoluteDirectoryPath> excludedDirectories,
                IEnumerable<IAbsoluteFilePath> excludedFiles) {
                return IsNotDirectoryExcluded(x, excludedDirectories) && !excludedFiles.Contains(x);
            }

            static bool IsNotDirectoryExcluded(IFilePath x,
                IReadOnlyCollection<IAbsoluteDirectoryPath> excludedDirectories) {
                IPath b = x;
                while (b.HasParentDirectory) {
                    var parent = b.ParentDirectoryPath;
                    if (excludedDirectories.Contains(parent))
                        return false;
                    b = parent;
                }
                return true;
            }

            static IEnumerable<IAbsoluteFilePath> GetExcludedFiles(IAbsoluteDirectoryPath workingDirectory,
                IEnumerable<IRelativePath> exclusions) {
                return exclusions.Where(x => x.IsFilePath)
                    .Select(x => x.GetAbsolutePathFrom(workingDirectory))
                    .Cast<IAbsoluteFilePath>();
            }

            static IEnumerable<IAbsoluteDirectoryPath> GetExcludedDirectories(IAbsoluteDirectoryPath workingDirectory,
                IEnumerable<IRelativePath> exclusions) {
                return exclusions.Where(x => x.IsDirectoryPath)
                    .Select(x => x.GetAbsolutePathFrom(workingDirectory))
                    .Cast<IAbsoluteDirectoryPath>();
            }
        }
    }

    public class GameLockChanged : IDomainEvent
    {
        public GameLockChanged(Guid gameId, bool isLocked) {
            GameId = gameId;
            IsLocked = isLocked;
        }

        public Guid GameId { get; set; }
        public bool IsLocked { get; set; }
    }

    public interface IContentInstallationService
    {
        Task Install(IInstallContentAction<IInstallableContent> action);
        void Abort(Guid gameId);
        void Abort();
        Task Uninstall(IUninstallContentAction2<IUninstallableContent> action);
    }

    public class StatusChanged : IDomainEvent
    {
        public StatusChanged(Status status, double progress = 0, double speed = 0) {
            if (progress.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(progress), "NaN");
            if (speed.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(speed), "NaN");
            if (progress < 0)
                throw new ArgumentOutOfRangeException(nameof(progress), "Below 0");
            if (speed < 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Below 0");
            Status = status;
            Progress = progress;
            Speed = speed;
        }

        public Status Status { get; }
        public double Progress { get; }
        public double Speed { get; }
    }

    public class ContentStatusChanged : IDomainEvent
    {
        public ContentStatusChanged(IContent content, ItemState state, double progress = 0, double speed = 0) {
            if (progress.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(progress), "NaN");
            if (speed.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(speed), "NaN");
            if (progress < 0)
                throw new ArgumentOutOfRangeException(nameof(progress), "Below 0");
            if (speed < 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Below 0");
            Content = content;
            State = state;
            Progress = progress;
            Speed = speed;
        }

        public IContent Content { get; }
        public ItemState State { get; }
        public double Progress { get; }
        public double Speed { get; }
    }

    public enum ItemState
    {
        Uptodate,
        NotInstalled,
        UpdateAvailable,
        Uninstalled,

        Installing,
        Updating,
        Uninstalling
    }

    public class GameInstallationInProgressException : Exception {}
}