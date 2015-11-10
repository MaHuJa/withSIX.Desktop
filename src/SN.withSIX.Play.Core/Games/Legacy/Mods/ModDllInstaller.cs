// <copyright company="SIX Networks GmbH" file="ModDllInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [Obsolete]
    public class ModDllInstaller : IEnableLogging
    {
        const string Ts3SubPath = "plugins";
        static readonly string[] ts332Dlls = {"acre_win32.dll", "acre2_win32.dll"};
        static readonly string[] ts364Dlls = {"acre_win64.dll", "acre2_win64.dll"};
        readonly Game _game;
        readonly LocalMachineInfo _localMachineInfo;

        public ModDllInstaller(LocalMachineInfo lmi, Game game) {
            _localMachineInfo = lmi;
            _game = game;
        }

        public bool ProcessDlls(IAbsoluteDirectoryPath path, bool force = true) {
            var dlls = CheckForDlls(path);
            if (!dlls.Any())
                return false;

            foreach (var dll in dlls)
                TryProcessDll(force, dll);

            return true;
        }

        void TryProcessDll(bool force, IAbsoluteFilePath dll) {
            try {
                ProcessDll(dll, force);
            } catch (PathDoesntExistException e) {
                this.Logger().FormattedWarnException(e, "Path: " + e.Path);
            } catch (Win32Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        static IAbsoluteFilePath[] CheckForDlls(IAbsoluteDirectoryPath path) {
            var fileEntries = Directory.EnumerateFiles(path.ToString(), "*.dll",
                SearchOption.AllDirectories);
            return fileEntries.Where(file => file.EndsWith(".dll")).Select(x => x.ToAbsoluteFilePath()).ToArray();
        }

        void ProcessDll(IAbsoluteFilePath dll, bool force = true) {
            var fileName = dll.FileName.ToLower();
            var ts332Path = _localMachineInfo.TS3_32_Path;
            var ts364Path = _localMachineInfo.TS3_64_Path;
            switch (fileName) {
            case "task_force_radio_win32.dll": {
                InstallTaskForceRadio(dll, ts332Path, force);
                break;
            }
            case "task_force_radio_win64.dll": {
                InstallTaskForceRadio(dll, ts364Path, force);
                break;
            }
            case "dsound.dll": {
                var path = _game.InstalledState.Directory;
                if (path != null)
                    InstallDll(dll, path, null, force);
                break;
            }
            default: {
                if (ts332Dlls.Contains(fileName))
                    InstallTs3Plugin(dll, ts332Path, force);
                else if (ts364Dlls.Contains(fileName))
                    InstallTs3Plugin(dll, ts364Path, force);
                break;
            }
            }
        }

        public static void InstallTaskForceRadio(IAbsoluteFilePath fi, IAbsoluteDirectoryPath path, bool force) {
            if (!InstallTs3Plugin(fi, path, force))
                return;
            var di = fi.GetBrotherDirectoryWithName("radio-sounds");
            if (di.Exists)
                InstallTs3PluginFolder(di, path, force);
        }

        public static void InstallTs3PluginFolder(IAbsoluteDirectoryPath di, IAbsoluteDirectoryPath path, bool force) {
            if (path != null)
                InstallFolder(di, path, Ts3SubPath, force);
        }

        public static bool InstallTs3Plugin(IAbsoluteFilePath fi, IAbsoluteDirectoryPath path, bool force) {
            return path != null && InstallDll(fi, path, Ts3SubPath, force);
        }

        static Version GetTs3Version(IAbsoluteDirectoryPath path) {
            Contract.Requires<ArgumentNullException>(path != null);

            var executable = path.DirectoryInfo.EnumerateFiles("ts3client_win*.exe").FirstOrDefault();
            if (executable == null)
                throw new FileNotFoundException("The Teamspeak 3 Executable (ts3client_win(64/32).exe) was not found");
            var versionInfo = FileVersionInfo.GetVersionInfo(executable.FullName);
            return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart,
                versionInfo.FilePrivatePart);
        }

        static void InstallFolder(IAbsoluteDirectoryPath di, IAbsoluteDirectoryPath destination, string subPath,
            bool force) {
            Contract.Requires<ArgumentNullException>(destination != null);

            if (!destination.Exists)
                throw new PathDoesntExistException(destination.ToString());
            if (subPath != null)
                destination = destination.GetChildDirectoryWithName(subPath);

            TryCopyFolder(di, destination.GetChildDirectoryWithName(di.DirectoryName), force);
        }

        static void TryCopyFolder(IAbsoluteDirectoryPath di, IAbsoluteDirectoryPath fullPath, bool force) {
            try {
                Tools.FileUtil.Ops.CopyDirectoryWithUpdaterFallbackAndRetry(di, fullPath, force);
            } catch (Win32Exception ex) {
                if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                    throw;
                throw ex.HandleUserCancelled();
            }
        }

        static bool InstallDll(IAbsoluteFilePath fi, IAbsoluteDirectoryPath destination, string subPath = null,
            bool force = true) {
            Contract.Requires<ArgumentNullException>(destination != null);

            if (!destination.Exists)
                throw new PathDoesntExistException(destination.ToString());
            if (subPath != null)
                destination = destination.GetChildDirectoryWithName(subPath);
            var fullPath = destination.GetChildFileWithName(fi.FileName);

            if (!force && fullPath.Exists)
                return false;

            return TryCopyDll(fi, fullPath);
        }

        static bool TryCopyDll(IAbsoluteFilePath fi, IAbsoluteFilePath fullPath) {
            try {
                Tools.FileUtil.Ops.CopyWithUpdaterFallbackAndRetry(fi, fullPath, true, true);
                return true;
            } catch (Win32Exception ex) {
                if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                    throw;
                throw ex.HandleUserCancelled();
            }
        }
    }
}