// <copyright company="SIX Networks GmbH" file="BundleItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class BundleItem : PropertyChangedBase, IComparePK<BundleItem>
    {
        Bundle _current;
        Dependency _currentDependency;
        bool _isPinned;
        bool _isSelected;

        public BundleItem(string name, RepositoryHandler handler, IEnumerable<SpecificVersion> bundles = null) {
            Handler = handler;
            Name = name;
            Bundles = new ReactiveList<SpecificVersion>(bundles);
            //CurrentDependency = GetLatestDependency();
        }

        public string Name { get; protected set; }
        public ReactiveList<SpecificVersion> Bundles { get; set; }
        public RepositoryHandler Handler { get; protected set; }
        public Bundle Current
        {
            get { return _current; }
            set
            {
                if (!SetProperty(ref _current, value))
                    return;
                CurrentDependency = value == null ? null : value.ToSpecificVersion().ToDependency();
            }
        }
        public Dependency CurrentDependency
        {
            get { return _currentDependency; }
            set { SetProperty(ref _currentDependency, value); }
        }
        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool ComparePK(object other) {
            var o = other as BundleItem;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(BundleItem other) {
            return other != null && other.Name != null && other.Name.Equals(Name);
        }

        public async Task<Bundle> UpdateCurrent(SpecificVersion collection) {
            var hasBundle = Handler.Repository.HasBundle(collection);
            if (!hasBundle) {
                if (Handler.Remote)
                    await Handler.BundleManager.GetAndAddBundle(collection).ConfigureAwait(false);
                else
                    collection = null;
            }
            return Current = collection != null ? Handler.BundleManager.GetMetaData(collection) : null;
        }

        public void OpenInCommandPrompt() {
            throw new NotImplementedException();
        }

        public void OpenInExplorer() {
            throw new NotImplementedException();
        }
    }
}