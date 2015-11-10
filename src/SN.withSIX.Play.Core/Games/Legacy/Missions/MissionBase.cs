// <copyright company="SIX Networks GmbH" file="MissionBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Legacy.Missions
{
    public abstract class MissionBase : HostedContent, IRecent, IHaveType<string>
    {
        string _fullName;
        bool? _hasNotes;
        bool? _isFavorite;
        string _island;
        long _size;
        long _sizePacked;
        string _type;

        protected MissionBase(Guid id) : base(id) {
            Controller = new MissionController(this);
            Categories = new[] {Common.DefaultCategory};
        }

        public IAbsoluteDirectoryPath CustomPath { get; set; }
        public MissionController Controller { get; }
        public string FullName
        {
            get { return _fullName; }
            set { SetProperty(ref _fullName, value); }
        }
        public override bool IsFavorite
        {
            get
            {
                return _isFavorite == null
                    ? (bool) (_isFavorite = DomainEvilGlobal.Settings.MissionOptions.IsFavorite(this))
                    : (bool) _isFavorite;
            }
            set
            {
                if (_isFavorite == value)
                    return;
                if (value)
                    DomainEvilGlobal.Settings.MissionOptions.AddFavorite(this);
                else
                    DomainEvilGlobal.Settings.MissionOptions.RemoveFavorite(this);

                _isFavorite = value;
                OnPropertyChanged();
            }
        }
        public long Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }
        public long SizePacked
        {
            get { return _sizePacked; }
            set { SetProperty(ref _sizePacked, value); }
        }
        public override string Notes
        {
            get { return DomainEvilGlobal.NoteStorage.GetNotes(this); }
            set
            {
                DomainEvilGlobal.NoteStorage.SetNotes(this, value);
                _hasNotes = !string.IsNullOrEmpty(value);
                new[] {"Notes", "HasNotes"}.ForEach(OnPropertyChanged);
            }
        }
        public override bool HasNotes
        {
            get
            {
                if (_hasNotes != null)
                    return (bool) _hasNotes;
                _hasNotes = DomainEvilGlobal.NoteStorage.HasNotes(this);
                return (bool) _hasNotes;
            }
        }
        public string Island
        {
            get { return _island; }
            set { SetProperty(ref _island, value); }
        }
        public abstract bool IsLocal { get; }
        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
        public abstract string ObjectTag { get; }
        public DateTime? LastUsed
        {
            get
            {
                var recent = DomainEvilGlobal.Settings.MissionOptions.RecentMissions
                    .OrderByDescending(x => x.On)
                    .FirstOrDefault(x => x.Matches(this));
                if (recent == null)
                    return null;

                return recent.On;
            }
        }
        // TODO: THIS NEEDS OPTIMIZATION. WE DONT WANT TO READ FROM DISK HERE, and deifnitely dont want to download!!
        // Low prio because missions are really not used that much atm...
        public IEnumerable<IMod> GetMods(ISupportModding modding, IContentManager manager) {
            var controller = Controller;
            var package = controller.GetPackageIfAvailable2();
            if (package == null)
                return Enumerable.Empty<IMod>();

            var mods = package.Dependencies.Keys;
            return mods.Any()
                ? manager.GetMods(modding, mods)
                : Enumerable.Empty<IMod>();
        }

        public abstract string CombinePath(string getDocumentsGamePath, bool inclFolderName = true);

        string CombineCustomPath() {
            return CombineFullPath(CustomPath.ToString());
        }

        public void OpenInExplorer() {
            var path = CombineCustomPath();
            if (File.Exists(path) || Directory.Exists(path))
                Tools.FileUtil.SelectInExplorer(path);
            else {
                var parent = Directory.GetParent(path).FullName;
                if (!Directory.Exists(parent))
                    return;
                Tools.FileUtil.OpenFolderInExplorer(parent);
            }
        }

        public override bool ComparePK(object obj) {
            var emp = obj as MissionBase;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }

        public string PathType() {
            return Type == MissionTypes.MpMission ? MissionFolders.MpMissions : MissionFolders.SpMissions;
        }

        public void RefreshLastJoinedOn() {
            OnPropertyChanged("LastUsed");
        }

        public virtual bool ComparePK(MissionBase obj) {
            if (obj == null)
                return false;

            if (obj is MissionFolder && this is Mission)
                return false;
            if (obj is Mission && this is MissionFolder)
                return false;

            var otherKey = obj.ObjectTag;
            if (otherKey == null)
                return false;

            return otherKey.Equals(ObjectTag);
        }

        public bool Matches(FavoriteMission other) {
            return other != null && other.Matches(this);
        }

        public abstract string CombineFullPath(string fullPath);
    }
}