// <copyright company="SIX Networks GmbH" file="UserconfigProcessor.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NDepend.Path;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma
{
    public class UserconfigProcessor : IEnableLogging
    {
        public string ProcessUserconfig(IAbsoluteDirectoryPath modPath, IAbsoluteDirectoryPath gamePath,
            string exisitingChecksum, bool force = true) {
            Contract.Requires<ArgumentNullException>(modPath != null);
            Contract.Requires<ArgumentNullException>(gamePath != null);

            var backupAndClean = force;

            var path = GetUserconfigPath(modPath);
            if (!File.Exists(path) && !Directory.Exists(path))
                return null;

            this.Logger().Info("Found userconfig to process at " + path);

            var checksum = GetConfigChecksum(path);

            if (checksum != exisitingChecksum)
                backupAndClean = true;

            var uconfig = gamePath.GetChildDirectoryWithName("userconfig");
            var uconfigPath = uconfig.GetChildDirectoryWithName(Mod.GetRepoName(modPath.DirectoryName));

            if (backupAndClean)
                new UserconfigBackupAndClean().ProcessBackupAndCleanInstall(path, gamePath, uconfig, uconfigPath);
            else
                new UserconfigUpdater().ProcessMissingFiles(path, gamePath, uconfig, uconfigPath);

            return checksum;
        }

        string GetConfigChecksum(string path) {
            if (File.Exists(path))
                return GetChecksum(path.ToAbsoluteFilePath());
            if (Directory.Exists(path))
                return CreateMd5ForFolder(path);
            throw new ArgumentOutOfRangeException("UserConfig Path provided not a directory or file", path);
        }

        static string CreateMd5ForFolder(string path) {
            // assuming you want to include nested folders
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .OrderBy(p => p).ToList();

            MD5 md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++) {
                string file = files[i];

                // hash path
                string relativePath = file.Substring(path.Length + 1);
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                byte[] contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }

        static string GetChecksum(IAbsoluteFilePath uconfigPath) {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(uconfigPath.ToString()))
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
        }

        static string GetUserconfigPath(IAbsoluteDirectoryPath modPath) {
            var storePath = modPath.GetChildDirectoryWithName("store");
            var path = storePath.GetChildFileWithName("userconfig.tar");
            if (path.Exists)
                return path.ToString();

            path = modPath.GetChildFileWithName("userconfig.tar");
            if (path.Exists)
                return path.ToString();

            var dir = storePath.GetChildDirectoryWithName("userconfig");
            if (!dir.Exists)
                dir = modPath.GetChildDirectoryWithName("userconfig");
            return dir.ToString();
        }
    }
}