// <copyright company="SIX Networks GmbH" file="ToggleableModProxy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    // TODO: This can be used to configure the version of a mod per collection?
    [Obsolete("Handle differently?")]
    public sealed class ToggleableModProxy : PropertyChangedBase, IMod, IHaveModel<IContent>, IHaveModel<IMod>,
        IDisposable
    {
        readonly Collection _collection;
        BaseVersion _desiredVersion;
        bool _isEnabled;
        bool _isRequired;
        bool _isVersionLocked;
        int? _order;

        public ToggleableModProxy(IMod content, Collection collection) {
            Model = content;
            _collection = collection;
            CanChangeRequiredState = collection.CanChangeRequiredState;
            Model.PropertyChanged += ModelOnPropertyChanged;
            _isEnabled = GetIsEnabled();
            _isRequired = GetIsRequired();
            _desiredVersion = _collection.GetDesiredModVersion(this);
            _isVersionLocked = GetIsVersionLocked();
            var cc = collection as CustomCollection;
            IsVersionReadOnly = content is LocalMod || content is CustomRepoMod || collection is SubscribedCollection ||
                                cc != null && !cc.AllowChanges();

            var versionObservable = this.WhenAnyValue(x => x.DesiredVersion)
                .Skip(1);

            // TODO: This is horrible and is the reason why selecting a collection must activate the collection currently
            // Package should instead be dealth with on the ToggleableModProxy level, but would require quite extensive changes..
            versionObservable
                .Where(x => DomainEvilGlobal.SelectedGame.ActiveGame.CalculatedSettings.Collection == collection)
                .Subscribe(SetPackage);

            _order = _collection.GetOrder(this);
            if (cc != null) {
                versionObservable.Subscribe(
                    x => cc.SetVersion(Name, x == null || x is SpecializedDependency ? null : x.VersionData));
                this.WhenAnyValue(x => x.Order)
                    .Skip(1)
                    .Subscribe(x => cc.SetOrder(this, x));
                this.WhenAnyValue(x => x.IsRequired)
                    .Skip(1)
                    .Subscribe(x => cc.SetRequired(this, x));
            }
        }

        public bool CanChangeRequiredState { get; private set; }
        public bool IsVersionReadOnly { get; private set; }
        public BaseVersion DesiredVersion
        {
            get { return _desiredVersion; }
            set { SetProperty(ref _desiredVersion, value); }
        }
        public int? Order
        {
            get { return _order; }
            set { SetProperty(ref _order, value); }
        }
        public bool IsRequired
        {
            get { return _isRequired; }
            set { SetProperty(ref _isRequired, value); }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                if (value)
                    _collection.EnableMod(Name);
                else
                    _collection.DisableMod(Name);
                OnPropertyChanged();
            }
        }
        public bool IsVersionLocked
        {
            get { return _isVersionLocked; }
            set { SetProperty(ref _isVersionLocked, value); }
        }

        public void Dispose() {
            Dispose(true);
        }

        IContent IHaveModel<IContent>.Model
        {
            get { return Model; }
        }
        public IMod Model { get; }
        public ModController Controller
        {
            get { return Model.Controller; }
        }

        public int GetMaxThreads() {
            return Model.GetMaxThreads();
        }

        public DateTime UpdatedVersion
        {
            get { return Model.UpdatedVersion; }
            set { Model.UpdatedVersion = value; }
        }
        public IAbsoluteDirectoryPath CustomPath
        {
            get { return Model.CustomPath; }
            set { Model.CustomPath = value; }
        }
        public GameModType Type
        {
            get { return Model.Type; }
            set { Model.Type = value; }
        }
        public string CppName
        {
            get { return Model.CppName; }
            set { Model.CppName = value; }
        }
        public string FullName
        {
            get { return Model.FullName; }
            set { Model.FullName = value; }
        }
        public string MinBuild
        {
            get { return Model.MinBuild; }
            set { Model.MinBuild = value; }
        }
        public string MaxBuild
        {
            get { return Model.MaxBuild; }
            set { Model.MaxBuild = value; }
        }
        public string Path
        {
            get { return Model.Path; }
        }
        public Guid GameId
        {
            get { return Model.GameId; }
            set { Model.GameId = value; }
        }
        public string Guid
        {
            get { return Model.Guid; }
            set { Model.Guid = value; }
        }
        public bool HasLicense
        {
            get { return Model.HasLicense; }
            set { Model.HasLicense = value; }
        }
        public string ModVersion
        {
            get { return Model.ModVersion; }
            set { Model.ModVersion = value; }
        }
        public long Size
        {
            get { return Model.Size; }
            set { Model.Size = value; }
        }
        public long SizeWd
        {
            get { return Model.SizeWd; }
            set { Model.SizeWd = value; }
        }
        public string[] Aliases
        {
            get { return Model.Aliases; }
            set { Model.Aliases = value; }
        }
        public string[] Dependencies
        {
            get { return Model.Dependencies; }
            set { Model.Dependencies = value; }
        }
        public Uri[] Mirrors
        {
            get { return Model.Mirrors; }
            set { Model.Mirrors = value; }
        }
        public List<Network> Networks
        {
            get { return Model.Networks; }
            set { Model.Networks = value; }
        }
        public Userconfig UserConfig
        {
            get { return Model.UserConfig; }
        }
        public string UserConfigChecksum
        {
            get { return Model.UserConfigChecksum; }
            set { Model.UserConfigChecksum = value; }
        }

        public string GetSerializationString() {
            return Model.GetSerializationString();
        }

        public Guid[] GetGameRequirements() {
            return Model.GetGameRequirements();
        }

        public void OpenChangelog() {
            Model.OpenChangelog();
        }

        public bool CompatibleWith(ISupportModding game) {
            return Model.CompatibleWith(game);
        }

        public string GetUrl(string type = "changelog") {
            return Model.GetUrl(type);
        }

        public void OpenReadme() {
            Model.OpenReadme();
        }

        public void OpenLicense() {
            Model.OpenLicense();
        }

        public void OpenHomepage() {
            Model.OpenHomepage();
        }

        public bool GameMatch(ISupportModding game) {
            return Model.GameMatch(game);
        }

        public bool RequiresAdminRights() {
            return Model.RequiresAdminRights();
        }

        public IEnumerable<IAbsolutePath> GetPaths() {
            return Model.GetPaths();
        }

        public void LoadSettings(ISupportModding game) {
            Model.LoadSettings(game);
        }

        public bool Match(string name) {
            return Model.Match(name);
        }

        public string GetRemotePath() {
            return Model.GetRemotePath();
        }

        public string NoteName
        {
            get { return Model.NoteName; }
        }
        public bool HasNotes
        {
            get { return Model.HasNotes; }
        }
        public string Notes
        {
            get { return Model.Notes; }
            set { Model.Notes = value; }
        }
        public DateTime CreatedAt
        {
            get { return Model.CreatedAt; }
            set { Model.CreatedAt = value; }
        }
        public DateTime? UpdatedAt
        {
            get { return Model.UpdatedAt; }
            set { Model.UpdatedAt = value; }
        }

        public bool ComparePK(object other) {
            return ((IComparePK<IMod>) Model).ComparePK(other);
        }

        public bool ComparePK(IMod other) {
            return Model.ComparePK(other);
        }

        public bool ComparePK(SyncBase other) {
            return Model.ComparePK(other);
        }

        public string[] Categories
        {
            get { return Model.Categories; }
            set { Model.Categories = value; }
        }
        public Guid Id
        {
            get { return Model.Id; }
        }
        bool IContentEngineContent.IsInstalled => Model.IsInstalled;
        public IAbsoluteDirectoryPath PathInternal => Model.PathInternal;
        public Guid NetworkId
        {
            get { return Model.NetworkId; }
            set { Model.NetworkId = value; }
        }
        public string HomepageUrl
        {
            get { return Model.HomepageUrl; }
            set { Model.HomepageUrl = value; }
        }
        public bool HasImage
        {
            get { return Model.HasImage; }
            set { Model.HasImage = value; }
        }
        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }
        public string Image
        {
            get { return Model.Image; }
            set { Model.Image = value; }
        }
        public string ImageLarge
        {
            get { return Model.ImageLarge; }
            set { Model.ImageLarge = value; }
        }
        public string Version
        {
            get { return Model.Version; }
            set { Model.Version = value; }
        }
        public bool IsInCurrentCollection
        {
            get { return Model.IsInCurrentCollection; }
            set { Model.IsInCurrentCollection = value; }
        }
        public string DisplayName
        {
            get { return Model.DisplayName; }
        }
        public string Revision
        {
            get { return Model.Revision; }
            set { Model.Revision = value; }
        }
        public string Author
        {
            get { return Model.Author; }
            set { Model.Author = value; }
        }
        public string Description
        {
            get { return Model.Description; }
            set { Model.Description = value; }
        }
        public bool IsCustomContent
        {
            get { return Model.IsCustomContent; }
        }

        public Uri ProfileUrl() {
            return Model.ProfileUrl();
        }

        public Uri GetChangelogUrl() {
            return Model.GetChangelogUrl();
        }

        public ContentState State
        {
            get { return Model.State; }
            set { Model.State = value; }
        }
        public bool IsFavorite
        {
            get { return Model.IsFavorite; }
            set { Model.IsFavorite = value; }
        }
        public int SearchScore
        {
            get { return Model.SearchScore; }
            set { Model.SearchScore = value; }
        }

        public void ToggleFavorite() {
            Model.ToggleFavorite();
        }

        public ReactiveList<IHierarchicalLibraryItem> Children
        {
            get { return Model.Children; }
        }
        public ICollectionView ChildrenView
        {
            get { return Model.ChildrenView; }
        }
        public IHierarchicalLibraryItem SelectedItem
        {
            get { return Model.SelectedItem; }
            set { Model.SelectedItem = value; }
        }
        public ObservableCollection<object> SelectedItemsInternal
        {
            get { return Model.SelectedItemsInternal; }
            set { Model.SelectedItemsInternal = value; }
        }

        public void ClearSelection() {
            Model.ClearSelection();
        }

        object IHaveSelectedItem.SelectedItem
        {
            get { return Model.SelectedItem; }
            set { Model.SelectedItem = (IHierarchicalLibraryItem) value; }
        }
        public ICollectionView ItemsView
        {
            get { return Model.ItemsView; }
        }

        public void SetPackage() {
            SetPackage(DesiredVersion);
        }

        void SetPackage(BaseVersion desiredVersion) {
            if (Model.Controller.Package == null)
                return;
            Model.Controller.Package.UpdateLocalDependency(desiredVersion);
        }

        public string GetDesiredOrGlobal() {
            // But why do we get this weird data in the first place?!
            var versionData = GetDesiredOrGlobalInternal();
            return string.IsNullOrWhiteSpace(versionData) ? null : versionData;
        }

        string GetDesiredOrGlobalInternal() {
            return DesiredVersion == null || DesiredVersion is GlobalDependency
                ? GetVersionConstraint(GetGlobalDesiredVersion())
                : GetVersionConstraint(DesiredVersion.VersionData);
        }

        string GetGlobalDesiredVersion() {
            return DomainEvilGlobal.Settings.GameOptions.GetDesiredPackageVersion(Name);
        }

        static string GetVersionConstraint(string desired) {
            switch (desired) {
            case null:
                //return VersionKeyWords.Latest;
                return null;
            case VersionKeyWords.Latest:
                return null;
            case VersionKeyWords.LatestInclPreRelease:
                return null;
            //return VersionKeyWords.LatestInclPreRelease; // TODO: Allow this configuration
            default:
                return desired;
            }
        }

        bool GetIsEnabled() {
            return !_collection.DisabledItems.Contains(Name, StringComparer.CurrentCultureIgnoreCase);
        }

        bool GetIsRequired() {
            return _collection.RequiredMods.Contains(Name, StringComparer.CurrentCultureIgnoreCase)
                //|| Collection.RequiredMods.Contains(CppName, StringComparer.CurrentCultureIgnoreCase)
                   || Aliases.Any(x => _collection.RequiredMods.Contains(x));
        }

        void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            OnPropertyChanged(propertyChangedEventArgs.PropertyName);
        }

        public void RefreshInfo() {
            IsRequired = GetIsRequired();
            if (IsRequired && !IsEnabled)
                IsEnabled = true;
        }

        void Dispose(bool disposing) {
            Model.PropertyChanged -= ModelOnPropertyChanged;
        }

        ~ToggleableModProxy() {
            Dispose(false);
        }

        public void Lock() {
            DesiredVersion = Model.Controller.Package.GetLatestDependency();
            IsVersionLocked = true;
        }

        public void Unlock() {
            DesiredVersion = new GlobalDependency(Name.ToLower());
            IsVersionLocked = false;
        }

        bool GetIsVersionLocked() {
            return !(DesiredVersion is SpecializedDependency);
        }

        public void ToggleEnabled() {
            IsEnabled = !IsEnabled;
        }
    }
}