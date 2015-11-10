// <copyright company="SIX Networks GmbH" file="ServerLibrarySetup.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Games.Legacy.Servers;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class ServerLibrarySetup : LibrarySetup<ServerLibraryItemViewModel>, IDisposable
    {
        readonly ServerLibraryItemViewModel<BuiltInServerContainer> _builtinFavorites;
        readonly ServerLibraryItemViewModel<BuiltInServerContainer> _builtinFeatured;
        readonly ServerLibraryItemViewModel<BuiltInServerContainer> _builtinRecent;
        readonly CompositeDisposable _disposables = new CompositeDisposable();
        readonly ServerLibraryViewModel _library;
        readonly IServerList _serverList;
        public readonly ServerLibraryGroupViewModel BuiltInGroup;
        ServerLibraryItemViewModel<BuiltInServerContainer> _network;

        public ServerLibrarySetup(ServerLibraryViewModel library, IServerList serverList) {
            _library = library;
            _serverList = serverList;
            BuiltInGroup = new ServerLibraryGroupViewModel(library, null) {IsRoot = true, SortOrder = 0};

            _builtinFeatured =
                new ServerLibraryItemViewModel<BuiltInServerContainer>(library, new BuiltInServerContainer("Featured"),
                    new CleanServerFilter(), null,
                    SixIconFont.withSIX_icon_Ribbon, BuiltInGroup, true) {
                        IconForeground = SixColors.SixOrange,
                        IsRoot = true,
                        SortOrder = 7,
                        IsHead = true
                    };
            _builtinFavorites =
                new ServerLibraryItemViewModel<BuiltInServerContainer>(library, new BuiltInServerContainer("Favorites"),
                    new CleanServerFilter(), null,
                    SixIconFont.withSIX_icon_Star, BuiltInGroup, true) {
                        IconForeground = SixColors.SixOrange,
                        IsRoot = true,
                        SortOrder = 8
                    };
            _builtinRecent =
                new ServerLibraryItemViewModel<BuiltInServerContainer>(library, new BuiltInServerContainer("Recent"),
                    new CleanServerFilter(), null,
                    SixIconFont.withSIX_icon_Clock, BuiltInGroup, true) {IsRoot = true, SortOrder = 9};

            Groups = new LibraryGroupViewModel<ServerLibraryViewModel>[] {null, BuiltInGroup};

            if (serverList != null)
                Setup();

            Items.AddRange(Groups.Where(x => x != null && x != BuiltInGroup));
            CreateItemsView();
        }

        public LibraryGroupViewModel<ServerLibraryViewModel>[] Groups { get; protected set; }

        public void Dispose() {
            Dispose(true);
        }

        void Setup() {
            SetupNetwork();
        }

        void SetupNetwork() {
            // TODO: Different saving mechanism; we don't want to store also the required columns, but just the user columns...
            //var serverOptions = UserSettings.Current.ServerOptions;
            _network = new NetworkLibraryItemViewModel(_library, new BuiltInServerContainer("Browse"),
                _serverList.Filter,
                BuiltInGroup) {IsRoot = true, SortOrder = 6};
            // serverOptions.SortData
            //serverOptions.SortData = network.Sort.SortDescriptions;
            _serverList.Items.KeepCollectionInSync(_network.Items);
            Items.Insert(0, _network);
            SetupFavorites();
            SetupRecent();
            SetupFeatured();
        }

        void SetupRecent() {
            _disposables.Add(_network.Items.KeepCollectionInSync(_builtinRecent.Items, x => x.LastJoinedOn != null));
            _disposables.Add(_network.Items.ItemChanged
                .Where(x => x.PropertyName == "LastJoinedOn")
                .Subscribe(x => {
                    var val = x.Sender.LastJoinedOn;
                    if (val == null)
                        _builtinRecent.Items.Remove(x.Sender);
                    else
                        _builtinRecent.Items.AddWhenMissing(x.Sender);
                }));

            Items.Add(_builtinRecent);
        }

        void SetupFavorites() {
            _disposables.Add(_network.Items.KeepCollectionInSync(_builtinFavorites.Items, x => x.IsFavorite));
            _disposables.Add(_network.Items.ItemChanged
                .Where(x => x.PropertyName == "IsFavorite")
                .Subscribe(x => {
                    var val = x.Sender.IsFavorite;
                    if (val)
                        _builtinFavorites.Items.AddWhenMissing(x.Sender);
                    else
                        _builtinFavorites.Items.Remove(x.Sender);
                }));

            Items.Add(_builtinFavorites);
        }

        void SetupFeatured() {
            Items.Add(_builtinFeatured);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                _disposables.Dispose();
        }

        public class NetworkLibraryItemViewModel : ServerLibraryItemViewModel<BuiltInServerContainer>
        {
            public NetworkLibraryItemViewModel(ServerLibraryViewModel library, BuiltInServerContainer model,
                IFilter filter,
                ServerLibraryGroupViewModel @group = null,
                bool doGrouping = false)
                : base(library, model, filter, null, SixIconFont.withSIX_icon_Nav_Server, @group, true, doGrouping) {
                Items.ChangeTrackingEnabled = true;
            }
        }
    }
}