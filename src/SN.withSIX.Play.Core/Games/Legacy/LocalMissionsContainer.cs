// <copyright company="SIX Networks GmbH" file="LocalMissionsContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using MoreLinq;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DataContract(Name = "LocalMissions",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class LocalMissionsContainer : LocalContainerBase<IContent>, IDisposable, IEnableLogging
    {
        bool _disposed;
        FileSystemWatcher _fsw;
        public LocalMissionsContainer() {}
        public LocalMissionsContainer(string name, string path, Game game) : base(name, path, game) {}

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override void FillLocalLibrary(string path) {
            Path = path;
            FillLocalLibrary();
        }

        public override void FillLocalLibrary() {
            DisposeFsw();
            RefreshLocalLibrary();
            SetupFolderChangeTracking();
        }

        protected override void RefreshLocalLibrary() {
            Items.Clear();
            var path = Path;
            if (path == null || !Directory.Exists(path))
                return;
            TryRefreshLocalLibrary(path);
        }

        void TryRefreshLocalLibrary(string path) {
            try {
                Items.AddRange(CreateLocalMissionsFromGameBasedFolder(path));
            } catch (IOException e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        void SetupFolderChangeTracking() {
            var path = Path;
            if (path == null || !Directory.Exists(path))
                return;

            _fsw = new FileSystemWatcher((path.EndsWith(@"\") ? path : (path + "\\"))) {
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = true
            };

            _fsw.Changed += OnChanged;
            _fsw.Created += OnChanged;
            _fsw.Deleted += OnChanged;
            _fsw.Renamed += OnRenamed;

            _fsw.EnableRaisingEvents = true;
        }

        void OnChanged(object sender, FileSystemEventArgs args) {
            if (args.FullPath.ToLower().Contains(Repository.DefaultRepoRootDirectory))
                return;
            RefreshLocalLibrary();
        }

        void OnRenamed(object sender, RenamedEventArgs args) {
            if (args.FullPath.ToLower().Contains(Repository.DefaultRepoRootDirectory))
                return;
            RefreshLocalLibrary();
        }

        IEnumerable<MissionBase> CreateLocalMissionsFromGameBasedFolder(string path) {
            if (!Game.InstalledState.IsInstalled)
                return Enumerable.Empty<MissionBase>();
            var missions = GetLocalMissions(System.IO.Path.Combine(path, MissionFolders.SpMissions));
            var mpmissions = GetLocalMissions(System.IO.Path.Combine(path, MissionFolders.MpMissions),
                MissionTypes.MpMission);
            return missions.Concat(mpmissions);
        }

        IEnumerable<MissionBase> GetLocalMissions(string missionsPath, string type = MissionTypes.SpMission) {
            if (!Directory.Exists(missionsPath))
                return Enumerable.Empty<MissionBase>();
            var missions = CalculatedGameSettings.ContentManager.GetLocalMissions(missionsPath);
            missions.ForEach(x => x.Type = type);
            return missions.Select(CreateLocalMission);
        }

        MissionBase CreateLocalMission(MissionBase mission) {
            if (mission is MissionFolder)
                return mission;
            return Game.Lists.Missions.FirstOrDefault(
                y => y.Name.Equals(mission.Name, StringComparison.OrdinalIgnoreCase))
                   ?? mission;
        }

        ~LocalMissionsContainer() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;

            if (disposing) {
                // dispose managed resources
                DisposeFsw();
            }

            // free native resources
            // set large fields to null.
            // call Dispose on your base class if needed
            // base.Dispose(disposing);

            _disposed = true;
        }

        void DisposeFsw() {
            if (_fsw == null)
                return;
            _fsw.EnableRaisingEvents = false;
            _fsw.Dispose();
            _fsw = null;
        }
    }
}