// <copyright company="SIX Networks GmbH" file="MultiLibraryItemContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class MultiLibraryItemContextMenu : ContextMenuBase<IReadOnlyCollection<IHierarchicalLibraryItem>>
    {
        readonly ModLibraryViewModel _modLibraryViewModel;

        public MultiLibraryItemContextMenu(ModLibraryViewModel modLibraryViewModel) {
            _modLibraryViewModel = modLibraryViewModel;
        }

        protected override void UpdateItemsFor(IReadOnlyCollection<IHierarchicalLibraryItem> item) {
            base.UpdateItemsFor(item);
            Items.ForEach(x => x.IsVisible = true);
            GetAsyncItem(RemoveSelected).IsVisible = item.OfType<CollectionLibraryItemViewModel>().Any();
            if (!item.All(x => x is CollectionLibraryItemViewModel)) {
                GetItem(ClearCustomizations)
                    .IsVisible = false;
                GetItem(ClearCollection)
                    .IsVisible = false;
                GetAsyncItem(Unsubscribe)
                    .IsVisible = false;
            } else {
                if (item.Any(x => ((CollectionLibraryItemViewModel) x).IsSubscribedCollection)) {
                    //GetItem(ClearCustomizations)
                    //    .IsVisible = false;
                    GetItem(ClearCollection)
                        .IsVisible = false;
                    GetAsyncItem(RemoveSelected)
                        .IsVisible = false;
                }
                if (!item.All(x => ((CollectionLibraryItemViewModel) x).IsSubscribedCollection)) {
                    GetAsyncItem(Unsubscribe)
                        .IsVisible = false;
                }
            }
        }

        [MenuItem, DoNotObfuscate]
        public Task RemoveSelected(IReadOnlyCollection<IHierarchicalLibraryItem> items) {
            return _modLibraryViewModel.RemoveLibraryItem(items.OfType<CollectionLibraryItemViewModel>());
        }

        [MenuItem, DoNotObfuscate]
        public void ClearCollection(IReadOnlyCollection<IHierarchicalLibraryItem> content) {
            _modLibraryViewModel.ClearCollections(content.OfType<CollectionLibraryItemViewModel>());
        }

        [MenuItem, DoNotObfuscate]
        public void ClearCustomizations(IReadOnlyCollection<IHierarchicalLibraryItem> content) {
            _modLibraryViewModel.ClearCustomizations(content.OfType<CollectionLibraryItemViewModel>());
        }

        [MenuItem, DoNotObfuscate]
        public Task Unsubscribe(IReadOnlyCollection<IHierarchicalLibraryItem> content) {
            return
                Task.WhenAll(
                    content.OfType<SubscribedCollectionLibraryItemViewModel>()
                        .Select(x => _modLibraryViewModel.Unsubscribe(x)));
        }
    }
}