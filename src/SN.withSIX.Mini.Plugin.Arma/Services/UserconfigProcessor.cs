// <copyright company="SIX Networks GmbH" file="UserconfigProcessor.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Mini.Plugin.Arma.Services
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
            var uconfigPath = uconfig.GetChildDirectoryWithName(GetRepoName(modPath.DirectoryName));

            if (backupAndClean)
                new UserconfigBackupAndClean().ProcessBackupAndCleanInstall(path, gamePath, uconfig, uconfigPath);
            else
                new UserconfigUpdater().ProcessMissingFiles(path, gamePath, uconfig, uconfigPath);

            return checksum;
        }

        public static string GetRepoName(string name) {
            Contract.Requires<ArgumentNullException>(name != null);

            return name.StartsWith("@") ? name.Substring(1).ToLower() : name.ToLower();
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

        abstract class UserconfigProcessorBase : IEnableLogging
        {
            protected bool ConfirmUserconfigIsNotFile(string uconfig) {
                if (File.Exists(uconfig) && !Directory.Exists(uconfig)) {
                    this.Logger().Warn("WARNING: Userconfig folder not existing (or is file), aborting");
                    return false;
                }
                return true;
            }
        }

        class UserconfigBackupAndClean : UserconfigProcessorBase
        {
            public void ProcessBackupAndCleanInstall(string path, IAbsoluteDirectoryPath gamePath,
                IAbsoluteDirectoryPath uconfig, IAbsoluteDirectoryPath uconfigPath) {
                BackupExistingUserconfig(uconfigPath.ToString());
                TryUserconfigClean(path, gamePath, uconfig, uconfigPath);
            }

            static void BackupExistingUserconfig(string uconfigPath) {
                var time = Tools.Generic.GetCurrentUtcDateTime.Ticks;
                if (Directory.Exists(uconfigPath))
                    Directory.Move(uconfigPath, uconfigPath + "_" + time);
                if (File.Exists(uconfigPath))
                    File.Move(uconfigPath, uconfigPath + "_" + time);
            }

            void TryUserconfigClean(string path, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath uconfig,
                IAbsoluteDirectoryPath uconfigPath) {
                if (!ConfirmUserconfigIsNotFile(uconfig.ToString()))
                    return;

                if (Directory.Exists(path))
                    TryUserconfigDirectoryOverwrite(path.ToAbsoluteDirectoryPath(), uconfigPath);
                else
                    TryUserconfigUnpackOverwrite(path.ToAbsoluteFilePath(), gamePath);
            }

            static void TryUserconfigUnpackOverwrite(IAbsoluteFilePath path, IAbsoluteDirectoryPath gamePath) {
                try {
                    Tools.Compression.UnpackRetryUpdater(path, gamePath, true);
                } catch (Win32Exception ex) {
                    if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                        throw;
                    throw ex.HandleUserCancelled();
                }
            }

            static void TryUserconfigDirectoryOverwrite(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath uconfigPath) {
                try {
                    Tools.FileUtil.Ops.CopyDirectoryWithUpdaterFallbackAndRetry(path, uconfigPath, true);
                } catch (Win32Exception ex) {
                    if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                        throw;
                    throw ex.HandleUserCancelled();
                }
            }
        }

        class UserconfigUpdater : UserconfigProcessorBase
        {
            public void ProcessMissingFiles(string path, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath uconfig,
                IAbsoluteDirectoryPath uconfigPath) {
                if (uconfigPath.Exists)
                    this.Logger().Info("Userconfig folder already exists, checking for new files only");
                TryUserconfigUpdate(path, gamePath, uconfig, uconfigPath);
            }

            void TryUserconfigUpdate(string path, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath uconfig,
                IAbsoluteDirectoryPath uconfigPath) {
                if (!ConfirmUserconfigIsNotFile(uconfig.ToString()))
                    return;

                if (Directory.Exists(path))
                    TryUserconfigDirectory(path.ToAbsoluteDirectoryPath(), uconfigPath);
                else if (File.Exists(path))
                    TryUserconfigUnpack(path.ToAbsoluteFilePath(), gamePath);
            }

            static void TryUserconfigUnpack(IAbsoluteFilePath path, IAbsoluteDirectoryPath gamePath) {
                try {
                    Tools.Compression.UnpackRetryUpdater(path, gamePath);
                } catch (Win32Exception ex) {
                    if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                        throw;
                    throw ex.HandleUserCancelled();
                }
            }

            void TryUserconfigDirectory(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath uconfigPath) {
                try {
                    Tools.FileUtil.Ops.CopyDirectoryWithUpdaterFallbackAndRetry(path, uconfigPath);
                } catch (Win32Exception ex) {
                    if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                        throw;
                    throw ex.HandleUserCancelled();
                } catch (IOException e) {
                    this.Logger().FormattedWarnException(e);
                }
            }
        }
    }
}