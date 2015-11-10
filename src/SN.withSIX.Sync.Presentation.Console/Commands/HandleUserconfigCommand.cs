// <copyright company="SIX Networks GmbH" file="HandleUserconfigCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class HandleUserconfigCommand : BaseCommand
    {
        public HandleUserconfigCommand() {
            IsCommand("handleuserconfig", "Handle userconfig if available");
            AllowsAnyAdditionalArguments(" <folder> (<folder>...)");
        }

        public override int Run(params string[] remainingArguments) {
            if (!remainingArguments.Any())
                throw new ConsoleHelpAsException("Please specify at least one folder to repack pbos of");

            foreach (var dir in remainingArguments.Select(dir => Repository.RepoTools.GetRootedPath(dir)))
                HandleUserconfig(dir);

            return 0;
        }

        void HandleUserconfig(IAbsoluteDirectoryPath dir) {
            var userConfigPath = dir.GetChildDirectoryWithName("userconfig");
            if (!userConfigPath.Exists)
                return;
            System.Console.WriteLine("Found userconfig in {0}, processing", dir);
            HandleUserConfigPath(dir, userConfigPath);
        }

        void HandleUserConfigPath(IAbsoluteDirectoryPath dir, IAbsoluteDirectoryPath userConfigPath) {
            var files = userConfigPath.DirectoryInfo.EnumerateFiles();
            var directories = userConfigPath.DirectoryInfo.EnumerateDirectories();
            var hasFiles = files.Any();
            var hasDirectories = directories.Any();
            if (hasFiles && hasDirectories) {
                throw new NotSupportedException(
                    "The userconfig folder contains both files and folders, unable to detect what type it is");
            }

            var storePath = dir.GetChildDirectoryWithName("store");
            if (hasFiles)
                HandleUserconfigFiles(userConfigPath, storePath, files);
            else
                HandleUserconfigDirectories(userConfigPath, storePath, directories);
        }

        void HandleUserconfigDirectories(IAbsoluteDirectoryPath userConfigPath, IAbsoluteDirectoryPath storePath,
            IEnumerable<DirectoryInfo> directories) {
            System.Console.WriteLine("Directory based userconfig");
            WriteUserConfigTar(userConfigPath, storePath);
            userConfigPath.DirectoryInfo.Delete(true);
        }

        void HandleUserconfigFiles(IAbsoluteDirectoryPath userConfigPath, IAbsoluteDirectoryPath storePath,
            IEnumerable<FileInfo> files) {
            System.Console.WriteLine("File based userconfig");
            var subDir =
                userConfigPath.GetChildDirectoryWithName(
                    userConfigPath.ParentDirectoryPath.DirectoryName.ToLower().Replace("@", ""));
            subDir.MakeSurePathExists();
            foreach (var f in files)
                f.MoveTo(subDir.GetChildFileWithName(f.Name).ToString());
            WriteUserConfigTar(userConfigPath, storePath);
            userConfigPath.DirectoryInfo.Delete(true);
        }

        static void WriteUserConfigTar(IAbsoluteDirectoryPath userConfigPath, IAbsoluteDirectoryPath storePath) {
            storePath.MakeSurePathExists();
            //using (var tarFile = new TmpFileCreated()) {
            Tools.Compression.PackTar(userConfigPath, storePath.GetChildFileWithName("userconfig.tar"));
            //  tarFile.FilePath
            //Tools.Compression.Gzip.GzipAuto(tarFile.FilePath, storePath.GetChildFileWithName("userconfig.tar.gz"));
            //}
        }
    }
}