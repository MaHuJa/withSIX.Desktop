// <copyright company="SIX Networks GmbH" file="MissionLibrarySetup.cs">
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
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class MissionLibrarySetup : LibrarySetup<ContentLibraryItemViewModel>, IDisposable
    {
        readonly CompositeDisposable _disposables = new CompositeDisposable();
        readonly IEventAggregator _eventBus;
        readonly Game _game;
        readonly MissionLibraryViewModel _library;
        readonly IContentManager _missionList;
        readonly UserSettings _settings;
        public readonly MissionLibraryGroupViewModel BuiltInGroup;
        public readonly MissionLibraryGroupViewModel CollectionsGroup;
        public readonly MissionLibraryGroupViewModel LocalGroup;
        public readonly MissionLibraryGroupViewModel OnlineGroup;
        //"add repo",
        ContentLibraryItemViewModel<BuiltInContentContainer> _builtinFavorites;
        ContentLibraryItemViewModel<BuiltInContentContainer> _builtinFeatured;
        ContentLibraryItemViewModel<BuiltInContentContainer> _builtinRecent;
        bool _disposed;
        ContentLibraryItemViewModel<LocalMissionsContainer>[] _gameFolders;
        NetworkLibraryItemViewModel _network;
        ContentLibraryItemViewModel<BuiltInContentContainer> _updates;

        public MissionLibrarySetup(MissionLibraryViewModel library, Game game, IContentManager missionList,
            UserSettings settings,
            IEventAggregator eventBus) {
            Contract.Requires<ArgumentNullException>(game != null);
            Contract.Requires<NotSupportedException>(game.SupportsMissions());
            Contract.Requires<ArgumentNullException>(missionList != null);

            _library = library;
            _game = game;
            _missionList = missionList;
            _eventBus = eventBus;
            _settings = settings;
            OnlineGroup = new MissionLibraryGroupViewModel(library, "Repositories", icon: SixIconFont.withSIX_icon_Cloud) {
                IsRoot = true,
                SortOrder = 13
            };
            LocalGroup = new MissionLibraryGroupViewModel(library, "Local", "add folder",
                SixIconFont.withSIX_icon_System) {IsRoot = true, SortOrder = 12};
            CollectionsGroup = new MissionLibraryGroupViewModel(library, "Collections", "new collection",
                SixIconFont.withSIX_icon_Folder) {IsRoot = true, SortOrder = 11};
            BuiltInGroup = new MissionLibraryGroupViewModel(library, null) {IsRoot = true, SortOrder = 0};
            Groups = new LibraryGroupViewModel<MissionLibraryViewModel>[] {null, BuiltInGroup, LocalGroup};
                // , OnlineGroup

            _eventBus.Subscribe(this);

            Items.AddRange(Groups.Where(x => x != null && x != BuiltInGroup));
            if (_game.SupportsMissions())
                Setup();
            CreateItemsView();
        }

        public LibraryGroupViewModel<MissionLibraryViewModel>[] Groups { get; protected set; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MissionLibrarySetup() {
            Dispose(false);
        }

        void Setup() {
            SetupLocal();
            SetupNetwork();
        }

        void SetupLocal() {
            _gameFolders =
                ((ISupportMissions) _game).LocalMissionsContainers()
                    .Select(x => CreateLocalItem(x, true))
                    .ToArray();
            LocalGroup.Children.AddRange(_gameFolders);
            ResetLocalMissionContainers(_missionList.LocalMissionsContainers.Where(x => x.GameMatch(_game)).ToArray());

            _disposables.Add(
                _missionList.LocalMissionsContainers.TrackChanges(x => LocalGroup.Children.Add(CreateLocalItem(x)),
                    x => LocalGroup.Children.Remove(GetLocalContainer(x)),
                    ResetLocalMissionContainers, container => container.GameMatch(_game)));
        }

        void ResetLocalMissionContainers(IEnumerable<LocalMissionsContainer> reset) {
            LocalGroup.Children.RemoveRange(GetLocalMissions().ToArray());
            LocalGroup.Children.AddRange(_gameFolders.Concat(reset.Select(x => CreateLocalItem(x))));
        }

        ContentLibraryItemViewModel GetLocalContainer(LocalMissionsContainer localModsContainer) {
            return GetLocalMissions().FirstOrDefault(x => x.Model.Equals(localModsContainer));
        }

        public ContentLibraryItemViewModel<LocalMissionsContainer> CreateLocalItem(string name, bool isFeatured,
            string path) {
            return CreateLocalItem(new LocalMissionsContainer(name, path, _game), isFeatured);
        }

        public IEnumerable<ContentLibraryItemViewModel<LocalMissionsContainer>> GetLocalMissions() {
            return LocalGroup.Children.OfType<ContentLibraryItemViewModel<LocalMissionsContainer>>();
        }

        ContentLibraryItemViewModel<LocalMissionsContainer> CreateLocalItem(LocalMissionsContainer missionContainer,
            bool isFeatured = false) {
            missionContainer.FillLocalLibrary();
            return new LocalMissionLibraryItemViewModel(_library, missionContainer, LocalGroup, isFeatured);
        }

        void SetupNetwork() {
            var container = new BuiltInContentContainer("Browse");
            _disposables.Add(_missionList.Missions.KeepCollectionInSync2(container.Items, x => x.GameMatch(_game)));
            _network = new NetworkLibraryItemViewModel(_library, container, BuiltInGroup) {IsRoot = true, SortOrder = 6};

            Items.Insert(0, _network);
            SetupUpdates();
            SetupBuiltInFeatured();
            SetupBuiltInFavorites();
            SetupBuiltInRecent();
        }

        void SetupUpdates() {
            _updates = new MissionContentLibraryItemViewModel<BuiltInContentContainer>(_library,
                new BuiltInContentContainer("Updates"),
                SixIconFont.withSIX_icon_Arrow_Down_Dir,
                isFeatured: true) {IconForeground = SixColors.SixOrange, IsRoot = true, SortOrder = 2};

            _network.Items.OfType<Mission>().Where(x => x.State == ContentState.UpdateAvailable)
                .ForEach(x => _updates.Items.Add(x));

            _disposables.Add(_network.Items.ItemChanged.Where(x => x.PropertyName == "State")
                .Subscribe(x => HandleMissionStateChanged((MissionBase) x.Sender)));

            _disposables.Add(_updates.WhenAnyValue(x => x.HasItems)
                .Subscribe(x => {
                    if (x)
                        Items.Add(_updates);
                    else
                        Items.Remove(_updates);
                }));
        }

        void HandleMissionStateChanged(MissionBase sender) {
            if (sender.State == ContentState.UpdateAvailable)
                _updates.Items.AddWhenMissing(sender);
            else
                _updates.Items.Remove(sender);
        }

        void SetupBuiltInFeatured() {
            _builtinFeatured =
                new MissionContentLibraryItemViewModel<BuiltInContentContainer>(_library,
                    new BuiltInContentContainer("Featured"),
                    SixIconFont.withSIX_icon_Ribbon, BuiltInGroup, true) {
                        IconForeground = SixColors.SixOrange,
                        IsRoot = true,
                        SortOrder = 7,
                        IsHead = true
                    };
            Items.Add(_builtinFeatured);
        }

        void SetupBuiltInFavorites() {
            var container = new BuiltInContentContainer("Favorites");
            _disposables.Add(_network.Items.KeepCollectionInSyncOfType<Mission, IContent>(container.Items,
                x => x.IsFavorite));
            _builtinFavorites =
                new MissionContentLibraryItemViewModel<BuiltInContentContainer>(_library, container,
                    SixIconFont.withSIX_icon_Star, BuiltInGroup, true) {
                        IconForeground = SixColors.SixOrange,
                        IsRoot = true,
                        SortOrder = 8
                    };

            // TODO: These probably gets reset if _network.Items resets..
            SetupFavoriteTracking(_network.Items);
            foreach (var g in _gameFolders)
                SetupFavoriteTracking(g.Model.Items);

            Items.Add(_builtinFavorites);
        }

        void SetupFavoriteTracking(ReactiveList<IContent> items) {
            _disposables.Add(items.ItemChanged.Where(x => x.PropertyName == "IsFavorite")
                .Subscribe(x => {
                    if (x.Sender.IsFavorite)
                        _builtinFavorites.Items.Add(x.Sender);
                    else
                        _builtinFavorites.Items.Remove(x.Sender);
                }));
        }

        void SetupBuiltInRecent() {
            var container = new BuiltInContentContainer("Recent");
            _settings.MissionOptions.RecentMissions.Select(FindMission)
                .Where(x => x != null)
                .SyncCollection(container.Items);

            _builtinRecent =
                new MissionContentLibraryItemViewModel<BuiltInContentContainer>(_library, container,
                    SixIconFont.withSIX_icon_Clock, BuiltInGroup, true) {IsRoot = true, SortOrder = 9};


            _disposables.Add(_settings.MissionOptions.RecentMissions.TrackChangesDerrivedConvert(_builtinRecent.Items,
                FindMission,
                set => FindMission(set) != null));

            Items.Add(_builtinRecent);
        }

        MissionBase FindMission(RecentMission x) {
            return _game.Lists.Missions.Cast<MissionBase>().FirstOrDefault(x.Matches)
                   ?? FindInGameFolder(x);
        }

        MissionBase FindInGameFolder(RecentMission recent) {
            return _gameFolders == null
                ? null
                : _gameFolders.SelectMany(x => x.Model.Items.Cast<MissionBase>())
                    .FirstOrDefault(recent.Matches);
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

                _gameFolders = null;

                var items = LocalGroup.Children.OfType<ContentLibraryItemViewModel<LocalMissionsContainer>>().ToArray();
                foreach (var contentLibraryItem in items)
                    contentLibraryItem.Model.Dispose();
                LocalGroup.Children.RemoveRange(items);
            }

            // free native resources
            // set large fields to null.
            // call Dispose on your base class if needed
            // base.Dispose(disposing);

            _disposed = true;
        }

        public class LocalMissionLibraryItemViewModel : MissionLibraryItemViewModel<LocalMissionsContainer>
        {
            public LocalMissionLibraryItemViewModel(MissionLibraryViewModel library, LocalMissionsContainer model,
                MissionLibraryGroupViewModel @group = null,
                bool isFeatured = false, bool doGrouping = false)
                : base(library, model, SixIconFont.withSIX_icon_Folder, @group, isFeatured, doGrouping) {
                Items.ChangeTrackingEnabled = true;
                Description = model.Path;
            }
        }

        public class NetworkLibraryItemViewModel : MissionLibraryItemViewModel<BuiltInContentContainer>
        {
            public NetworkLibraryItemViewModel(MissionLibraryViewModel library, BuiltInContentContainer model,
                MissionLibraryGroupViewModel @group = null, bool doGrouping = false)
                : base(library, model, SixIconFont.withSIX_icon_Nav_Server, @group, true, doGrouping) {
                Items.ChangeTrackingEnabled = true;
            }
        }
    }
}