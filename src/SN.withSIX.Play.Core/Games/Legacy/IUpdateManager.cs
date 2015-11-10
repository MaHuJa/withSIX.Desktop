// <copyright company="SIX Networks GmbH" file="IUpdateManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core.Legacy.Status;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public interface IActionState
    {
        bool IsActionEnabled { get; set; }
        string ActionText { get; set; }
        ActionStatus ActionState { get; set; }
        StatusMod ActiveStatusMod { get; }
        int ProgressBarVisiblity { get; set; }
        string ActionWarningMessage { get; set; }
    }

    public enum ActionStatus
    {
        Disabled,
        Update,
        Play,
        NoGameFound,
        Diagnose
    }


    public interface IModsState
    {
        IList<UpdateState> ModUpdates { get; set; }
        OverallUpdateState State { get; set; }
        bool IsUpdateNeeded { get; }
    }

    public interface IUpdateManager : IActionState, IModsState
    {
        IObservable<Tuple<Collection, ContentState>> StateChange { get; }
        Task Play();
        Task HandleConvertOrInstallOrUpdate(bool force = false);
        Task HandleUninstall();
        void RefreshModInfo();
        void Terminate();
        Task DownloadMission(Mission mission);
        Task ProcessRepoMpMissions();
        Task ProcessRepoMissions();
        Task ProcessRepoApps();
        Task MoveModFoldersIfValidAndExists(IAbsoluteDirectoryPath sourcePath, IAbsoluteDirectoryPath destinationPath);
        Task MovePathIfValidAndExists(IAbsoluteDirectoryPath sourcePath, IAbsoluteDirectoryPath destinationPath);
        Task PreGameLaunch();
    }
}