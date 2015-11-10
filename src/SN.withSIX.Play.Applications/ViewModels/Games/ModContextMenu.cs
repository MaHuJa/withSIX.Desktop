// <copyright company="SIX Networks GmbH" file="ModContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MoreLinq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class ModContextMenu : ModMenuBase<IMod>
    {
        public ModContextMenu(ModLibraryViewModel library) : base(library) {
            this.WhenAnyValue(x => x.CurrentItem.Controller.IsInstalled)
                .Select(x => new {CurrentItem, x})
                .Subscribe(info => {
                    Items.Where(
                        x =>
                            x.AsyncAction == UninstallFromDisk || x.AsyncAction == LaunchMod ||
                            x.Action == OpenInExplorer)
                        .ForEach(x => x.IsVisible = info.x);

                    var installAction = GetAsyncItem(Diagnose);
                    installAction.Name = ModController.ConvertState(info.CurrentItem.State);
                    installAction.IsVisible = installAction.Name != null && !(info.CurrentItem is LocalMod);
                });
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void ShowInfo(IMod content) {
            Library.ShowInfo(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void UseMod(IMod content) {
            Library.ActiveItem = content;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void SelectVersion(IMod content) {
            Library.ShowVersion(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public Task CreateCollectionWithMod(IMod content) {
            return Library.AddCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task LaunchMod(IMod content) {
            return Library.Launch(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Folder), DoNotObfuscate]
        public void OpenInExplorer(IMod content) {
            Tools.FileUtil.OpenFolderInExplorer(content.Controller.Path);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Tools), DoNotObfuscate]
        public Task Diagnose(IMod content) {
            return Library.Diagnose(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task UninstallFromDisk(IMod content) {
            return Library.Uninstall(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Notes), DoNotObfuscate]
        public void ShowNotes(IMod content) {
            Library.ShowNotes(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public void AddToActiveCollection(IMod content) {
            Library.AddToCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void RemoveFromActiveCollection(IMod content) {
            Library.RemoveFromCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void RemoveFromCollection(IMod content) {
            Library.RemoveFromSelectedCollection(content);
        }

        [MenuItem("Add to..."), DoNotObfuscate]
        public Task AddTo(IMod content) {
            return Library.OpenAddToCollectionsView(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void MakeRequired(IMod content) {
            SetRequired(content, true);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void MakeOptional(IMod content) {
            SetRequired(content, false);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void LockVersion(IMod content) {
            var tm = (ToggleableModProxy) content;
            tm.Lock();
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void UnlockVersion(IMod content) {
            var tm = (ToggleableModProxy) content;
            tm.Unlock();
        }

        static void SetRequired(IMod content, bool isRequired) {
            var tm = (ToggleableModProxy) content;
            tm.IsRequired = isRequired;
        }

        protected override void UpdateItemsFor(IMod item) {
            var isCustomRepoMod = item.ToMod() is CustomRepoMod;
            GetAsyncItem(CreateCollectionWithMod)
                .IsEnabled = !isCustomRepoMod;

            var activeItem = Library.ActiveItem as Collection;
            var selectedItem = Library.SelectedItem as IHaveModel<Collection>;
            var inSelectedItem = selectedItem != null &&
                                 selectedItem.Model.ModItems()
                                     .Any(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

            var equal = selectedItem != null && selectedItem.Model == activeItem;
            var inActiveItem = activeItem != null &&
                               activeItem.ModItems()
                                   .Any(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

            var sub = activeItem as SubscribedCollection;
            var tm = item as ToggleableModProxy;
            var isRequired = tm != null && tm.IsRequired;

            GetItem(AddToActiveCollection)
                .IsVisible = activeItem != null && !equal && !inActiveItem && sub == null;
            GetItem(RemoveFromActiveCollection)
                .IsVisible = activeItem != null && inActiveItem && sub == null && !isRequired;
            var isLocalMod = item is LocalMod;
            GetAsyncItem(AddTo)
                .IsVisible = !isLocalMod;

            var customCollection = selectedItem == null ? null : selectedItem.Model as CustomCollection;
            var isCustomCollection = customCollection != null;

            GetItem(RemoveFromCollection)
                .IsVisible = selectedItem != null && inSelectedItem && sub == null && !isRequired;

            var commonReq = isCustomCollection && customCollection.AllowChanges() && tm != null;

            GetItem(SelectVersion)
                .IsVisible = commonReq;

            var requiredReq = commonReq && !customCollection.AllModsRequired;
            GetItem(MakeRequired)
                .IsVisible = requiredReq && !tm.IsRequired;
            GetItem(MakeOptional)
                .IsVisible = requiredReq && tm.IsRequired;

            var mod = tm.ToMod();
            var lockedReq = commonReq && !customCollection.AllModsLocked && !(mod is CustomRepoMod) &&
                            !(mod is LocalMod);
            GetItem(LockVersion)
                .IsVisible = lockedReq && !tm.IsVersionLocked;
            GetItem(UnlockVersion)
                .IsVisible = lockedReq && tm.IsVersionLocked;
        }
    }
}