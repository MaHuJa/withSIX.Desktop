// <copyright company="SIX Networks GmbH" file="ISelfUpdater.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Core.Applications.Services
{
    public interface ISelfUpdater
    {
        ITransferProgress Status { get; }
        IAbsoluteFilePath Destination { get; }
        string RemoteVersion { get; }
        bool NewVersionDownloaded { get; }
        IAbsoluteDirectoryPath InstallPath { get; }
        bool IsRunning { get; }
        bool ExistsAndIsValid(IAbsoluteFilePath exePath);
        Task<bool> ProgramApplyUpdateIfExists(IAbsoluteFilePath exePath);
        Task CheckForUpdate();
        bool PerformSelfUpdate(string action = SelfUpdaterCommands.UpdateCommand, params string[] args);
        Version GetLocalVersion();
        bool IsInstalled();
        bool IsLegacyInstalled();
        bool ApplyInstallIfRequired(IAbsoluteFilePath exePath);
        bool IsLocalUpdate(string exePath);
        bool UninstallSingleExe();
    }
}