// <copyright company="SIX Networks GmbH" file="LibrarySetup.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public abstract class LibrarySetup<TContent> : IHaveReactiveItems<IHierarchicalLibraryItem>
    {
        readonly object _itemsLock = new object();

        protected LibrarySetup() {
            Items = new ReactiveList<IHierarchicalLibraryItem>();
        }

        public ICollectionView ItemsView { get; private set; }
        public ReactiveList<IHierarchicalLibraryItem> Items { get; }

        protected void CreateItemsView() {
            UiHelper.TryOnUiThread(CreateItemsViewInternal);
        }

        void CreateItemsViewInternal() {
            Items.EnableCollectionSynchronization(_itemsLock);
            //var groupDesc = new PropertyGroupDescription("Group");
            //Groups.SyncCollection(groupDesc.GroupNames);
            ItemsView = Items.CreateCollectionView(
                new[] {
                    new SortDescription("SortOrder", ListSortDirection.Ascending),
                    new SortDescription("Group.Header", ListSortDirection.Ascending),
                    new SortDescription("IsFeatured", ListSortDirection.Descending),
                    new SortDescription("Model.Name", ListSortDirection.Ascending)
                },
                null, // new[] {groupDesc}
                null, null, true);
        }
    }
}