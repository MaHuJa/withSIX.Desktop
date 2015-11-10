// <copyright company="SIX Networks GmbH" file="MultiContentContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class MultiContentContextMenu : ContextMenuBase<IReadOnlyCollection<IContent>>
    {
        public MultiContentContextMenu(ModLibraryViewModel library) {
            Library = library;
        }

        public ModLibraryViewModel Library { get; }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void RemoveFromActiveCollection(IReadOnlyCollection<IContent> content) {
            Library.RemoveFromCollection(content);
        }

        [MenuItem("Add selected to..."), DoNotObfuscate]
        public Task AddTo(IReadOnlyCollection<IContent> content) {
            return Library.OpenAddToCollectionsView(content.OfType<IMod>().ToArray());
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public void AddToActiveCollection(IReadOnlyCollection<IContent> content) {
            Library.AddToCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Share), DoNotObfuscate]
        public Task Share(IReadOnlyCollection<IContent> content) {
            return Library.ShareToContact(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task LaunchSelected(IReadOnlyCollection<IContent> content) {
            return Library.Launch(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Tools), DoNotObfuscate]
        public Task DiagnoseSelected(IReadOnlyCollection<IContent> content) {
            return Library.Diagnose(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task UninstallSelectedFromDisk(IReadOnlyCollection<IContent> content) {
            return Library.Uninstall(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public Task CreateCollectionWithSelected(IReadOnlyCollection<IContent> content) {
            return Library.AddCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void MakeRequired(IReadOnlyCollection<IContent> content) {
            SetRequired(content, true);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void MakeOptional(IReadOnlyCollection<IContent> content) {
            SetRequired(content, false);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void LockVersion(IReadOnlyCollection<IContent> content) {
            content.OfType<ToggleableModProxy>().ForEach(x => x.Lock());
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public void UnlockVersion(IReadOnlyCollection<IContent> content) {
            content.OfType<ToggleableModProxy>().ForEach(x => x.Unlock());
        }

        static void SetRequired(IReadOnlyCollection<IContent> content, bool isRequired) {
            content.OfType<ToggleableModProxy>().ForEach(x => x.IsRequired = isRequired);
        }

        protected override void UpdateItemsFor(IReadOnlyCollection<IContent> item) {
            base.UpdateItemsFor(item);

            Items.ForEach(x => x.IsVisible = true);

            var activeItem = Library.ActiveItem as Collection;
            var selectedItem = Library.SelectedItem as IHaveModel<Collection>;
            var customCollection = selectedItem == null ? null : selectedItem.Model as CustomCollection;
            var isCustomCollection = customCollection != null;
            var toggleableModProxies = item.OfType<ToggleableModProxy>().ToList();
            var commonReq = isCustomCollection && customCollection.AllowChanges() &&
                            item.Count == toggleableModProxies.Count();

            var sub = activeItem as SubscribedCollection;

            var requireReq = commonReq && !customCollection.AllModsRequired;
            GetItem(MakeRequired)
                .IsVisible = requireReq &&
                             toggleableModProxies.All(x => !x.IsRequired);
            GetItem(MakeOptional)
                .IsVisible = requireReq &&
                             toggleableModProxies.All(x => x.IsRequired);

            var lockReq = commonReq && !customCollection.AllModsLocked &&
                          !toggleableModProxies.Select(x => x.ToMod()).Any(x => x is CustomRepoMod || x is LocalMod);
            GetItem(LockVersion)
                .IsVisible = lockReq &&
                             toggleableModProxies.All(x => !x.IsVersionLocked);
            GetItem(UnlockVersion)
                .IsVisible = lockReq &&
                             toggleableModProxies.All(x => x.IsVersionLocked);

            if (item.Count == toggleableModProxies.Count()) {
                GetItem(AddToActiveCollection)
                    .IsVisible = false;
            } else {
                var allMods = item.All(x => x is Mod);
                if (allMods) {
                    GetItem(AddToActiveCollection)
                        .IsVisible = activeItem != null && sub == null;
                    GetItem(RemoveFromActiveCollection)
                        .IsVisible = false;
                }
            }

            if (item.Any(x => x is Collection)) {
                GetItem(RemoveFromActiveCollection)
                    .IsVisible = false;
                GetItem(AddToActiveCollection)
                    .IsVisible = false;
                GetAsyncItem(AddTo)
                    .IsVisible = false;
                GetAsyncItem(CreateCollectionWithSelected)
                    .IsVisible = false;
                Items.First(x => x.AsyncAction == Share)
                    .IsVisible = false;
                GetAsyncItem(LaunchSelected)
                    .IsVisible = false;
            } else {
                if (item.Any(x => x is LocalMod || x is CustomRepoMod)) {
                    GetAsyncItem(Share)
                        .IsVisible = false;
                }
            }


            if (!item.All(x => x.State == ContentState.Uptodate || x.State == ContentState.UpdateAvailable)) {
                GetAsyncItem(UninstallSelectedFromDisk)
                    .IsVisible = false;
            }
            if (
                !item.All(
                    x =>
                        x.State != ContentState.NotInstalled && x.State != ContentState.Local && !(x is LocalMod) &&
                        !(x is Collection))) {
                GetAsyncItem(DiagnoseSelected)
                    .IsVisible = false;
            }
            GetAsyncItem(AddTo)
                .IsVisible = false;
            GetAsyncItem(Share)
                .IsVisible = false;
            GetAsyncItem(LaunchSelected)
                .IsVisible = false;
        }
    }
}