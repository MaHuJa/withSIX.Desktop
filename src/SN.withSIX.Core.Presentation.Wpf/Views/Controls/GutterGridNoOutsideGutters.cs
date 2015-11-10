// <copyright company="SIX Networks GmbH" file="GutterGridNoOutsideGutters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class GutterGridNoOutsideGutters : GutterGrid
    {
        public new static readonly DependencyProperty NumberOfRowsProperty;
        public new static readonly DependencyProperty NumberOfColumnsProperty;
        public new static readonly DependencyProperty ColumnGutterWidthProperty;
        public new static readonly DependencyProperty RowGutterWidthProperty;
        public new static readonly DependencyProperty RowProperty;
        public new static readonly DependencyProperty ColumnProperty;
        public new static readonly DependencyProperty RowSpanProperty;
        public new static readonly DependencyProperty ColumnSpanProperty;

        static GutterGridNoOutsideGutters() {
            NumberOfRowsProperty = DependencyProperty.Register("NumberOfRows", typeof (int),
                typeof (GutterGridNoOutsideGutters),
                new PropertyMetadata(1, GridLayoutChanged));
            NumberOfColumnsProperty = DependencyProperty.Register("NumberOfColumns", typeof (int),
                typeof (GutterGridNoOutsideGutters),
                new PropertyMetadata(1, GridLayoutChanged));
            ColumnGutterWidthProperty = DependencyProperty.Register("ColumnGutterWidth", typeof (GridLength),
                typeof (GutterGridNoOutsideGutters), new PropertyMetadata(DefaultGutter, GridLayoutChanged));
            RowGutterWidthProperty = DependencyProperty.Register("RowGutterWidth", typeof (GridLength),
                typeof (GutterGridNoOutsideGutters), new PropertyMetadata(DefaultGutter, GridLayoutChanged));
            RowProperty = DependencyProperty.RegisterAttached("Row", typeof (int), typeof (GutterGridNoOutsideGutters),
                new PropertyMetadata(ChildElementRowChanged));
            ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof (int),
                typeof (GutterGridNoOutsideGutters),
                new PropertyMetadata(ChildElementColumnChanged));
            RowSpanProperty = DependencyProperty.RegisterAttached("RowSpan", typeof (int),
                typeof (GutterGridNoOutsideGutters),
                new PropertyMetadata(1, ChildElementRowSpanChanged));
            ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan", typeof (int),
                typeof (GutterGridNoOutsideGutters),
                new PropertyMetadata(1, ChildElementColumnSpanChanged));
        }

        public new int NumberOfRows
        {
            get { return (int) GetValue(NumberOfRowsProperty); }
            set { SetValue(NumberOfRowsProperty, value); }
        }
        public new int NumberOfColumns
        {
            get { return (int) GetValue(NumberOfColumnsProperty); }
            set { SetValue(NumberOfRowsProperty, value); }
        }
        public new GridLength ColumnGutterWidth
        {
            get { return (GridLength) GetValue(ColumnGutterWidthProperty); }
            set { SetValue(ColumnGutterWidthProperty, value); }
        }
        public new GridLength RowGutterWidth
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
            var grid = d as GutterGridNoOutsideGutters;
            if (grid == null)
                return;

            var oneStarGridLength = new GridLength(1, GridUnitType.Star);

            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(0)});
            for (var i = 0; i < grid.NumberOfColumns; i++) {
                grid.ColumnDefinitions.Add(new ColumnDefinition {Width = oneStarGridLength});
                grid.ColumnDefinitions.Add(new ColumnDefinition {
                    Width = i + 1 < grid.NumberOfColumns ? grid.ColumnGutterWidth : new GridLength(0)
                });
            }

            grid.RowDefinitions.Clear();
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(0)});
            for (var i = 0; i < grid.NumberOfRows; i++) {
                grid.RowDefinitions.Add(new RowDefinition {Height = oneStarGridLength});
                grid.RowDefinitions.Add(new RowDefinition {
                    Height = i + 1 < grid.NumberOfRows ? grid.RowGutterWidth : new GridLength(0)
                });
            }
        }
    }
}