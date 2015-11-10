// <copyright company="SIX Networks GmbH" file="SelectionCollectionHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public class SelectionCollectionHelper<T> : ReactiveObject, ISelectionCollectionHelper<T>
    {
        [DoNotObfuscate] readonly ObservableAsPropertyHelper<bool> _isItemSelected;
        readonly object _lock = new object();
        T _selectedItem;

        public SelectionCollectionHelper()
            : this(Enumerable.Empty<T>()) {}

        public SelectionCollectionHelper(ReactiveList<T> items, T selectedItem = default(T)) {
            Items = items;
            _selectedItem = selectedItem;

            _isItemSelected = this.WhenAnyValue(x => x.SelectedItem)
                .Select(x => x != null)
                .ToProperty(this, x => x.IsItemSelected, false, Scheduler.Immediate);
        }

        public SelectionCollectionHelper(IEnumerable<T> items, T selectedItem = default(T))
            : this(new ReactiveList<T>(items) {
                ChangeTrackingEnabled = true
            }, selectedItem) {}

        public bool IsItemSelected
        {
            get { return _isItemSelected.Value; }
        }
        public T SelectedItem
        {
            get { return _selectedItem; }
            set { this.RaiseAndSetIfChanged(ref _selectedItem, value); }
        }
        public ReactiveList<T> Items { get; protected set; }

        public T SelectNext(bool selectNullIfNoneExists = true) {
            if (IsItemSelected) {
                var index = Items.IndexOf(SelectedItem);
                if (Items.Count > (index + 1))
                    SelectedItem = Items[index + 1];
                else if (selectNullIfNoneExists)
                    SelectedItem = default(T);
            } else if (Items.Count > 0)
                SelectedItem = Items[0];
            return SelectedItem;
        }

        public T SelectPrevious() {
            if (IsItemSelected) {
                var index = Items.IndexOf(SelectedItem);
                SelectedItem = ((index - 1) >= 0) ? Items[index - 1] : default(T);
            } else if (Items.Count > 0)
                SelectedItem = Items[Items.Count - 1];
            return SelectedItem;
        }
    }
}