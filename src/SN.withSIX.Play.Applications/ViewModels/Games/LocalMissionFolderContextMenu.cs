// <copyright company="SIX Networks GmbH" file="LocalMissionFolderContextMenu.cs">
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
    public class LocalMissionFolderContextMenu : MissionLibraryItemMenuBase<LocalMissionsContainer>
    {
        public LocalMissionFolderContextMenu(MissionLibraryViewModel library) : base(library) {
            Contract.Requires<ArgumentNullException>(library != null);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Folder), DoNotObfuscate]
        public void ShowInExplorer(ContentLibraryItemViewModel<LocalMissionsContainer> contentItem) {
            Tools.FileUtil.OpenFolderInExplorer(contentItem.Model.Path);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task RemoveLocalFolder(ContentLibraryItemViewModel<LocalMissionsContainer> contentItem) {
            return contentItem.Remove();
        }

        [MenuItem, DoNotObfuscate]
        public void Move(ContentLibraryItemViewModel<LocalMissionsContainer> contentItem) {
            Library.MoveLocalMissionDirectory(contentItem);
        }

        protected override void UpdateItemsFor(ContentLibraryItemViewModel<LocalMissionsContainer> lm) {
            GetAsyncItem(RemoveLocalFolder)
                .IsEnabled = !lm.IsFeatured;

            GetItem(Move)
                .IsEnabled = lm.IsFeatured;
        }
    }
}