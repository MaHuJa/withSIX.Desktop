// <copyright company="SIX Networks GmbH" file="ServerModsMigrator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Applications.Services
{
    public class ServerModsMigrator : IDomainService
    {
        public async Task<bool> ShouldMoveServerMods(Game currentGame) {
            var folders = GetServerModFolders(currentGame).ToArray();
            if (!folders.Any())
                return false;

            var report =
                (await UserError.Throw(new BasicUserError("Migrate server mods?",
                    "It appears you still have a deprecated 'servermods' folder\nThis folder is no longer relevant, would you like to merge the mods to the mod installation path?\n\nPlease make sure the Game is closed, and no other mod files (incl readmes) or utilities are open before proceeding",
                    RecoveryCommands.YesNoCommands)))
                == RecoveryOptionResult.RetryOperation;

            return report;
        }

        public void MigrateServerMods(Game currentGame, StatusRepo statusRepo) {
            statusRepo.Action = RepoStatus.Moving;
            MigrateServerMods(GetServerModFolders(currentGame),
                currentGame.Modding().ModPaths.Path, statusRepo);
        }

        IEnumerable<string> GetServerModFolders(Game game) {
            const string subFolder = "servermods";
            return new[] {game.InstalledState.Directory, game.Modding().ModPaths.Path}
                .Where(x => x != null)
                .Select(x => x.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Tools.FileUtil.CleanPath)
                .Distinct()
                .Select(x => Path.Combine(x, subFolder))
                .Where(Directory.Exists);
        }

        static void MigrateServerMods(IEnumerable<string> folders, IAbsoluteDirectoryPath modPath, StatusRepo statusRepo) {
            foreach (var folder in folders) {
                foreach (var modFolder in Directory.EnumerateDirectories(folder)) {
                    var status = new Status(Path.GetFileName(modFolder), statusRepo) {Action = RepoStatus.Processing};
                    MigrateServerModFolder(modFolder.ToAbsoluteDirectoryPath(), modPath);
                    status.Progress = 100;
                }
                Directory.Delete(folder, true);
            }
        }

        static void MigrateServerModFolder(IAbsoluteDirectoryPath modFolder, IAbsoluteDirectoryPath modPath) {
            var folderName = modFolder.DirectoryName;
            var destination = modPath.GetChildDirectoryWithName(folderName);
            if (!destination.Exists)
                Tools.FileUtil.Ops.MoveDirectory(modFolder, destination);
            else
                Directory.Delete(modFolder.ToString(), true);
        }
    }
}