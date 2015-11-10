// <copyright company="SIX Networks GmbH" file="PathMover.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Services;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Packages;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class PathMover : IDomainService
    {
        static readonly string[] excludeFolders = {"missions", "mpmissions"};

        public void MoveModFolders(IAbsoluteDirectoryPath oldModsPath, IAbsoluteDirectoryPath newModsPath) {
            newModsPath.MakeSurePathExists();

            foreach (var dir in Directory.EnumerateDirectories(oldModsPath.ToString())
                .Where(x => !excludeFolders.Contains(Path.GetFileName(x).ToLower()))
                .Where(x => File.Exists(Path.Combine(x, Package.SynqInfoFile)) ||
                            Directory.Exists(Path.Combine(x, Repository.RepoFolderName))))
                TryMoveDir(dir.ToAbsoluteDirectoryPath(), newModsPath);
        }

        static void TryMoveDir(IAbsoluteDirectoryPath dir, IAbsoluteDirectoryPath newModsPath) {
            var newPath = newModsPath.GetChildDirectoryWithName(dir.DirectoryName);
            try {
                if (newPath.Exists)
                    Directory.Delete(newPath.ToString(), true);
            } finally {
                Tools.FileUtil.Ops.MoveDirectory(dir, newPath);
            }
        }
    }
}