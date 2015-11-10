// <copyright company="SIX Networks GmbH" file="MissionLibraryGroupViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup
{
    public class MissionLibraryGroupViewModel : LibraryGroupViewModel<MissionLibraryViewModel>
    {
        readonly LocalMissionFolderContextMenu _localMissionFolderContextMenu;

        public MissionLibraryGroupViewModel(MissionLibraryViewModel library, string header, string addHeader = null,
            string icon = null) : base(library, header, addHeader, icon) {
            _localMissionFolderContextMenu = new LocalMissionFolderContextMenu(library);
            this.WhenAnyValue(x => x.SelectedItem)
                .Cast<LibraryItemViewModel>()
                .Subscribe(HandleContextMenu);
        }

        void HandleContextMenu(LibraryItemViewModel item) {
            if (item == null) {
                ContextMenu = null;
                return;
            }

            var local = item as ContentLibraryItemViewModel<LocalMissionsContainer>;
            if (local != null) {
                _localMissionFolderContextMenu.ShowForItem(local);
                ContextMenu = _localMissionFolderContextMenu;
                return;
            }

            ContextMenu = null;
        }
    }
}