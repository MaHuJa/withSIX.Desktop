// <copyright company="SIX Networks GmbH" file="CollectionContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class CollectionContextMenu : ModMenuBase<Collection>
    {
        public CollectionContextMenu(ModLibraryViewModel library) : base(library) {
            this.WhenAnyValue(x => x.CurrentItem.IsInstalled)
                .Subscribe(isInstalled => Items.Where(
                    x => x.AsyncAction == UninstallModsFromDisk || x.AsyncAction == DiagnoseCollection ||
                         x.AsyncAction == LaunchModSet)
                    .ForEach(y => y.IsEnabled = isInstalled));
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void ActivateModset(Collection content) {
            Library.ActiveItem = content;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public Task CreateCollectionWithModset(Collection content) {
            return Library.AddCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task LaunchModSet(Collection content) {
            return Library.Launch(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task UninstallModsFromDisk(Collection content) {
            return Library.Uninstall(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Notes), DoNotObfuscate]
        public void ShowNotes(Collection content) {
            Library.ShowNotes(content);
        }

        [MenuItem, DoNotObfuscate]
        public Task DiagnoseCollection(Collection content) {
            return Library.Diagnose(content);
        }
    }
}