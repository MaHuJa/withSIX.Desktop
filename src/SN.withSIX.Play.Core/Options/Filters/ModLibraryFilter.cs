// <copyright company="SIX Networks GmbH" file="ModLibraryFilter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Caliburn.Micro;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Options.Filters
{
    public class ModLibraryFilter : FilterBase<IContent>
    {
        string _author;
        bool? _isInstalled;
        bool? _isUpdateAvailable;
        int? _maxSize;
        int? _minSize;
        string _name;

        public ModLibraryFilter() {
            if (!Execute.InDesignMode) {
                this.WhenAnyValue(x => x.Name, x => x.IsInstalled, x => x.IsUpdateAvailable,
                    x => x.Author, x => x.MinSize, x => x.MaxSize, (u, v, w, x, y, z) => true)
                    .Subscribe(x => ExecutePublish());
            }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string Author
        {
            get { return _author; }
            set { SetProperty(ref _author, value); }
        }
        public int? MinSize
        {
            get { return _minSize; }
            set { SetProperty(ref _minSize, value); }
        }
        public int? MaxSize
        {
            get { return _maxSize; }
            set { SetProperty(ref _maxSize, value); }
        }
        /*
        public string Category {
            get { return _category; }
            set { SetProperty(ref _category, value); }
        }

        public DateTime? ReleaseDateStart {
            get { return _releaseDateStart; }
            set { SetProperty(ref _releaseDateStart, value); }
        }

        public DateTime? ReleaseDateEnd {
            get { return _releaseDateEnd; }
            set { SetProperty(ref _releaseDateEnd, value); }
        }

        public DateTime? UpdateDateStart {
            get { return _updateDateStart; }
            set { SetProperty(ref _updateDateStart, value); }
        }

        public DateTime? UpdateDateEnd {
            get { return _updateDateEnd; }
            set { SetProperty(ref _updateDateEnd, value); }
        }

        public bool? Compatibility { get; set; }
        public int? MinServers {
            get { return _minServers; }
            set { SetProperty(ref _minServers, value); }
        }

        public int? MinActiveServers {
            get { return _minActiveServers; }
            set { SetProperty(ref _minActiveServers, value); }
        }

        public int? MinMissions {
            get { return _minMissions; }
            set { SetProperty(ref _minMissions, value); }
        }
        */

        public bool? IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
        }
        public bool? IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set { SetProperty(ref _isUpdateAvailable, value); }
        }

        public override bool AnyFilterEnabled() {
            return !string.IsNullOrWhiteSpace(Name) || IsInstalled != null || IsUpdateAvailable != null
                   || !string.IsNullOrWhiteSpace(Author) || MinSize.HasValue || MaxSize.HasValue;
            //  || !string.IsNullOrWhiteSpace(Category) || ReleaseDateStart != null || UpdateDateStart != null || ReleaseDateEnd != null || UpdateDateEnd != null
            // || MinServers != null || MinActiveServers != null || MinMissions != null;
        }

        public override void DefaultFilters() {
            Name = null;
            IsInstalled = null;
            IsUpdateAvailable = null;
            Author = null;
            MinSize = null;
            MaxSize = null;

            base.DefaultFilters();
        }

        public override bool Handler(IContent obj) {
            if (obj == null)
                return false;

            var mod = obj as IMod;
            if (mod != null)
                return ProcessMod(mod);

            var modSet = obj as Collection;
            if (modSet != null)
                return ProcessModSet(modSet);

            return ProcessName(obj)
                   && ProcessAuthor(obj);
        }

        bool ProcessModSet(Collection collection) {
            return ProcessName(collection)
                   && ProcessInstalled(collection)
                   && ProcessUpdated(collection)
                   && ProcessAuthor(collection)
                   && ProcessSize(collection);
        }

        bool ProcessMod(IMod mod) {
            return ProcessName(mod)
                   && ProcessInstalled(mod)
                   && ProcessUpdated(mod)
                   && ProcessAuthor(mod)
                   && ProcessSize(mod);
        }

        bool ProcessUpdated(IMod mod) {
            return IsUpdateAvailable == null ||
                   (IsUpdateAvailable.Value && mod.State == ContentState.UpdateAvailable)
                   || (!IsUpdateAvailable.Value && mod.State != ContentState.UpdateAvailable);
        }

        bool ProcessInstalled(IMod mod) {
            return IsInstalled == null || (IsInstalled.Value && mod.State != ContentState.NotInstalled)
                   || (!IsInstalled.Value && mod.State == ContentState.NotInstalled);
        }

        bool ProcessSize(IMod mod) {
            return (MinSize == null || (mod.SizeWd >= MinSize*FileSizeUnits.MB))
                   && (MaxSize == null || (mod.SizeWd <= MaxSize*FileSizeUnits.MB));
        }

        bool ProcessName(IMod mod) {
            return string.IsNullOrWhiteSpace(Name) || mod.Name.NullSafeContainsIgnoreCase(Name) ||
                   mod.FullName.NullSafeContainsIgnoreCase(Name);
        }

        bool ProcessAuthor(IContent mod) {
            return string.IsNullOrWhiteSpace(Author) || mod.Author.NullSafeContainsIgnoreCase(Author);
        }

        bool ProcessUpdated(Collection collection) {
            return IsUpdateAvailable == null ||
                   (IsUpdateAvailable.Value && collection.State == ContentState.UpdateAvailable)
                   ||
                   (!IsUpdateAvailable.Value && collection.State != ContentState.UpdateAvailable);
        }

        bool ProcessSize(Collection collection) {
            return (MinSize == null || (collection.Size >= MinSize*FileSizeUnits.MB))
                   && (MaxSize == null || (collection.Size <= MaxSize*FileSizeUnits.MB));
        }

        bool ProcessInstalled(Collection collection) {
            return IsInstalled == null || (IsInstalled.Value && collection.State != ContentState.NotInstalled)
                   ||
                   (!IsInstalled.Value && collection.State == ContentState.NotInstalled);
        }

        bool ProcessName(IContent mod) {
            return string.IsNullOrWhiteSpace(Name) || mod.Name.NullSafeContainsIgnoreCase(Name);
        }
    }
}