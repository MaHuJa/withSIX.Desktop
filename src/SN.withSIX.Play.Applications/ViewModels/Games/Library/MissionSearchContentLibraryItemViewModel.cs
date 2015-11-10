// <copyright company="SIX Networks GmbH" file="MissionSearchContentLibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public class MissionSearchContentLibraryItemViewModel : SearchContentLibraryItemViewModel
    {
        public MissionSearchContentLibraryItemViewModel(MissionLibraryViewModel library) : base(library) {
            MissionContextMenu = new MissionContextMenu(library);
            MissionFolderContextMenu = new MissionFolderContextMenu(library);

            MissionBarMenu = new MissionContextMenu(library);
            MissionFolderBarMenu = new MissionFolderContextMenu(library);

            SetupMenus(HandleSingleMenu, x => ContextMenu = null);
        }

        public MissionFolderContextMenu MissionFolderContextMenu { get; }
        public MissionContextMenu MissionContextMenu { get; }
        public MissionFolderContextMenu MissionFolderBarMenu { get; }
        public MissionContextMenu MissionBarMenu { get; }

        void HandleSingleMenu(IContent first) {
            var folder = first as MissionFolder;
            if (folder != null) {
                MissionFolderContextMenu.ShowForItem(folder);
                MissionFolderBarMenu.ShowForItem(folder);
                ContextMenu = MissionFolderContextMenu;
                BarMenu = MissionFolderBarMenu;
            } else {
                var mission = first as Mission;
                if (mission != null) {
                    MissionContextMenu.ShowForItem(mission);
                    MissionBarMenu.ShowForItem(mission);
                }
                BarMenu = MissionBarMenu;
                ContextMenu = MissionContextMenu;
            }
        }
    }
}