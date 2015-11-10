// <copyright company="SIX Networks GmbH" file="ModShortcutMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class ModShortcutMenu : MenuItem<CollectionLibraryItemViewModel>
    {
        public ModShortcutMenu(object parent) {
            Contract.Requires<ArgumentNullException>(parent != null);
            Parent = (CustomCollectionContextMenu) parent;
        }

        public CustomCollectionContextMenu Parent { get; set; }

        [MenuItem, DoNotObfuscate]
        public Task CreateDesktopShortcut(CollectionLibraryItemViewModel content) {
            return Parent.Library.CreateShortcutGame(content.Model);
        }

        [MenuItem("Create desktop shortcut through PwS; Update and Launch"), DoNotObfuscate]
        public Task CreateDesktopShortcutThroughPws(CollectionLibraryItemViewModel content) {
            return Parent.Library.CreateShortcutPws(content.Model);
        }

        [MenuItem("Create desktop shortcut through PwS; Update and Join"), DoNotObfuscate]
        public Task CreateDesktopShortcutThroughPwsJoin(CollectionLibraryItemViewModel content) {
            return Parent.Library.CreateShortcutPwsJoin(content.Model);
        }

        [MenuItem("Create desktop shortcut through PwS in lockdown"), DoNotObfuscate]
        public Task CreateDesktopShortcutThroughPwsLockdown(CollectionLibraryItemViewModel content) {
            return Parent.Library.CreateShortcutPwsLockdown(content.Model);
        }

        protected override void UpdateItemsFor(CollectionLibraryItemViewModel item) {
            base.UpdateItemsFor(item);

            GetAsyncItem(CreateDesktopShortcut)
                .IsEnabled = DomainEvilGlobal.SelectedGame.ActiveGame.InstalledState.IsInstalled;
        }
    }
}