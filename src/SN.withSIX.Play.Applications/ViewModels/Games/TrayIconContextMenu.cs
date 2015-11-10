// <copyright company="SIX Networks GmbH" file="TrayIconContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using MoreLinq;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class RecentMenuItem : MenuItem
    {
        public RecentMenuItem(RecentCollection collection) {
            Id = collection.Id;
            Name = collection.Name;
            Action = () => Process.Start(collection.GetLaunchUrl());
        }

        public Guid Id { get; }
    }

    public class RecentContextMenu : MenuItem
    {
        public RecentContextMenu() {
            Name = "Recent";
            Icon = SixIconFont.withSIX_icon_Clock;

            DomainEvilGlobal.Settings.ModOptions.RecentCollections.CopyAndTrackChangesOnUiThread(
                AddRecentItem,
                RemoveRecentItem,
                reset => {
                    Items.Clear();
                    reset.ForEach(AddRecentItem);
                });
        }

        void AddRecentItem(RecentCollection collection) {
            RemoveRecentItem(collection);
            InsertRecentItem(collection);

            if (Items.Count > 10)
                Items.RemoveAt(10);
        }

        void InsertRecentItem(RecentCollection collection) {
            Items.Insert(0, new RecentMenuItem(collection));
        }

        void RemoveRecentItem(RecentCollection collection) {
            Items.RemoveAll<IMenuItem, RecentMenuItem>(x => x.Id == collection.Id);
        }
    }

    public class TrayIconContextMenu : ContextMenuBase
    {
        readonly IPlayShellViewModel _psvm;

        public TrayIconContextMenu(IPlayShellViewModel playShellViewModel) {
            _psvm = playShellViewModel;

            Items.InsertRange(0, new[] {
                new RecentContextMenu(),
                new MenuItem {IsSeparator = true}
            });
        }

        [MenuItem, DoNotObfuscate]
        public void Open() {
            _psvm.TrayIconDoubleclicked.Execute(null);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void Exit() {
            _psvm.Exit.Execute(null);
        }
    }
}