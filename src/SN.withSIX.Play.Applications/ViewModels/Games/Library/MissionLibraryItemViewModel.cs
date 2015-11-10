// <copyright company="SIX Networks GmbH" file="MissionLibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class MissionLibraryItemViewModel<T> : MissionContentLibraryItemViewModel<T>
        where T : class, ISelectionList<IContent>
    {
        public MissionLibraryItemViewModel(MissionLibraryViewModel library, T model, string icon = null,
            MissionLibraryGroupViewModel group = null, bool isFeatured = false,
            bool doGrouping = false)
            : this(library, model, doGrouping, group) {
            Icon = icon;
            IsFeatured = isFeatured;
        }

        MissionLibraryItemViewModel(MissionLibraryViewModel library, T model, bool doGrouping = false,
            MissionLibraryGroupViewModel group = null)
            : base(library, model, @group: group, doGrouping: doGrouping) {
            var groups = doGrouping
                ? new[] {PropertyGroupDescription}
                : null;
            Filter = new MissionFilter();

            SetupFilterChanged();
            UiHelper.TryOnUiThread(() => {
                Items.EnableCollectionSynchronization(ItemsLock);
                _itemsView =
                    Items.CreateCollectionView(
                        new[] {
                            new SortDescription("IsFavorite", ListSortDirection.Descending),
                            new SortDescription("FullName", ListSortDirection.Ascending)
                        },
                        groups, null, Filter.Handler, true);

                _childrenView =
                    Children.CreateCollectionView(
                        new[] {
                            new SortDescription("Model.IsFavorite", ListSortDirection.Descending),
                            new SortDescription("Model.Name", ListSortDirection.Ascending)
                        }, null,
                        null, null, true);
            });
            Sort = new SortViewModel(ItemsView, Columns, null, RequiredColumns);
            SetupGrouping();
        }

        public override ReactiveList<IContent> Items
        {
            get { return Model.Items; }
        }
    }
}