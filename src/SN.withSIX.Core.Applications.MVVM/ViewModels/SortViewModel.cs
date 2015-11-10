// <copyright company="SIX Networks GmbH" file="SortViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public interface ISortDescriptions
    {
        SortData[] Columns { get; }
        SortDescriptionCollection SortDescriptions { get; }
        bool SortVisible { get; set; }
        SortData SelectedSort { get; set; }
        void ToggleVisibility();
    }

    [DoNotObfuscate]
    public class SortViewModel : PropertyChangedBase, ISortDescriptions
    {
        readonly SortData[] _requiredColumns;
        SortData _selectedSort;
        bool _sortVisible;

        public SortViewModel(ICollectionView source, SortData[] columns, IEnumerable<SortDescription> existing = null,
            SortData[] requiredColumns = null) {
            View = source;
            Columns = columns;
            _requiredColumns = requiredColumns ?? new SortData[0];
            if (existing != null) {
                var d = View.DeferRefresh();
                SortDescriptions.Clear();
                foreach (var c in _requiredColumns)
                    SortDescriptions.Add(c.ToSortDescription());
                var item = existing
                    .FirstOrDefault(x => Columns.Select(y => y.Value).Contains(x.PropertyName));
                if (item != default(SortDescription)) {
                    SelectedSort = Columns.First(x => x.Value == item.PropertyName);
                    SelectedSort.SortDirection = item.Direction;
                    SortDescriptions.Add(item);
                }

                d.Dispose();
            } else {
                var item = SortDescriptions.FirstOrDefault(x => Columns.Select(y => y.Value).Contains(x.PropertyName));
                if (item != default(SortDescription)) {
                    SelectedSort = Columns.First(x => x.Value == item.PropertyName);
                    SelectedSort.SortDirection = item.Direction;
                }
            }

            OldSelectedSort = SelectedSort;

            this.WhenAnyValue(x => x.SelectedSort)
                .Skip(1)
                .Subscribe(x => SortColumn());
            this.SetCommand(x => x.SortCommand).Subscribe(x => SortColumn());
            this.SetCommand(x => x.ToggleVisibilityCommand).Subscribe(x => ToggleVisibility());
        }

        public ReactiveCommand SortCommand { get; private set; }
        public ReactiveCommand ToggleVisibilityCommand { get; private set; }
        public ICollectionView View { get; }
        SortData OldSelectedSort { get; set; }
        public SortData[] Columns { get; }
        public SortDescriptionCollection SortDescriptions
        {
            get { return View.SortDescriptions; }
        }
        public bool SortVisible
        {
            get { return _sortVisible; }
            set { SetProperty(ref _sortVisible, value); }
        }
        public SortData SelectedSort
        {
            get { return _selectedSort; }
            set { SetProperty(ref _selectedSort, value); }
        }

        [ReportUsage]
        public void ToggleVisibility() {
            SortVisible = !SortVisible;
        }

        public void SetSort(string value) {
            UiHelper.TryOnUiThread(() => { SelectedSort = Columns.First(x => x.Value == value); });
        }

        [ReportUsage]
        void SortColumn() {
            var data = SelectedSort;
            var d = View.DeferRefresh();

            _requiredColumns.Select(x => x.ToSortDescription()).SyncCollection(SortDescriptions);

            if (data != null) {
                if (OldSelectedSort == data) {
                    data.SortDirection = data.SortDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }
                SortDescriptions.Add(data.ToSortDescription());
            }
            OldSelectedSort = data;

            d.Dispose();
        }
    }
}