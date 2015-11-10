// <copyright company="SIX Networks GmbH" file="IContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public interface IHaveSelectedItem
    {
        object SelectedItem { get; set; }
    }

    public interface IHaveSelectedItems
    {
        ObservableCollection<object> SelectedItemsInternal { get; set; }
        void ClearSelection();
    }

    public interface IHaveSelectedItemsView : IHaveSelectedItems, IHaveSelectedItem
    {
        ICollectionView ItemsView { get; }
    }

    public interface IHierarchicalLibraryItem : IHaveSelectedItemsView
    {
        ReactiveList<IHierarchicalLibraryItem> Children { get; }
        ICollectionView ChildrenView { get; }
        IHierarchicalLibraryItem SelectedItem { get; set; }
    }

    [DoNotObfuscateType, Obsolete("Can be removed once ToggleableModProxy/IMod is changed")]
    public interface IContent : IHaveNotes, IFavorite, IHaveTimestamps, IComparePK<SyncBase>, INotifyPropertyChanged,
        ISearchScore, IToggleFavorite, IHierarchicalLibraryItem
    {
        Guid Id { get; }
        string Version { get; set; }
        string Name { get; set; }
        string[] Categories { get; set; }
        string HomepageUrl { get; set; }
        bool HasImage { get; set; }
        string Image { get; set; }
        string ImageLarge { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        ContentState State { get; set; }
        bool IsCustomContent { get; }
        Uri ProfileUrl();
        Uri GetChangelogUrl();
    }
}