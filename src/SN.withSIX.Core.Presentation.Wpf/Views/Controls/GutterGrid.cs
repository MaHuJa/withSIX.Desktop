// <copyright company="SIX Networks GmbH" file="GutterGrid.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class GutterGrid : Grid
    {
        public static readonly DependencyProperty NumberOfRowsProperty;
        public static readonly DependencyProperty NumberOfColumnsProperty;
        public static readonly DependencyProperty ColumnGutterWidthProperty;
        public static readonly DependencyProperty RowGutterWidthProperty;
        public new static readonly DependencyProperty RowProperty;
        public new static readonly DependencyProperty ColumnProperty;
        public new static readonly DependencyProperty RowSpanProperty;
        public new static readonly DependencyProperty ColumnSpanProperty;
        protected static readonly GridLength DefaultGutter;

        static GutterGrid() {
            NumberOfRowsProperty = DependencyProperty.Register("NumberOfRows", typeof (int), typeof (GutterGrid),
                new PropertyMetadata(1, GridLayoutChanged));
            NumberOfColumnsProperty = DependencyProperty.Register("NumberOfColumns", typeof (int), typeof (GutterGrid),
                new PropertyMetadata(1, GridLayoutChanged));
            ColumnGutterWidthProperty = DependencyProperty.Register("ColumnGutterWidth", typeof (GridLength),
                typeof (GutterGrid), new PropertyMetadata(DefaultGutter, GridLayoutChanged));
            RowGutterWidthProperty = DependencyProperty.Register("RowGutterWidth", typeof (GridLength),
                typeof (GutterGrid), new PropertyMetadata(DefaultGutter, GridLayoutChanged));
            RowProperty = DependencyProperty.RegisterAttached("Row", typeof (int), typeof (GutterGrid),
                new PropertyMetadata(ChildElementRowChanged));
            ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof (int), typeof (GutterGrid),
                new PropertyMetadata(ChildElementColumnChanged));
            RowSpanProperty = DependencyProperty.RegisterAttached("RowSpan", typeof (int), typeof (GutterGrid),
                new PropertyMetadata(1, ChildElementRowSpanChanged));
            ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan", typeof (int), typeof (GutterGrid),
                new PropertyMetadata(1, ChildElementColumnSpanChanged));

            DefaultGutter = new GridLength(10, GridUnitType.Pixel);
        }

        public int NumberOfRows
        {
            get { return (int) GetValue(NumberOfRowsProperty); }
            set { SetValue(NumberOfRowsProperty, value); }
        }
        public int NumberOfColumns
        {
            get { return (int) GetValue(NumberOfColumnsProperty); }
            set { SetValue(NumberOfRowsProperty, value); }
        }
        public GridLength ColumnGutterWidth
        {
            get { return (GridLength) GetValue(ColumnGutterWidthProperty); }
            set { SetValue(ColumnGutterWidthProperty, value); }
        }
        public GridLength RowGutterWidth
        {
            get { return (GridLength) GetValue(RowGutterWidthProperty); }
            set { SetValue(RowGutterWidthProperty, value); }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved) {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            ChildElementRowChanged(visualAdded,
                new DependencyPropertyChangedEventArgs(RowProperty, 0, GetValue(RowProperty)));
            ChildElementColumnChanged(visualAdded,
                new DependencyPropertyChangedEventArgs(ColumnProperty, 0, GetValue(ColumnProperty)));
        }

        static void ChildElementColumnSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var uiElement = d as UIElement;
            if (uiElement == null)
                return;

            var columnSpanWithoutGutters = (int) e.NewValue;
            var columnSpanWithGutters = columnSpanWithoutGutters*2 - 1;

            Grid.SetColumnSpan(uiElement, columnSpanWithGutters);
        }

        static void ChildElementRowSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var uiElement = d as UIElement;
            if (uiElement == null)
                return;

            var rowSpanWithoutGutters = (int) e.NewValue;
            var rowSpanWithGutters = rowSpanWithoutGutters*2 - 1;

            Grid.SetRowSpan(uiElement, rowSpanWithGutters);
        }

        static void ChildElementRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var uiElement = d as UIElement;
            if (uiElement == null)
                return;

            var rowWithoutGutters = (int) e.NewValue;
            var rowWithGutters = rowWithoutGutters*2 + 1;

            Grid.SetRow(uiElement, rowWithGutters);
        }

        static void ChildElementColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var uiElement = d as UIElement;
            if (uiElement == null)
                return;

            var columnWithoutGutters = (int) e.NewValue;
            var columnWithGutters = columnWithoutGutters*2 + 1;

            Grid.SetColumn(uiElement, columnWithGutters);
        }

        static void GridLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var grid = d as GutterGrid;
            if (grid == null)
                return;

            var oneStarGridLength = new GridLength(1, GridUnitType.Star);

            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = grid.ColumnGutterWidth});
            for (var i = 0; i < grid.NumberOfColumns; i++) {
                grid.ColumnDefinitions.Add(new ColumnDefinition {Width = oneStarGridLength});
                grid.ColumnDefinitions.Add(new ColumnDefinition {Width = grid.ColumnGutterWidth});
            }

            grid.RowDefinitions.Clear();
            grid.RowDefinitions.Add(new RowDefinition {Height = grid.RowGutterWidth});
            for (var i = 0; i < grid.NumberOfRows; i++) {
                grid.RowDefinitions.Add(new RowDefinition {Height = oneStarGridLength});
                grid.RowDefinitions.Add(new RowDefinition {Height = grid.RowGutterWidth});
            }
        }

        public new static void SetRow(UIElement element, int value) {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(Grid.RowProperty, value);
        }

        public new static int GetRow(UIElement element) {
            if (element == null)
                throw new ArgumentNullException("element");

            return (int) element.GetValue(Grid.RowProperty);
        }

        public new static void SetColumn(UIElement element, int value) {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(Grid.ColumnProperty, value);
        }

        public new static int GetColumn(UIElement element) {
            if (element == null)
                throw new ArgumentNullException("element");

            return (int) element.GetValue(Grid.ColumnProperty);
        }

        public new static void SetRowSpan(UIElement element, int value) {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(Grid.RowSpanProperty, value);
        }

        public new static int GetRowSpan(UIElement element) {
            if (element == null)
                throw new ArgumentNullException("element");

            return (int) element.GetValue(Grid.RowSpanProperty);
        }

        public new static void SetColumnSpan(UIElement element, int value) {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(Grid.ColumnSpanProperty, value);
        }

        public new static int GetColumnSpan(UIElement element) {
            if (element == null)
                throw new ArgumentNullException("element");

            return (int) element.GetValue(Grid.ColumnSpanProperty);
        }
    }
}