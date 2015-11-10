// <copyright company="SIX Networks GmbH" file="ContentContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    /*
     * Problems:
     * - What happens when a mod gets added to the SyncManager with the name of an existing Local Mod?
     *      At the moment the Mod Container will never handle this.
     * - What happens when we want to handle a mod with another repository, or a mod is in a repository but not in the syncManager?
     * - Local Mods are also given GUID's this may be an issue, especially if a local mod turns out to be a syncMod
     */

    public abstract class ContentContainer<TItem, TLegacyItem> : PropertyChangedBase, ISupportListing<TItem>
    {
        protected readonly List<TItem> _content;
        IReadOnlyCollection<TItem> _readOnlyCollection;

        protected ContentContainer()
            : this(new List<TItem>()) {}

        protected ContentContainer(List<TItem> content) {
            _content = content;
            UpdateReadonlyCollection();
        }

        public IReadOnlyCollection<TItem> List {
            get { return _readOnlyCollection; }
            private set { SetProperty(ref _readOnlyCollection, value); }
        }

        public void UpdateContent(IEnumerable<TLegacyItem> items) {
            _content.Clear();
            _content.AddRange(ConstructItems(items));
            UpdateReadonlyCollection();
        }

        protected abstract IEnumerable<TItem> ConstructItems(IEnumerable<TLegacyItem> items);

        void UpdateReadonlyCollection() {
            List = _content.ToList().AsReadOnly();
        }
    }

    public abstract class CollectionContentContainer<TItem> : ContentContainer<TItem, Collection> {}
}