// <copyright company="SIX Networks GmbH" file="ServerLibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Data;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class ServerLibraryItemViewModel<T> : ServerLibraryItemViewModel, IHaveModel<T>
        where T : ISelectionList<Server>
    {
        public ServerLibraryItemViewModel(ServerLibraryViewModel library, T model, IFilter serverFilter,
            SortDescriptionCollection existing,
            string icon = null, ServerLibraryGroupViewModel group = null, bool isFeatured = false,
            bool doGrouping = false)
            : base(library, serverFilter, existing, icon, group, isFeatured, doGrouping) {
            Model = model;

            SetupViews();
        }

        public override ReactiveList<Server> Items
        {
            get { return Model.Items; }
        }
        public T Model { get; }
    }

    public abstract class ServerLibraryItemViewModel : LibraryItemViewModel<Server>
    {
        static readonly PropertyGroupDescription PropertyGroupDescription =
            new PropertyGroupDescription("GameType");
        readonly SortDescriptionCollection _existing;
        readonly ServerBarMenu _serverBarMenu;
        readonly ServerContextMenu _serverMenu;

        protected ServerLibraryItemViewModel(ServerLibraryViewModel library, IFilter serverFilter,
            SortDescriptionCollection existing = null,
            string icon = null, ServerLibraryGroupViewModel @group = null, bool isFeatured = false,
            bool doGrouping = false)
            : base(@group) {
            // TODO: Push down to LibraryItem
            Icon = icon;
            Group = group;
            IsFeatured = isFeatured;
            DoGrouping = doGrouping;
            _existing = existing;

            Filter = serverFilter;

            _serverMenu = new ServerContextMenu(library);
            _serverBarMenu = new ServerBarMenu(library);

            SetupMenus(HandleSingleMenu, x => ContextMenu = null);
        }

        void HandleSingleMenu(Server first) {
            _serverBarMenu.SetNextItem(first);
            _serverMenu.ShowForItem(first);
            ContextMenu = _serverMenu;
            BarMenu = _serverBarMenu;
        }

        protected void SetupViews() {
            var groups = DoGrouping
                ? new[] {PropertyGroupDescription}
                : null;

            UiHelper.TryOnUiThread(() => {
                Items.EnableCollectionSynchronization(ItemsLock);
                _itemsView =
                    Items.CreateCollectionView(
                        new[] {
                            new SortDescription("IsFeatured", ListSortDirection.Descending),
                            new SortDescription("HasFriends", ListSortDirection.Descending),
                            new SortDescription("IsFavorite", ListSortDirection.Descending),
                            new SortDescription("NumPlayers", ListSortDirection.Descending)
                        }, groups,
                        new[] {
                            "HasBasicInfo",
                            "IsFeatured",
                            "IsFavorite",
                            "Ping"
                        }, Filter.Handler, true);

                _childrenView =
                    Children.CreateCollectionView(
                        new[] {
                            new SortDescription("Model.IsFavorite", ListSortDirection.Descending),
                            new SortDescription("Model.Name", ListSortDirection.Ascending)
                        }, null,
                        null, null, true);
                Sort = new SortViewModel(ItemsView, ServersViewModel.Columns,
                    _existing, ServersViewModel.RequiredColumns);

                SetupGrouping();
                SetupFilterChanged();
            });
        }

        protected void SetupGrouping() {
            this.WhenAnyValue(x => x.DoGrouping)
                .Skip(1)
                .Subscribe(x => {
                    if (x)
                        EnableGrouping();
                    else
                        DisableGrouping();
                });
        }

        void EnableGrouping() {
            ItemsView.GroupDescriptions.AddWhenMissing(PropertyGroupDescription);
        }

        void DisableGrouping() {
            ItemsView.GroupDescriptions.Remove(PropertyGroupDescription);
        }
    }
}