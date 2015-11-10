// <copyright company="SIX Networks GmbH" file="CustomCollectionContextMenu.cs">
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
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class CustomCollectionContextMenu : ContextMenuBase<CollectionLibraryItemViewModel>
    {
        readonly UploadCollection _uploadCollection;

        public CustomCollectionContextMenu(ModLibraryViewModel library) {
            Library = library;
            _uploadCollection = library.UploadCollection;
            this.WhenAnyValue(x => x.CurrentItem.Model.IsInstalled, x => x.CurrentItem.Model.Items.Count,
                (installed, count) => installed && count > 0)
                .Subscribe(isInstalled =>
                    Items.Where(
                        x =>
                            x.Action == UninstallModsFromDisk || x.AsyncAction == Diagnose ||
                            x.AsyncAction == LaunchCollection)
                        .ForEach(x => x.IsEnabled = isInstalled));
        }

        internal ModLibraryViewModel Library { get; }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void ActivateCollection(CollectionLibraryItemViewModel content) {
            Library.ActiveItem = content.Model;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task LaunchCollection(CollectionLibraryItemViewModel content) {
            return Library.LaunchCollection(content);
        }

        [MenuItem(type: typeof (ModShortcutMenu)), DoNotObfuscate]
        public void CreateDesktopShortcut(CollectionLibraryItemViewModel content) {}

        [MenuItem, DoNotObfuscate]
        public Task Publish(CollectionLibraryItemViewModel content) {
            return _uploadCollection.Upload(content as CustomCollectionLibraryItemViewModel);
        }

        [MenuItem, DoNotObfuscate]
        public void UninstallModsFromDisk(CollectionLibraryItemViewModel content) {
            Library.UninstallCollection(content);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Notes), DoNotObfuscate]
        public void ShowNotes(CollectionLibraryItemViewModel content) {
            Library.ShowCollectionNotes(content);
        }

        [MenuItem, DoNotObfuscate]
        public Task Diagnose(CollectionLibraryItemViewModel content) {
            return Library.Diagnose(content.Model);
        }

        [MenuItem("Copy (fork) collection"), DoNotObfuscate]
        public void ForkCollection(CollectionLibraryItemViewModel content) {
            Library.CloneCollection(content);
        }

        [MenuItem, DoNotObfuscate]
        public void RenameCollection(CollectionLibraryItemViewModel content) {
            content.IsEditing = true;
        }

        [MenuItem, DoNotObfuscate]
        public void ClearCollection(CollectionLibraryItemViewModel content) {
            Library.ClearCollection(content);
        }

        [MenuItem, DoNotObfuscate]
        public void ClearCustomizations(CollectionLibraryItemViewModel content) {
            Library.ClearCustomizations(content);
        }

        [MenuItem, DoNotObfuscate]
        public Task RemoveCollection(CollectionLibraryItemViewModel content) {
            return content.Remove();
        }

        [MenuItem, DoNotObfuscate]
        public Task Unsubscribe(CollectionLibraryItemViewModel item) {
            return Library.Unsubscribe((SubscribedCollectionLibraryItemViewModel) item);
        }

        [MenuItem, DoNotObfuscate]
        public Task RefreshCustomRepoInfo(CollectionLibraryItemViewModel content) {
            return Library.RefreshCustomRepoInfo(content);
        }

        [MenuItem, DoNotObfuscate]
        public void ShowInfo(CollectionLibraryItemViewModel obj) {
            obj.ViewOnline();
        }

        protected override void UpdateItemsFor(CollectionLibraryItemViewModel item) {
            var hasCustomRepo = false;
            var cm = item.Model as CustomCollection;
            if (cm != null)
                hasCustomRepo = cm.HasCustomRepo();
            var sc = item.Model as SubscribedCollection;
            if (sc != null)
                hasCustomRepo = hasCustomRepo || sc.Repositories.Any();

            GetAsyncItem(Publish)
                .IsVisible = item.IsPublishable;

            GetItem(RenameCollection)
                .IsVisible = !item.IsFeatured && !item.IsSubscribedCollection && !hasCustomRepo;
            GetAsyncItem(RemoveCollection)
                .IsVisible = !item.IsFeatured && !item.IsSubscribedCollection;
            GetItem(ClearCollection)
                .IsVisible = !item.IsFeatured && !hasCustomRepo && !item.IsSubscribedCollection;
            GetItem(ClearCustomizations)
                .IsVisible = !item.IsFeatured && !item.IsSubscribedCollection;

            GetItem(ForkCollection)
                .IsVisible = !hasCustomRepo;

            GetItem(ShowInfo)
                .IsVisible = item.IsHosted;

            GetAsyncItem(RefreshCustomRepoInfo)
                .IsVisible = hasCustomRepo;

            GetAsyncItem(Unsubscribe)
                .IsVisible = item.IsSubscribedCollection;
        }
    }
}