// <copyright company="SIX Networks GmbH" file="ModLibrarySetup.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Caliburn.Micro;
using MoreLinq;
using ReactiveUI;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class ModLibrarySetup : LibrarySetup<ContentLibraryItemViewModel>, IHandle<GamePathChanged>,
        IHandle<ModPathChanged>,
        IDisposable
    {
        readonly ContentLibraryItemViewModel<BuiltInContentContainer> _aIa;
        readonly IContentManager _contentList;
        readonly Guid[] _desiredModsets = {
            new Guid("37d570f0-e8ba-4eac-b13b-9dedc1841013"),
            new Guid("a6292017-015e-4258-831e-88ef556de692"), new Guid("894b8a60-5387-406c-8b5e-891298e94d30")
        };
        readonly CompositeDisposable _disposables = new CompositeDisposable();
        readonly IEventAggregator _eventBus;
        readonly string[] _featured = {
            "@allinarma", "@task_force_radio", "@bornholm", "@fa18x_black_wasp", "@rhs_usf3", "@rhs_afrf3", "@tacbf",
            "@JSRS2", "@mcc_sandbox",
            "@mcc_sandbox_a3", "@TMR", "@alive", "@blastcore_a3", "@TPW_MODS", "@SpeedOfSound", "@VTS_WeaponResting",
            "@landtex_a3a", "@BaBe_midTex", "@ACRE", "@ACRE2",
            "@AllInArmaHighQualityPack", "@AllInArmaStandaloneLite", "@AllInArmaTerrainPack",
            "@AllInArmaTerrainPackLite"
        };
        readonly Game _game;
        readonly ModLibraryViewModel _library;
        readonly ISupportModding _modding;
        readonly UserSettings _settings;
        public readonly ModLibraryGroupViewModel BuiltInGroup;
        public readonly ModLibraryGroupViewModel CollectionsGroup;
        public readonly ModLibraryGroupViewModel LocalGroup;
        public readonly ModLibraryGroupViewModel OnlineGroup;
        ContentLibraryItemViewModel<BuiltInContentContainer> _builtinFavorites;
        ContentLibraryItemViewModel<BuiltInContentContainer> _builtinFeatured;
        ContentLibraryItemViewModel<BuiltInContentContainer> _builtinRecent;
        bool _disposed;
        LocalModsLibraryItemViewModel[] _gameFolders;

        public ModLibrarySetup(ModLibraryViewModel library, Game game, IContentManager contentList,
            UserSettings settings, IEventAggregator eventBus) {
            Contract.Requires<ArgumentException>(game.SupportsMods());
            _library = library;
            _game = game;
            _modding = game.Modding();
            _settings = settings;
            _eventBus = eventBus;
            _contentList = contentList;
            OnlineGroup = new ModLibraryGroupViewModel(library, "Repositories", "add repo",
                SixIconFont.withSIX_icon_Cloud) {IsRoot = true, SortOrder = 13};
            LocalGroup = new ModLibraryGroupViewModel(library, "Local", "add folder",
                SixIconFont.withSIX_icon_System) {IsRoot = true, SortOrder = 12};
            CollectionsGroup = new ModLibraryGroupViewModel(library, "Collections", "new collection",
                SixIconFont.withSIX_icon_Folder) {IsRoot = true, SortOrder = 11};
            BuiltInGroup = new ModLibraryGroupViewModel(library, null) {IsRoot = true, SortOrder = 0};

            Groups = new LibraryGroupViewModel<ModLibraryViewModel>[] {
                null, BuiltInGroup, CollectionsGroup, LocalGroup,
                OnlineGroup
            };
            _aIa = new BrowseContentLibraryItemViewModel<BuiltInContentContainer>(library,
                new BuiltInContentContainer("AllInArma"), @group: LocalGroup, isFeatured: true);

            SetupItems();
            Items.AddRange(Groups.Where(x => x != null && x != BuiltInGroup));
            CreateItemsView();
            _eventBus.Subscribe(this);
        }

        public LibraryGroupViewModel<ModLibraryViewModel>[] Groups { get; protected set; }
        public ContentLibraryItemViewModel<Collection> Updates { get; set; }
        public NetworkLibraryItemViewModel Network { get; set; }

        ~ModLibrarySetup() {
            Dispose(false);
        }

        void SetupItems() {
            SetupLocalItems();
            SetupOnlineItems();
            SetupBuiltInPlaylist();
            SetupCollections();
        }

        void SetupCollections() {
            SetupCustomCollections();
            SetupSubscribedCollections();

            _contentList.CustomCollections.ChangeTrackingEnabled = true;
            _contentList.SubscribedCollections.ChangeTrackingEnabled = true;

            _disposables.Add(_contentList.CustomCollections.ItemChanged
                .Subscribe(x => {
                    if (x.PropertyName != "GameId")
                        return;

                    if (_game.Id == x.Sender.GameId)
                        CreateCustomModSet(x.Sender);
                    else {
                        var col = GetCustomRepoModSets().FirstOrDefault(m => m.Model == x.Sender);
                        OnlineGroup.Children.RemoveAll(c => c == col);
                    }
                }));
        }

        void SetupCustomCollections() {
            SetupCollectionItems(_contentList.CustomCollections, CollectionsGroup.Children, x => x.GameMatch(_game));
        }

        void SetupSubscribedCollections() {
            SetupCollectionItems(_contentList.SubscribedCollections, CollectionsGroup.Children, x => x.GameMatch(_game));
        }

        void SetupCollectionItems<T>(IReactiveList<T> src, IReactiveList<IHierarchicalLibraryItem> dst,
            Func<T, bool> predicate = null) where T : Collection {
            lock (src) {
                var srcF = predicate == null ? src : src.Where(predicate);
                dst.AddRange(srcF.Select(CreateCustomModSet));
                _disposables.Add(src.TrackChanges(
                    x => dst.Add(CreateCustomModSet(x)),
                    x => dst.Remove(GetCollection(x)),
                    reset => {
                        dst.RemoveRange(GetCollections().ToArray());
                        dst.AddRange(reset.Select(CreateCustomModSet));
                    }, predicate));
            }
        }

        ContentLibraryItemViewModel<Collection> GetCollection(Collection x) {
            return GetCollections().FirstOrDefault(y => y.Model.Equals(x));
        }

        IEnumerable<ContentLibraryItemViewModel<Collection>> GetCollections() {
            return CollectionsGroup.Children.OfType<ContentLibraryItemViewModel<Collection>>();
        }

        void SetupOnlineItems() {
            SetupNetwork();
            SetupCustomRepositories();
        }

        void SetupCustomRepositories() {
            RebuildCustomRepositories();
            _disposables.Add(_contentList.CustomRepositories.TrackChanges(AddRepoWhenCompatible,
                x => OnlineGroup.Children.Remove(GetRepo(x)), RepoReset));
        }

        void RebuildCustomRepositories() {
            lock (_contentList.CustomRepositories)
                RepoReset(_contentList.CustomRepositories);
        }

        ContentLibraryItemViewModel<SixRepo> GetRepo(SixRepo repo) {
            return
                OnlineGroup.Children.OfType<ContentLibraryItemViewModel<SixRepo>>().FirstOrDefault(y => y.Model == repo);
        }

        void SetupNetwork() {
            var container = new BuiltInContentContainer("Browse");
            _disposables.Add(_contentList.Mods.KeepCollectionInSyncOfType(container.Items,
                y => y.GameMatch(_modding)));

            Network = new NetworkLibraryItemViewModel(_library, container, BuiltInGroup) {
                IsRoot = true,
                SortOrder = 6,
                IsHead = true
            };
            Items.Insert(0, Network);

            SetupUpdates();
            SetupBuiltInFeatured();
            SetupBuiltInFavorites();
            SetupBuiltInRecent();
        }

        void SetupUpdates() {
            Updates = new CollectionLibraryItemViewModel(_library,
                new BuiltInCollection(_game.Modding()) {Name = "Updates"},
                SixIconFont.withSIX_icon_Arrow_Down_Open,
                isFeatured: true) {IconForeground = SixColors.SixOrange, IsRoot = true, SortOrder = 2};

            Updates.Model.AddModAndUpdateState(
                Network.Items.OfType<IMod>().Where(x => x.State == ContentState.UpdateAvailable).ToArray(),
                _contentList);

            _disposables.Add(Network.Items.ItemChanged.Where(x => x.PropertyName == "State")
                .Subscribe(x => HandleModStateChanged((IMod) x.Sender)));

            _disposables.Add(Updates.WhenAnyValue(x => x.HasItems)
                .Subscribe(x => {
                    if (x)
                        Items.Add(Updates);
                    else {
                        Items.Remove(Updates);
                        Updates.IsSelected = false;
                    }
                }));
        }

        void HandleModStateChanged(IMod sender) {
            if (sender.State == ContentState.UpdateAvailable)
                Updates.Model.AddModAndUpdateState(sender, _contentList);
            else
                Updates.Model.RemoveModAndUpdateState(sender, _contentList);
        }

        void RepoReset(ICollection<SixRepo> repositories) {
            var repos = GetCustomRepoItems().OfType<ContentLibraryItemViewModel<SixRepo>>().ToArray();
            repositories.ForEach(x => x.Reset(_contentList, _modding));

            var remove = repos.Where(x => !x.Model.GameMatch()).ToArray();
            var add = repositories.Where(x => x.GameMatch() && !repos.Select(y => y.Model).Contains(x))
                .Select(x => CreateCustomRepo(x, OnlineGroup)).ToArray();
            lock (OnlineGroup.Children) {
                OnlineGroup.Children.RemoveRange(remove);
                OnlineGroup.Children.AddRange(add);
            }
        }

        void AddRepoWhenCompatible(SixRepo repo) {
            var r = repo;
            r.Reset(_contentList, _modding);
            if (!r.GameMatch())
                return;

            OnlineGroup.Children.Add(CreateCustomRepo(r, OnlineGroup));
        }

        IEnumerable<ContentLibraryItemViewModel> GetCustomRepoItems() {
            return OnlineGroup.Children.OfType<ContentLibraryItemViewModel<SixRepo>>()
                .Concat(GetCustomRepoModSets().Cast<ContentLibraryItemViewModel>());
        }

        IEnumerable<ContentLibraryItemViewModel<Collection>> GetCustomRepoModSets() {
            return GetCollections().Where(y => {
                var m = y.Model as CustomCollection;
                return m != null && m.Id == Guid.Empty && m.CustomRepoUrl != null;
            });
        }

        public void Reset() {
            HandleAia();
            RebuildNetworkMods();
            RebuildCustomRepositories();
        }

        void RebuildNetworkMods() {
            _game.Lists.Mods.SyncCollection(Network.Items, y => ((IMod) y).GameMatch(_modding));
        }

        void SetupLocalItems() {
            _gameFolders = _modding.LocalModsContainers().Select(x => CreateLocalItem(x, true)).ToArray();
            LocalGroup.Children.AddRange(_gameFolders);
            lock (_contentList.LocalModsContainers) {
                ResetLocalModContainers(_contentList.LocalModsContainers.Where(x => x.GameMatch(_game)).ToArray());
                _disposables.Add(
                    _contentList.LocalModsContainers.TrackChanges(x => LocalGroup.Children.Add(CreateLocalItem(x)),
                        x => LocalGroup.Children.Remove(GetLocalContainer(x)),
                        ResetLocalModContainers, container => container.GameMatch(_game)));
            }
        }

        void ResetLocalModContainers(IEnumerable<LocalModsContainer> reset) {
            LocalGroup.Children.Clear();
            LocalGroup.Children.AddRange(_gameFolders.Concat(reset.Select(x => CreateLocalItem(x))));
            HandleAia();
        }

        void HandleAia() {
            _aIa.Children.Clear();

            var a3 = _game as Arma3Game;
            if (a3 != null && a3.CalculatedSettings.HasAllInArmaLegacy) {
                _aIa.Children.AddRange(a3.GetSubGamesLocalMods()
                    .DistinctBy(x => x.Path, StringComparer.CurrentCultureIgnoreCase)
                    .Select(x => CreateLocalItem(x, true)));
            }

            var hasChildren = _aIa.Children.Any();
            if (hasChildren && !LocalGroup.Children.Contains(_aIa))
                LocalGroup.Children.Add(_aIa);
            else if (!hasChildren && LocalGroup.Children.Contains(_aIa))
                LocalGroup.Children.Remove(_aIa);
        }

        ContentLibraryItemViewModel GetLocalContainer(LocalModsContainer localModsContainer) {
            return GetLocalMods().FirstOrDefault(x => x.Model.Equals(localModsContainer));
        }

        public IEnumerable<ContentLibraryItemViewModel<LocalModsContainer>> GetLocalMods() {
            return LocalGroup.Children.OfType<ContentLibraryItemViewModel<LocalModsContainer>>();
        }

        void SetupBuiltInPlaylist() {
            var container = new BuiltInContentContainer("Playlist");
            SyncCgsMods(_game.CalculatedSettings.CurrentMods, container);
            _disposables.Add(_game.CalculatedSettings.WhenAnyValue(x => x.CurrentMods)
                .Subscribe(x => SyncCgsMods(x, container)));
            var playlist = new BrowseContentLibraryItemViewModel<BuiltInContentContainer>(_library, container,
                SixIconFont.withSIX_icon_Play) {
                    Description = "All mods that will be used to Launch or Update, incl dependencies and server mods",
                    IsFeatured = true,
                    IsRoot = true,
                    IconForeground = SixColors.SixGreen,
                    SortOrder = 1
                };
            Items.Add(playlist);
        }

        void SyncCgsMods(IEnumerable<IMod> mods, IHaveReactiveItems<IContent> container) {
            mods.SyncCollection(container.Items);
        }

        void SetupBuiltInFeatured() {
            var container = new BuiltInContentContainer("Featured");
            _builtinFeatured =
                new BrowseContentLibraryItemViewModel<BuiltInContentContainer>(_library, container,
                    SixIconFont.withSIX_icon_Ribbon, BuiltInGroup, true) {
                        IconForeground = SixColors.SixOrange,
                        IsRoot = true,
                        SortOrder = 7,
                        IsHead = true
                    };
            container.Items.AddRange(_game.Lists.Collections.Where(x => _desiredModsets.Any(y => y.Equals(x.Id))));
            container.Items.AddRange(Network.Items.Where(IsFeatured));
            Items.Add(_builtinFeatured);
        }

        void SetupBuiltInFavorites() {
            var container = new BuiltInContentContainer("Favorites");
            _disposables.Add(Network.Items.KeepCollectionInSync(container.Items, x => x.IsFavorite));
            _disposables.Add(Network.Items.ItemChanged.Where(x => x.PropertyName == "IsFavorite")
                .Subscribe(x => {
                    if (x.Sender.IsFavorite)
                        _builtinFavorites.Items.Add(x.Sender);
                    else
                        _builtinFavorites.Items.Remove(x.Sender);
                }));

            _builtinFavorites =
                new BrowseContentLibraryItemViewModel<BuiltInContentContainer>(_library, container,
                    SixIconFont.withSIX_icon_Star, BuiltInGroup, true) {
                        IconForeground = SixColors.SixOrange,
                        IsRoot = true,
                        SortOrder = 8
                    };

            Items.Add(_builtinFavorites);
        }

        void SetupBuiltInRecent() {
            var container = new BuiltInContentContainer("Recent");
            _settings.ModOptions.RecentCollections.Select(FindModSet)
                .Where(x => x != null)
                .SyncCollection(container.Items);

            _builtinRecent =
                new BrowseContentLibraryItemViewModel<BuiltInContentContainer>(_library, container,
                    SixIconFont.withSIX_icon_Clock, BuiltInGroup, true) {IsRoot = true, SortOrder = 9};

            _disposables.Add(_settings.ModOptions.RecentCollections.TrackChangesDerrivedConvert(_builtinRecent.Items,
                FindModSet,
                set => FindModSet(set) != null));

            Items.Add(_builtinRecent);
        }

        bool IsFeatured(IContent mod) {
            // TODO: Convert to an editable featured.json list on the web
            return mod.Name != "@IFA3" &&
                   (_featured.Contains(mod.Name, StringComparer.OrdinalIgnoreCase) ||
                    _game.Lists.Collections.Any(
                        modSet => modSet.Mods.Contains(mod.Name, StringComparer.OrdinalIgnoreCase)));
        }

        Collection FindModSet(RecentCollection x) {
            return _game.Lists.Collections.FirstOrDefault(x.Matches) ??
                   _game.Lists.CustomCollections.FirstOrDefault(x.Matches);
        }

        ContentLibraryItemViewModel CreateCustomModSet(Collection collection) {
            var subscribedModSet = collection as SubscribedCollection;
            if (subscribedModSet != null)
                return new SubscribedCollectionLibraryItemViewModel(_library, subscribedModSet, CollectionsGroup);


            var customModSet = (CustomCollection) collection;
            /*
            // Do not allow publishing of collections with custom repos...
            if (customModSet.HasCustomRepo())
                return new CustomRepoCollectionLibraryItem(customModSet, CollectionsGroup);
*/

            return new CustomCollectionLibraryItemViewModel(_library, customModSet, CollectionsGroup);
        }

        ContentLibraryItemViewModel CreateCustomRepo(SixRepo repo, LibraryGroupViewModel group) {
            var libItem = new BrowseContentLibraryItemViewModel<SixRepo>(_library, repo, @group: group, isFeatured: true);
            foreach (var modSet in repo.CollectionItems) {
                modSet.UpdateState();
                var customRepoCollectionLibraryItem = new CustomRepoCollectionLibraryItemViewModel(_library, modSet,
                    null, true);
                libItem.Children.Add(customRepoCollectionLibraryItem);
                libItem.Items.Add(modSet);
            }
            return libItem;
        }

        public ContentLibraryItemViewModel<LocalModsContainer> CreateLocalItem(string name, bool isFeatured, string path) {
            return CreateLocalItem(new LocalModsContainer(name, path, _game), isFeatured);
        }

        LocalModsLibraryItemViewModel CreateLocalItem(LocalModsContainer localModsContainer,
            bool isFeatured = false) {
            localModsContainer.FillLocalLibrary();
            return new LocalModsLibraryItemViewModel(_library, localModsContainer, LocalGroup, isFeatured);
        }

        void ResetGameFolders() {
            foreach (var gf in _gameFolders)
                gf.Model.Dispose();
            var gameFolders = _modding.LocalModsContainers().Select(x => CreateLocalItem(x, true)).ToArray();
            LocalGroup.Children.AddRange(_gameFolders = gameFolders);
        }

        #region IDisposable

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;

            if (disposing) {
                // dispose managed resources
                _eventBus.Unsubscribe(this);
                _disposables.Dispose();

                foreach (var g in _gameFolders)
                    g.Model.Dispose();

                foreach (var contentLibraryItem in Items) {
                    var item = contentLibraryItem as ContentLibraryItemViewModel<LocalModsContainer>;
                    if (item != null)
                        item.Model.Dispose();
                }
            }

            // free native resources
            // set large fields to null.
            // call Dispose on your base class if needed
            // base.Dispose(disposing);

            _disposed = true;
        }

        #endregion

        #region IHandle events

        public void Handle(GamePathChanged message) {
            ResetGameFolders();
        }

        public void Handle(ModPathChanged message) {
            ResetGameFolders();
        }

        #endregion
    }
}