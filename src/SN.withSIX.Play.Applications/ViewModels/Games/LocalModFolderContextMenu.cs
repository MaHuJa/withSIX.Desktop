// <copyright company="SIX Networks GmbH" file="LocalModFolderContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class LocalModFolderContextMenu : ModLibraryItemMenuBase<LocalModsContainer>
    {
        public LocalModFolderContextMenu(ModLibraryViewModel library) : base(library) {
            Contract.Requires<ArgumentNullException>(library != null);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Folder), DoNotObfuscate]
        public void ShowInExplorer(ContentLibraryItemViewModel<LocalModsContainer> contentItem) {
            Tools.FileUtil.OpenFolderInExplorer(contentItem.Model.Path);
        }

        [MenuItem, DoNotObfuscate]
        public void EditLocation(ContentLibraryItemViewModel<LocalModsContainer> contentItem) {
            Library.MoveLocalModDirectory(contentItem);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task RemoveLocation(ContentLibraryItemViewModel contentItem) {
            return contentItem.Remove();
        }

        protected override void UpdateItemsFor(ContentLibraryItemViewModel<LocalModsContainer> lm) {
            GetAsyncItem(RemoveLocation)
                .IsEnabled = !lm.IsFeatured;

            GetItem(EditLocation)
                .IsEnabled = lm.IsFeatured;
        }
    }
}