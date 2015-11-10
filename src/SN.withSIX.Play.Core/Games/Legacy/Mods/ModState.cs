// <copyright company="SIX Networks GmbH" file="ModState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using YamlDotNet.Core;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public class ModState : IEnableLogging
    {
        readonly IMod _mod;
        bool _isValidSixSync;
        bool _isValidSixSyncPack;
        PackageItem _package;
        IAbsoluteFilePath _repoConfigYamlFile;
        IAbsoluteDirectoryPath _repoPath;
        IAbsoluteFilePath _repoYamlFile;
        IAbsoluteDirectoryPath _rootPath;
        bool _sixSyncRepoExists;
        RepoVersion _sixSyncVersionInfo;
        public string Guid;

        public ModState(IMod mod) {
            _mod = mod;
            if (_mod is LocalMod)
                State = ContentState.Local;
        }

        public IAbsoluteDirectoryPath Path { get; private set; }
        public bool Exists { get; private set; }
        public ContentState State { get; private set; }
        public string DesiredRevision { get; private set; }
        public string Revision { get; private set; }
        public string LatestRevision { get; private set; }

        public void UpdateLocalModState(ISupportModding game) {
            _package = null;
            SetSharedState(game);
        }

        public void UpdateSynqState(ISupportModding game, PackageItem package) {
            Contract.Requires<ArgumentNullException>(package != null);
            _package = package;
            SetSharedState(game);
            _isValidSixSync = false;
            _isValidSixSyncPack = false;
            _sixSyncRepoExists = false;
            _sixSyncVersionInfo = null;
            _package.UpdateCurrentVersion();
            Revision = GetSynqRevision();
            DesiredRevision = GetSynqDesiredRevision();
            LatestRevision = GetSynqLatestRevision();
            State = !ModMatchesActiveGame(game) ? ContentState.Incompatible : GetSynqModState();
            //Common.App.Mediator.Notify(new ModInfoChangedEvent(new ModInfo(_mod)));
        }

        string GetSynqLatestRevision() {
            return _package.GetLatestAnyDependency().VersionData;
        }

        public void UpdateSixSyncState(ISupportModding game) {
            SetSharedState(game);
            SetSixSyncPaths();
            _sixSyncRepoExists = DoesRepoExist();
            _isValidSixSync = IsValidSixSync();
            _isValidSixSyncPack = IsPackValidSixSync();
            _sixSyncVersionInfo = _isValidSixSync ? TryReadRepoFile(_repoYamlFile) : null;
            try {
                DesiredRevision = _mod.Version;
                LatestRevision = _mod.Version;
                Guid = GetSixSyncGuid();
                Revision = GetSixSyncRevision(false);
                State = !ModMatchesActiveGame(game) ? ContentState.Incompatible : GetSixSyncModState();
            } finally {
                _sixSyncVersionInfo = null;
            }
        }

        void SetSharedState(ISupportModding game) {
            SetPaths(game);
            SetExistsState();
        }

        bool ModMatchesActiveGame(ISupportModding game) {
            return game != null && _mod.GameMatch(game);
        }

        void SetExistsState() {
            var path = Path;
            Exists = path != null && path.Exists;
        }

        IAbsoluteDirectoryPath GetPackPath() {
            var packPath = TryReadRepoConfig().PackPath;
            return
                (!String.IsNullOrWhiteSpace(packPath) ? packPath.ToAbsoluteDirectoryPath() : _repoPath)
                    .GetChildDirectoryWithName(Repository.PackFolderName);
        }

        public bool DoesRepoExist() {
            return _repoPath != null && _repoPath.Exists;
        }

        bool IsValidSixSync() {
            return _sixSyncRepoExists && _repoYamlFile.Exists;
        }

        bool IsPackValidSixSync() {
            return _sixSyncRepoExists && _repoConfigYamlFile.Exists && RepoPackYaml().Exists;
        }

        IAbsoluteFilePath RepoPackYaml() {
            return GetPackPath().GetChildFileWithName(Repository.VersionFileName);
        }

        RepoConfig TryReadRepoConfig() {
            try {
                return YamlExtensions.NewFromYamlFile< RepoConfig>(_repoConfigYamlFile);
            } catch (YamlParseException e) {
                this.Logger().FormattedWarnException(e);
                return new RepoConfig();
            } catch (YamlException e) {
                this.Logger().FormattedWarnException(e);
                return new RepoConfig();
            } catch (YamlExpectedOtherNodeTypeException e) {
                this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoConfig();
            }
        }

        string GetSixSyncGuid(bool defaultIfInvalid = false) {
            if (!_isValidSixSync || _sixSyncVersionInfo == null)
                return defaultIfInvalid ? _mod.Guid : null;

            return _sixSyncVersionInfo.Guid;
        }

        string GetSynqRevision() {
            var cv = _package.CurrentVersion;
            return cv == null ? null : cv.VersionData;
        }

        string GetSixSyncRevision(bool defaultIfInvalid) {
            if (!_isValidSixSync || _sixSyncVersionInfo == null)
                return defaultIfInvalid ? _mod.Version : null;

            return _sixSyncVersionInfo.Version.ToString();
        }

        string GetSixSyncPackRevision(bool defaultIfInvalid = false) {
            return _isValidSixSyncPack
                ? TryReadRepoFile(RepoPackYaml()).Version.ToString()
                : (defaultIfInvalid ? _mod.Version : null);
        }

        bool VersionMatch() {
            var localVersion = Revision;
            if (String.IsNullOrWhiteSpace(localVersion))
                return false;
            return localVersion == _mod.Version;
        }

        bool GuidMatch() {
            var localGuid = Guid;
            if (String.IsNullOrWhiteSpace(localGuid))
                return false;
            return localGuid == _mod.Guid;
        }

        ContentState GetSixSyncModState() {
            if (!Exists)
                return ContentState.NotInstalled;
            if (!VersionMatch() || !GuidMatch()
                || !WdAndPackVersionMatch())
                return ContentState.UpdateAvailable;
            return ContentState.Uptodate;
        }

        ContentState GetSynqModState() {
            var packageVersion = _package.CurrentVersion;
            if (packageVersion == null)
                return !Exists ? ContentState.NotInstalled : ContentState.Unverified;
            var d = _package.ActualDependency;
            if (d != null && !d.ComparePK(packageVersion))
                return ContentState.UpdateAvailable;
            return ContentState.Uptodate;
        }

        bool WdAndPackVersionMatch() {
            return GetSixSyncPackRevision() == GetSixSyncRevision(false);
        }

        string GetSynqDesiredRevision() {
            var d = _package.ActualDependency;
            return d == null ? null : d.VersionData;
        }

        IAbsoluteDirectoryPath GetModRootPath(ISupportModding game) {
            return _mod.CustomPath ?? game.ModPaths.Path;
        }

        void SetPaths(ISupportModding game) {
            _rootPath = GetModRootPath(game);
            Path = _rootPath != null ? _rootPath.GetChildDirectoryWithName(_mod.Name) : null;
        }

        void SetSixSyncPaths() {
            var path = Path;
            _repoPath = path == null ? null : path.GetChildDirectoryWithName(Repository.RepoFolderName);
            _repoYamlFile = _repoPath == null ? null : _repoPath.GetChildFileWithName(Repository.VersionFileName);
            _repoConfigYamlFile = _repoPath == null
                ? null
                : _repoPath.GetChildFileWithName(Repository.ConfigFileName);
        }

        RepoVersion TryReadRepoFile(IAbsoluteFilePath path) {
            try {
                return YamlExtensions.NewFromYamlFile< RepoVersion>(path);
            } catch (YamlParseException e) {
                this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            } catch (YamlException e) {
                this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            } catch (YamlExpectedOtherNodeTypeException e) {
                this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            }
        }
    }

    [Obsolete("Not used by Web anyway!")]
    public class ModInfoChangedEvent
    {
        public ModInfoChangedEvent(IModInfo modInfo) {
            ModInfo = modInfo;
        }

        public IModInfo ModInfo { get; private set; }
    }

    public class ModInfo : IModInfo
    {
        public ModInfo(IMod mod) {
            InstalledVersion = mod.Controller.Revision;
            Id = mod.Id;
            State = mod.State.ToString();
            Name = mod.FullName;
        }

        public string Name { get; set; }
        public string InstalledVersion { get; set; }
        public Guid Id { get; set; }
        public string State { get; set; }
    }

    public interface IModInfo
    {
        string InstalledVersion { get; set; }
        Guid Id { get; set; }
        string State { get; set; }
        string Name { get; set; }
    }
}