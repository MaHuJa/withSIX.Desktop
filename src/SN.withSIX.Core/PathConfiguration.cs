// <copyright company="SIX Networks GmbH" file="PathConfiguration.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Core
{
    public class PathConfiguration : IPathConfiguration, IDomainService, IEnableLogging
    {
        const string CompanyPath = "SIX Networks";
        static readonly string steamRegistry = @"SOFTWARE\Valve\Steam";
        public readonly IAbsoluteFilePath CmdExe =
            Path.Combine(Environment.SystemDirectory, "cmd.exe").ToAbsoluteFilePath();
        public readonly string SelfUpdaterExe = "withSIX-SelfUpdater.exe";
        public readonly string ServiceExe = "withSIX-Updater.exe";
        public IAbsoluteDirectoryPath ProgramDataPath { get; private set; }
        public IAbsoluteDirectoryPath SharedFilesPath { get; private set; }
        public IAbsoluteFilePath ServiceExePath { get; private set; }
        public IAbsoluteFilePath SelfUpdaterExePath { get; private set; }
        public IAbsoluteDirectoryPath SharedDllPath { get; private set; }
        public IAbsoluteDirectoryPath LocalDataSharedPath { get; private set; }
        public IAbsoluteDirectoryPath EntryPath { get; private set; }
        public IAbsoluteFilePath EntryLocation { get; private set; }
        public IAbsoluteDirectoryPath AwesomiumPath { get; set; }
        public IAbsoluteDirectoryPath NotePath { get; private set; }
        public IAbsoluteDirectoryPath LocalDataRootPath { get; private set; }
        public IAbsoluteDirectoryPath SynqRootPath { get; private set; }
        public IAbsoluteDirectoryPath JavaPath { get; private set; }
        public IAbsoluteDirectoryPath AppPath { get; private set; }
        public IAbsoluteDirectoryPath ConfigPath { get; private set; }
        public IAbsoluteDirectoryPath DataPath { get; private set; }
        public IAbsoluteDirectoryPath LocalDataPath { get; private set; }
        public IAbsoluteDirectoryPath LogPath { get; private set; }
        public IAbsoluteDirectoryPath MyDocumentsPath { get; private set; }
        public IAbsoluteDirectoryPath TempPath { get; private set; }
        public IAbsoluteDirectoryPath ToolMinGwBinPath { get; private set; }
        public IAbsoluteDirectoryPath ToolCygwinBinPath { get; private set; }
        public IAbsoluteDirectoryPath ToolPath { get; private set; }
        public IAbsoluteDirectoryPath SteamPath { get; private set; }
        //public string ToolsPath { get; private set; }
        public bool PathsSet { get; private set; }
        public IAbsoluteDirectoryPath StartPath { get; private set; }
        // TODO

        public virtual void SetPaths(IAbsoluteDirectoryPath appPath = null, IAbsoluteDirectoryPath dataPath = null,
            IAbsoluteDirectoryPath localDataPath = null, IAbsoluteDirectoryPath tempPath = null,
            IAbsoluteDirectoryPath configPath = null, IAbsoluteDirectoryPath toolPath = null,
            IAbsoluteDirectoryPath sharedDataPath = null) {
            //if (PathsSet) throw new Exception("Paths are already set!");
            if (PathsSet)
                this.Logger().Debug("Paths were already set!");

            EntryLocation = CommonBase.AssemblyLoader.GetEntryLocation().ToAbsoluteFilePath();
            EntryPath = CommonBase.AssemblyLoader.GetEntryPath().ToAbsoluteDirectoryPath();
            ProcessExtensions.DefaultWorkingDirectory = EntryPath;
            AppPath = appPath ?? GetAppPath();
            DataPath = dataPath ?? GetDataPath();
            NotePath = DataPath.GetChildDirectoryWithName("Notes");
            LocalDataRootPath = GetLocalDataRootPath();
            LocalDataPath = localDataPath ?? GetLocalDataPath();
            LocalDataSharedPath = sharedDataPath ?? GetLocalDataSharedPath();
            SharedDllPath = GetLocalSharedDllPath();
            LogPath = LocalDataPath.GetChildDirectoryWithName("Logs");
            TempPath = tempPath ??
                       Path.Combine(Path.GetTempPath(), Common.AppCommon.ApplicationName + " 2")
                           .ToAbsoluteDirectoryPath();
            ToolPath = toolPath ?? LocalDataSharedPath.GetChildDirectoryWithName("Tools");
            ToolMinGwBinPath = ToolPath.GetChildDirectoryWithName("mingw").GetChildDirectoryWithName("bin");
            ToolCygwinBinPath = ToolPath.GetChildDirectoryWithName("cygwin").GetChildDirectoryWithName("bin");
            ConfigPath = configPath ?? ToolPath.GetChildDirectoryWithName("Config");
            StartPath = Directory.GetCurrentDirectory().ToAbsoluteDirectoryPath();

            AwesomiumPath = LocalDataSharedPath.GetChildDirectoryWithName("Awesomium");

            MyDocumentsPath = GetMyDocumentsPath();
            ProgramDataPath = GetProgramDataPath();
            SharedFilesPath = AppPath;

            ServiceExePath = SharedFilesPath.GetChildFileWithName(Common.IsMini ? "Sync.exe" : ServiceExe);
            SelfUpdaterExePath = SharedFilesPath.GetChildFileWithName(SelfUpdaterExe);
            SteamPath = GetSteamPath();

            SynqRootPath = ProgramDataPath.GetChildDirectoryWithName("Synq");

            PathsSet = true;
        }

        static IAbsoluteDirectoryPath GetSteamPath() {
            var p = Tools.Generic.NullSafeGetRegKeyValue<string>(steamRegistry, "InstallPath");
            return p.IsBlankOrWhiteSpace() ? null : p.Trim().ToAbsoluteDirectoryPath();
        }

        IAbsoluteDirectoryPath GetSystemSharedPath() {
            return ProgramDataPath.GetChildDirectoryWithName(CompanyPath).GetChildDirectoryWithName("Shared");
        }

        public static IAbsoluteDirectoryPath GetLocalDataRootPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                CompanyPath).ToAbsoluteDirectoryPath();
        }

        public static IAbsoluteDirectoryPath GetRoamingRootPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                CompanyPath).ToAbsoluteDirectoryPath();
        }

        IAbsoluteDirectoryPath GetLocalDataSharedPath() {
            return LocalDataRootPath.GetChildDirectoryWithName("Shared");
        }

        IAbsoluteDirectoryPath GetLocalSharedDllPath() {
            return AppPath;
        }

        protected virtual IAbsoluteDirectoryPath GetAppPath() {
            return EntryPath;
        }

        protected virtual IAbsoluteDirectoryPath GetDataPath() {
            return GetRoamingRootPath().GetChildDirectoryWithName(Common.AppCommon.ApplicationName);
        }

        protected virtual IAbsoluteDirectoryPath GetLocalDataPath() {
            return LocalDataRootPath.GetChildDirectoryWithName(Common.AppCommon.ApplicationName);
        }

        protected virtual IAbsoluteDirectoryPath GetMyDocumentsPath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToAbsoluteDirectoryPath();
        }

        protected virtual IAbsoluteDirectoryPath GetProgramDataPath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToAbsoluteDirectoryPath();
        }
    }
}