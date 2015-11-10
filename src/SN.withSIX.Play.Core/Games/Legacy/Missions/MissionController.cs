// <copyright company="SIX Networks GmbH" file="MissionController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core.Packages.Internals;

namespace SN.withSIX.Play.Core.Games.Legacy.Missions
{
    public class MissionController : ContentController
    {
        ISupportMissions _game;

        public MissionController(MissionBase mission) : base(mission) {
            Mission = mission;
            if (Mission.IsLocal)
                Model.State = ContentState.Local;
        }

        public MissionBase Mission { get; }
        public IAbsoluteDirectoryPath Path { get; private set; }

        public IAbsolutePath GetMissionFile() {
            var missionFolder = Mission as MissionFolder;
            if (missionFolder != null)
                return missionFolder.CustomPath.GetChildDirectoryWithName(missionFolder.FolderName);

            var path = Path;
            if (path == null)
                return null;

            var missionFile = Mission as Mission;
            if (missionFile != null && missionFile.IsLocal)
                return path.GetChildFileWithName(missionFile.FileName);

            var package = GetPackageIfAvailable2();
            if (package == null)
                return null;
            var fileName =
                package.Files.Keys.FirstOrDefault(
                    x =>
                        x.EndsWith(".pbo", StringComparison.InvariantCultureIgnoreCase) ||
                        x.EndsWith(".ifa", StringComparison.InvariantCultureIgnoreCase));
            return fileName == null
                ? null
                : path.GetChildFileWithName(fileName);
        }

        public bool IsUpdateRequired() {
            if (Mission.IsLocal)
                return false;

            UpdateInfo();

            return Model.State != ContentState.Uptodate;
        }

        void UpdateInfo() {
            Revision = GetVersion();
            Mission.Version = Revision;
            DesiredRevision = GetDesiredRevision();
            Model.State = GetState();
        }

        public override sealed UpdateState CreateUpdateState() {
            return new UpdateState(Mission) {
                CurrentRevision = Revision,
                Revision = DesiredRevision,
                Size = (long) (Mission.Size*1024.0)
            };
        }

        public bool HasMultipleVersions() {
            return Package != null && Package.ActualPackages.Count > 1;
        }

        public PackageMetaData GetPackageIfAvailable2() {
            return Package == null ? null : GetPackageIfAvailable();
        }

        PackageMetaData GetPackageIfAvailable() {
            Package.UpdateCurrentIfAvailable();
            return Package.Current;
        }

        bool GetIsInstalled() {
            var missionFileName = GetMissionFile();
            return missionFileName != null && missionFileName.Exists;
        }

        bool IsUpToDate() {
            var mission = Mission as Mission;
            if (mission == null)
                return true;
            if (!GetIsInstalled())
                return false;
            var rev = Revision;
            return rev != null && rev.Equals(DesiredRevision);
        }

        string GetVersion() {
            var mission = Mission as Mission;
            if (mission == null)
                return null;
            var package = GetInstalledPackage(mission);
            return package == null ? null : package.VersionData;
        }

        string GetDesiredRevision() {
            if (Package == null)
                return Mission.Version;
            var d = Package.ActualDependency;
            return d == null ? null : d.VersionData;
        }

        ContentState GetState() {
            if (Mission.IsLocal)
                return ContentState.Local;

            if (!GetIsInstalled())
                return ContentState.NotInstalled;
            return !IsUpToDate() ? ContentState.UpdateAvailable : ContentState.Uptodate;
        }

        SpecificVersion GetInstalledPackage(Mission mission) {
            var path = Path;
            if (path == null)
                return null;
            return
                Sync.Core.Packages.Package.GetInstalledPackages(
                    System.IO.Path.Combine(path.ToString(), Sync.Core.Packages.Package.SynqInfoFile)
                        .ToAbsoluteFilePath())
                    .FirstOrDefault(x => x.Name.Equals(mission.PackageName));
        }

        IAbsoluteDirectoryPath GetPath() {
            var gp = _game.InstalledState.Directory;
            return gp == null ? null : gp.GetChildDirectoryWithName(Mission.PathType());
        }

        public void UpdateState(ISupportMissions game) {
            _game = game;
            Path = GetPath();
            UpdateInfo();
        }
    }
}