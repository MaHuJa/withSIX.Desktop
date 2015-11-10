// <copyright company="SIX Networks GmbH" file="Content.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class Content : ContentBase, IContent
    {
        List<Network> _networks;
        int _searchScore;
        ContentState _state;
        [DataMember] string _Version;

        protected Content(Guid id) : base(id) {
            Children = new ReactiveList<IHierarchicalLibraryItem>();
        }

        public List<Network> Networks
        {
            get { return _networks; }
            set { SetProperty(ref _networks, value); }
        }
        public virtual string Version
        {
            get { return _Version; }
            set { SetProperty(ref _Version, value); }
        }
        public virtual bool IsCustomContent
        {
            get { return false; }
        }

        public virtual Uri ProfileUrl() {
            return Tools.Transfer.JoinUri(CommonUrls.PlayUrl, GetPlayPath());
        }

        public virtual Uri GetChangelogUrl() {
            return Tools.Transfer.JoinUri(CommonUrls.ContentUrl, GetMyApiPath(), "changelog");
        }

        public ContentState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
        public int SearchScore
        {
            get { return _searchScore; }
            set { SetProperty(ref _searchScore, value); }
        }

        [DoNotObfuscate]
        public void ToggleFavorite() {
            IsFavorite = !IsFavorite;
        }

        string GetPlayPath() {
            return Tools.Transfer.JoinPaths(GetGameSlug(), GetSlugType(), Name.Sluggify());
        }

        protected virtual string GetSlugType() {
            return GetType().Name.ToUnderscore() + "s";
        }

        protected virtual string GetGameSlug() {
            return "arma-2";
        }

        string GetMyApiPath() {
            return Tools.Transfer.JoinPaths(GetApiPath(GetType().Name.ToUnderscore() + "s"), Id);
        }

        #region IHierarchicalLibraryItem

        public ReactiveList<IHierarchicalLibraryItem> Children { get; }
        public ICollectionView ChildrenView { get; private set; }
        public IHierarchicalLibraryItem SelectedItem { get; set; }
        public ObservableCollection<object> SelectedItemsInternal { get; set; }

        public void ClearSelection() {
            SelectedItemsInternal.Clear();
        }

        object IHaveSelectedItem.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (IHierarchicalLibraryItem) value; }
        }
        public ICollectionView ItemsView { get; private set; }

        #endregion
    }
}