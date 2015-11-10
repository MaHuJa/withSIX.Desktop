// <copyright company="SIX Networks GmbH" file="Mission.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Validators;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Missions
{
    public static class MissionFolders
    {
        public const string SpMissions = "missions";
        public const string MpMissions = "mpmissions";
    }

    public static class MissionTypes
    {
        public const string SpMission = "Mission";
        public const string MpMission = "MpMission";
    }

    public static class MissionTypesHunan
    {
        public const string SpMission = "SP Mission";
        public const string MpMission = "MP Mission";
    }

    public static class MissionAdditionalKeys
    {
        public const string SpMission = "mission_sp";
        public const string MpMission = "mission_mp";
    }

    [DoNotObfuscate]
    public class Mission : MissionBase, IComparePK<Mission>
    {
        public static readonly string ApiPath = "missions";
        string _fileName;
        string _gameMode;
        bool _isInstalled;
        string _md5;
        int _minPlayers;
        int _slots;
        string[] _tags = new string[0];
        string[] _types;
        int _userId;
        public Mission(Guid id) : base(id) {}
        public override bool IsLocal
        {
            get { return false; }
        }
        public string[] Types
        {
            get { return _types; }
            set { SetProperty(ref _types, value); }
        }
        public string[] Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }
        public bool RequiresMods { get; set; }
        public bool PlayerIsLeader { get; set; }
        public bool FriendlyAi { get; set; }
        public DateTime MissionDate { get; set; }
        public string Weather { get; set; }
        public string GameVersion { get; set; }
        public string Location { get; set; }
        public string Factions { get; set; }
        public string MissionSize { get; set; }
        public virtual string RestPath
        {
            get { return ApiPath; }
        }
        public string GameMode
        {
            get { return _gameMode; }
            set { SetProperty(ref _gameMode, value); }
        }
        public int Slots
        {
            get { return _slots; }
            set { SetProperty(ref _slots, value); }
        }
        public int UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }
        public int MinPlayers
        {
            get { return _minPlayers; }
            set { SetProperty(ref _minPlayers, value); }
        }
        public string MissionVersion
        {
            set { Version = value; }
        }
        public override string NoteName
        {
            get { return _fileName; }
        }
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }
        public string Md5
        {
            get { return _md5; }
            set { SetProperty(ref _md5, value); }
        }
        public Guid GameId { get; set; }
        public DateTime? LastPlayed { get; set; }
        public DateTime? UpdatedVersion
        {
            get { return UpdatedAt; }
        }
        public string LastPlayedAgo
        {
            get { return LastPlayed.HasValue ? LastPlayed.Value.Ago() : "Never"; }
        }
        public override string ObjectTag
        {
            get { return Id.ToString(); }
        }
        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
        }
        public string PackageName { get; set; }
        public GameMissionType ContentType { get; set; }
        public Game Game { get; set; }

        public override bool ComparePK(object obj) {
            var emp = obj as Mission;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }

        public bool ComparePK(Mission other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;

            if (other.FileName == default(string) || FileName == default(string))
                return false;
            return other.FileName.Equals(FileName);
        }

        public override Uri ProfileUrl() {
            if (IsLocal)
                throw new NotSupportedException("Local missions have no profile");
            return base.ProfileUrl();
        }

        public override string CombinePath(string path, bool inclFileName = true) {
            var fullPath = Path.Combine(path, PathType());
            return !inclFileName ? fullPath : CombineFullPath(fullPath);
        }

        public void PublishByEmail(string path) {
            Tools.Generic.TryOpenUrl("mailto:?subject=Mission File {0}, sent from Play withSIX&attach=path");
        }

        public bool GameMatch(Game game) {
            return game.Id == GameId;
        }

        public override string CombineFullPath(string fullPath) {
            return Path.Combine(fullPath, FileName);
        }

        protected override string GetGameSlug() {
            return Game.MetaData.Slug;
        }

        protected override string GetSlugType() {
            return "missions";
        }

        public string GetApiPath() {
            return ApiPath;
        }

        public static string NiceMissionName(string x) {
            return x.Replace("_", " ").Replace("%20", " ").UppercaseFirst();
        }

        public static string ValidMissionName(string x) {
            return FileNameValidator.ReplaceInvalidCharacters(x.Replace("%20", " "));
        }
    }

    public class LocalMission : Mission
    {
        public LocalMission(Guid id) : base(id) {}
        public override bool IsLocal
        {
            get { return true; }
        }
    }
}