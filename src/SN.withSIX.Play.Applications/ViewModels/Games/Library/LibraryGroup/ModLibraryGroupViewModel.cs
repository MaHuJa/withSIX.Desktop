// <copyright company="SIX Networks GmbH" file="ModLibraryGroupViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup
{
    public class ModLibraryGroupViewModel : LibraryGroupViewModel<ModLibraryViewModel>
    {
        readonly CustomCollectionContextMenu _customCollectionContextMenu;
        readonly LocalModFolderContextMenu _localModFolderContextMenu;
        readonly MultiLibraryItemContextMenu _multiContextMenu;
        readonly RepositoryOptionsContextMenu _repositoryOptionsContextMenu;

        public ModLibraryGroupViewModel(ModLibraryViewModel library, string header, string addHeader = null,
            string icon = null)
            : base(library, header, addHeader, icon) {
            _repositoryOptionsContextMenu = new RepositoryOptionsContextMenu(library);
            _customCollectionContextMenu = new CustomCollectionContextMenu(library);
            _localModFolderContextMenu = new LocalModFolderContextMenu(library);
            _multiContextMenu = new MultiLibraryItemContextMenu(library);
            this.WhenAnyValue(x => x.SelectedItem)
                .Cast<LibraryItemViewModel>()
                .Subscribe(HandleSingleMenu);

            this.WhenAnyObservable(x => x.SelectedItems.ItemsAdded,
                x => x.SelectedItems.ItemsRemoved)
                .Select(_ => Unit.Default)
                .Merge(this.WhenAnyObservable(x => x.SelectedItems.ShouldReset).Select(_ => Unit.Default))
                .Select(x => SelectedItems)
                .Subscribe(x => {
                    switch (x.Count) {
                    case 0:
                        ContextMenu = null;
                        break;
                    case 1:
                        HandleSingleMenu(x.OfType<LibraryItemViewModel>().First());
                        break;
                    default:
                        HandleMultiMenu(x.OfType<LibraryItemViewModel>().ToArray());
                        break;
                    }
                });
        }

        void HandleMultiMenu(IReadOnlyCollection<LibraryItemViewModel> items) {
            if (!items.Any()) {
                ContextMenu = null;
                return;
            }

            _multiContextMenu.ShowForItem(items);
            ContextMenu = _multiContextMenu;
        }

        void HandleSingleMenu(LibraryItemViewModel item) {
            if (item == null) {
                ContextMenu = null;
                return;
            }

            var collection = item as CollectionLibraryItemViewModel;
            if (collection != null) {
                _customCollectionContextMenu.ShowForItem(collection);
                ContextMenu = _customCollectionContextMenu;
                return;
            }

            var repo = item as ContentLibraryItemViewModel<SixRepo>;
            if (repo != null) {
                _repositoryOptionsContextMenu.ShowForItem(repo);
                ContextMenu = _repositoryOptionsContextMenu;
                return;
            }

            var local = item as ContentLibraryItemViewModel<LocalModsContainer>;
            if (local != null) {
                _localModFolderContextMenu.ShowForItem(local);
                ContextMenu = _localModFolderContextMenu;
                return;
            }

            ContextMenu = null;
        }
    }
}