// <copyright company="SIX Networks GmbH" file="SearchContentLibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ReactiveUI;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public abstract class SearchContentLibraryItemViewModel : ContentLibraryItemViewModel,
        IHaveModel<BuiltInContentContainer>,
        ISearchLibraryItem<IContent>
    {
        protected SearchContentLibraryItemViewModel(LibraryRootViewModel library)
            : base(library, null) {
            Model = new BuiltInContentContainer("Search");
            Icon = SixIconFont.withSIX_icon_Search;
            IsFeatured = true;
            Filter = new ModLibraryFilter();
            SetupFilterChanged();

            UiHelper.TryOnUiThread(() => {
                Items.EnableCollectionSynchronization(ItemsLock);
                _itemsView =
                    Items.CreateCollectionView(
                        new[] {
                            new SortDescription("SearchScore", ListSortDirection.Descending)
                        },
                        null, null, Filter.Handler, true);
            });
            var sortDatas = new[] {
                new SortData {
                    DisplayName = "Search score",
                    Value = "SearchScore",
                    SortDirection = ListSortDirection.Descending
                }
            };
            Sort = new SortViewModel(ItemsView, sortDatas.Concat(Columns).ToArray(), null, RequiredColumns);
            SetupGrouping();
            SortOrder = 3;
            IsRoot = true;
        }

        public BuiltInContentContainer Model { get; }
        public override ReactiveList<IContent> Items { get; } = new ReactiveList<IContent>();

        public void UpdateItems(IEnumerable<IContent> items) {
            items.SyncCollection(Items);
        }
    }
}