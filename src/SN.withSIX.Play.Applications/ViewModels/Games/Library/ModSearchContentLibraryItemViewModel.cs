// <copyright company="SIX Networks GmbH" file="ModSearchContentLibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class ModSearchContentLibraryItemViewModel : SearchContentLibraryItemViewModel
    {
        readonly CollectionBarMenu _collectionBarMenu;
        readonly CollectionContextMenu _collectionContextMenu;
        readonly ModBarMenu _modBarMenu;
        readonly ModContextMenu _modContextMenu;
        readonly MultiContentContextMenu _multiContentContextMenu;

        public ModSearchContentLibraryItemViewModel(ModLibraryViewModel library) : base(library) {
            _collectionContextMenu = new CollectionContextMenu(library);
            _modContextMenu = new ModContextMenu(library);
            _multiContentContextMenu = new MultiContentContextMenu(library);
            _modBarMenu = new ModBarMenu(library);
            _collectionBarMenu = new CollectionBarMenu(library);

            SetupMenus(HandleSingleMenu, HandleMultiMenu);
        }

        void HandleMultiMenu(IReadOnlyCollection<IContent> items) {
            _multiContentContextMenu.ShowForItem(items);
            ContextMenu = _multiContentContextMenu;
        }

        void HandleSingleMenu(IContent first) {
            var mod = first as IMod;
            if (mod != null) {
                _modContextMenu.ShowForItem(mod);
                ContextMenu = _modContextMenu;
                _modBarMenu.ShowForItem(mod);
                BarMenu = _modBarMenu;
            } else {
                var collection = first as Collection;
                if (collection != null) {
                    _collectionContextMenu.ShowForItem(collection);
                    _collectionBarMenu.ShowForItem(collection);
                }
                ContextMenu = _collectionContextMenu;
                BarMenu = _collectionBarMenu;
            }
        }
    }
}