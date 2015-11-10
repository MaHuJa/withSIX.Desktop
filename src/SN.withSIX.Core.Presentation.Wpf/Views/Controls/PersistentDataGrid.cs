// <copyright company="SIX Networks GmbH" file="PersistentDataGrid.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class PersistentDataGrid : DataGrid
    {
        public static readonly DependencyProperty ColumnInfoProperty = DependencyProperty.Register("ColumnInfo",
            typeof (
                ObservableCollection
                    <ColumnInfo>),
            typeof (
                PersistentDataGrid
                ),
            new FrameworkPropertyMetadata
                (null,
                    FrameworkPropertyMetadataOptions
                        .
                        BindsTwoWayByDefault,
                    ColumnInfoChangedCallback)
            );
        bool inWidthChange;
        bool updatingColumnInfo;
        public ObservableCollection<ColumnInfo> ColumnInfo
        {
            get { return (ObservableCollection<ColumnInfo>) GetValue(ColumnInfoProperty); }
            set { SetValue(ColumnInfoProperty, value); }
        }

        static void ColumnInfoChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e) {
            var grid = (PersistentDataGrid) dependencyObject;
            if (!grid.updatingColumnInfo)
                grid.ColumnInfoChanged();
        }

        protected override void OnInitialized(EventArgs e) {
            EventHandler sortDirectionChangedHandler = (sender, x) => UpdateColumnInfo();
            EventHandler widthPropertyChangedHandler = (sender, x) => inWidthChange = true;
            var sortDirectionPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(DataGridColumn.SortDirectionProperty, typeof (DataGridColumn));
            var widthPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof (DataGridColumn));

            Loaded += (sender, x) => {
                foreach (var column in Columns) {
                    sortDirectionPropertyDescriptor.AddValueChanged(column, sortDirectionChangedHandler);
                    widthPropertyDescriptor.AddValueChanged(column, widthPropertyChangedHandler);
                }
            };
            Unloaded += (sender, x) => {
                foreach (var column in Columns) {
                    sortDirectionPropertyDescriptor.RemoveValueChanged(column,
                        sortDirectionChangedHandler);
                    widthPropertyDescriptor.RemoveValueChanged(column, widthPropertyChangedHandler);
                }
            };

            base.OnInitialized(e);
        }

        void UpdateColumnInfo() {
            updatingColumnInfo = true;
            ColumnInfo = new ObservableCollection<ColumnInfo>(Columns.Select(x => new ColumnInfo(x)));
            updatingColumnInfo = false;
        }

        protected override void OnColumnReordered(DataGridColumnEventArgs e) {
            UpdateColumnInfo();
            base.OnColumnReordered(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e) {
            if (inWidthChange) {
                inWidthChange = false;
                UpdateColumnInfo();
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }

        void ColumnInfoChanged() {
            Items.SortDescriptions.Clear();
            foreach (var column in ColumnInfo) {
                var realColumn = Columns.FirstOrDefault(x => column.Header.Equals(x.Header));
                if (realColumn == null)
                    continue;
                column.Apply(realColumn, Columns.Count, Items.SortDescriptions);
            }
        }
    }

    public struct ColumnInfo
    {
        public int DisplayIndex;
        public object Header;
        public string PropertyPath;
        public ListSortDirection? SortDirection;
        public DataGridLengthUnitType WidthType;
        public double WidthValue;

        public ColumnInfo(DataGridColumn column) {
            Header = column.Header;
            PropertyPath = ((Binding) ((DataGridBoundColumn) column).Binding).Path.Path;
            WidthValue = column.Width.DisplayValue;
            WidthType = column.Width.UnitType;
            SortDirection = column.SortDirection;
            DisplayIndex = column.DisplayIndex;
        }

        public void Apply(DataGridColumn column, int gridColumnCount, SortDescriptionCollection sortDescriptions) {
            column.Width = new DataGridLength(WidthValue, WidthType);
            column.SortDirection = SortDirection;
            if (SortDirection != null)
                sortDescriptions.Add(new SortDescription(PropertyPath, SortDirection.Value));
            if (column.DisplayIndex != DisplayIndex) {
                var maxIndex = (gridColumnCount == 0) ? 0 : gridColumnCount - 1;
                column.DisplayIndex = (DisplayIndex <= maxIndex) ? DisplayIndex : maxIndex;
            }
        }
    }
}