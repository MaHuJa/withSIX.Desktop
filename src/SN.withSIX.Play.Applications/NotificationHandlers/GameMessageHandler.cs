// <copyright company="SIX Networks GmbH" file="GameMessageHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    [StayPublic]
    public class GameMessageHandler : INotificationHandler<ModPathChangedEvent>,
        INotificationHandler<ModAndSynqPathsChangedEvent>, INotificationHandler<SynqPathChangedEvent>
    {
        readonly IDialogManager _dialogManager;
        readonly UpdateManager _updateManager;

        public GameMessageHandler(UpdateManager updateManager, IDialogManager dialogManager) {
            _updateManager = updateManager;
            _dialogManager = dialogManager;
        }

        public async void Handle(ModAndSynqPathsChangedEvent notification) {
            var newModPath = notification.NewPaths.Path;
            if (newModPath != null)
                await ModPathWarningDialog(newModPath).ConfigureAwait(false);

            try {
                var oldSp = notification.OldPaths.RepositoryPath;
                var newSp = notification.NewPaths.RepositoryPath;
                await
                    MoveExistingModDataWhenRequested(notification.OldPaths.Path, notification.NewPaths.Path,
                        oldSp == null ? null : oldSp.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory),
                        newSp == null ? null : newSp.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory),
                        notification.Game)
                        .ConfigureAwait(false);
            } finally {
                ((Game) notification.Game).RefreshState();
            }
        }

        public async void Handle(ModPathChangedEvent notification) {
            var newModPath = notification.NewPaths.Path;
            if (newModPath != null)
                await ModPathWarningDialog(newModPath).ConfigureAwait(false);

            try {
                await
                    MoveExistingModDataWhenRequested(notification.OldPaths.Path, notification.NewPaths.Path, null, null,
                        notification.Game)
                        .ConfigureAwait(false);
            } finally {
                ((Game) notification.Game).RefreshState();
            }
        }

        public async void Handle(SynqPathChangedEvent notification) {
            var newSynqPath = notification.NewPaths.RepositoryPath;
            if (newSynqPath != null)
                await SynqPathWarningDialog(newSynqPath).ConfigureAwait(false);

            try {
                var oldSp = notification.OldPaths.RepositoryPath;
                var newSp = notification.NewPaths.RepositoryPath;
                await
                    MoveExistingSynqDataWhenRequested(
                        oldSp == null ? null : oldSp.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory),
                        newSp == null ? null : newSp.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory),
                        notification.Game)
                        .ConfigureAwait(false);
            } finally {
                ((Game) notification.Game).RefreshState();
            }
        }

        async Task MoveExistingModDataWhenRequested(IAbsoluteDirectoryPath oldModsPath,
            IAbsoluteDirectoryPath newModPath, IAbsoluteDirectoryPath oldSynqPath,
            IAbsoluteDirectoryPath newSynqPath, ISupportModding game) {
            if (ArePathsEqual(oldModsPath, newModPath)) {
                if (oldSynqPath != null && newSynqPath != null)
                    await MoveExistingSynqDataWhenRequested(oldSynqPath, newSynqPath, game);
                return;
            }

            var todo = await ModPathChangedDialog(oldModsPath, newModPath);
            if (todo) {
                await
                    UiTaskHandler.TryAction(
                        () => PerformMoveModAndSynq(oldModsPath, newModPath, oldSynqPath, newSynqPath, game))
                        .ConfigureAwait(false);
            }
        }

        async Task PerformMoveModAndSynq(IAbsoluteDirectoryPath oldModsPath, IAbsoluteDirectoryPath newModPath,
            IAbsoluteDirectoryPath oldSynqPath, IAbsoluteDirectoryPath newSynqPath, ISupportModding game) {
            await _updateManager.MoveModFoldersIfValidAndExists(oldModsPath, newModPath).ConfigureAwait(false);
            if (oldSynqPath != null && newSynqPath != null
                && !Tools.FileUtil.ComparePathsOsCaseSensitive(oldSynqPath.ToString(), newSynqPath.ToString())) {
                game.Controller.BundleManager.Repo.Dispose(); // Unlock
                await _updateManager.MovePathIfValidAndExists(oldSynqPath, newSynqPath);
            }
        }

        async Task MoveExistingSynqDataWhenRequested(IAbsoluteDirectoryPath oldSynqPath,
            IAbsoluteDirectoryPath newSynqPath, ISupportModding game) {
            if (ArePathsEqual(oldSynqPath, newSynqPath))
                return;

            var todo = await SynqPathChangedDialog(oldSynqPath, newSynqPath).ConfigureAwait(false);

            if (todo) {
                game.Controller.BundleManager.Repo.Dispose(); // Unlock
                await
                    UiTaskHandler.TryAction(
                        () => _updateManager.MovePathIfValidAndExists(oldSynqPath, newSynqPath))
                        .ConfigureAwait(false);
            }
        }

        static bool ArePathsEqual(IAbsoluteDirectoryPath old, IAbsoluteDirectoryPath @new) {
            return old == @new ||
                   (old != null && @new != null &&
                    Tools.FileUtil.ComparePathsOsCaseSensitive(old.ToString(), @new.ToString()));
        }

        async Task<bool> ModPathChangedDialog(IAbsoluteDirectoryPath oldModsPath, IAbsoluteDirectoryPath newModPath) {
            return (await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                string.Format("Mod path changed, do you want to move existing data?\nFrom: {0} to {1}",
                    oldModsPath, newModPath), "Move existing data?", SixMessageBoxButton.YesNo)))
                   == SixMessageBoxResult.Yes;
        }

        async Task<bool> SynqPathChangedDialog(IAbsoluteDirectoryPath oldSynqPath, IAbsoluteDirectoryPath newSynqPath) {
            return (await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                string.Format("Synq path changed, do you want to move existing data?\nFrom: {0} to {1}",
                    oldSynqPath, newSynqPath), "Move existing data?", SixMessageBoxButton.YesNo)))
                   == SixMessageBoxResult.Yes;
        }

        Task<SixMessageBoxResult> ModPathWarningDialog(IAbsoluteDirectoryPath value) {
            return _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                String.Format(
                    "You seem to have set a custom Mod installation path: {0}\nPlease make sure this path is writable, or restart the application as Administrator if needed",
                    value), "Custom path set"));
        }

        Task<SixMessageBoxResult> SynqPathWarningDialog(IAbsoluteDirectoryPath value) {
            return _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                String.Format(
                    "You seem to have set a custom synq installation path: {0}\nPlease make sure this path is writable, or restart the application as Administrator if needed",
                    value), "Custom path set"));
        }
    }
}