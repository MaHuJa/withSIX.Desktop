// <copyright company="SIX Networks GmbH" file="InPlaceEditor.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReactiveUI;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for InPlaceEditor.xaml
    /// </summary>
    public partial class InPlaceEditor : UserControl
    {
        public static readonly DependencyProperty TextBlockStyleProperty = DependencyProperty.Register(
            "TextBlockStyle", typeof (Style), typeof (InPlaceEditor), new PropertyMetadata(default(Style)));
        public static readonly DependencyProperty TextBoxStyleProperty = DependencyProperty.Register("TextBoxStyle",
            typeof (Style), typeof (InPlaceEditor), new PropertyMetadata(default(Style)));
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing",
            typeof (bool), typeof (InPlaceEditor),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
            typeof (InPlaceEditor),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable",
            typeof (bool), typeof (InPlaceEditor), new PropertyMetadata(true));
        readonly MouseButtonEventHandler _handler;

        public InPlaceEditor() {
            InitializeComponent();

            EditTextBox.KeyUp += EditTextBoxOnKeyUp;
            this.WhenAnyValue(x => x.IsEditing)
                .Skip(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(OnEditingChanged);
            EditTextBox.LostFocus += EditTextBox_OnLostFocus;
            EditTextBox.IsMouseCapturedChanged += CustomControlIsMouseCapturedChanged;
            _handler = HandleClickOutsideOfControl;
        }

        public Style TextBlockStyle
        {
            get { return (Style) GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }
        public Style TextBoxStyle
        {
            get { return (Style) GetValue(TextBoxStyleProperty); }
            set { SetValue(TextBoxStyleProperty, value); }
        }
        public bool IsEditing
        {
            get { return (bool) GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }
        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public bool IsEditable
        {
            get { return (bool) GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        void OnEditingChanged(bool editing) {
            if (editing)
                EnterEditingMode();
            else
                ExitEditingMode();
        }

        void ExitEditingMode() {
            RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, _handler);
            ReleaseMouseCapture();
            MouseDown -= EditTextBox.POnMouseDown;
            MouseUp -= EditTextBox.POnMouseUp;
            MouseEnter -= EditTextBox.POnMouseEnter;
            EditTextBox.MouseLeave -= OnMouseLeave;
            MouseMove -= OnMouseMove;
            Cursor = Cursors.Arrow;
        }

        void OnMouseMove(object sender, MouseEventArgs e) {
            var mousePosition = Mouse.GetPosition(this);
            if (!(mousePosition.Y < 0 || mousePosition.X < 0 || mousePosition.Y > ActualHeight ||
                  mousePosition.X > ActualWidth))
                Cursor = Cursors.IBeam;
            else
                Cursor = Cursors.Arrow;
        }

        void EditTextBoxOnKeyUp(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Enter
                || keyEventArgs.Key == Key.Escape)
                IsEditing = false;
        }

        void TextBlockMouseButtonDown(object sender, MouseButtonEventArgs e) {
            if (!IsEditable)
                return;
            IsEditing = true;
        }

        public void EnterEditingMode() {
            SelectAllText(EditTextBox);
            EditTextBox.Focus();
            CaptureMouse();
            AddHandler();
            MouseDown += EditTextBox.POnMouseDown;
            MouseUp += EditTextBox.POnMouseUp;
            MouseEnter += EditTextBox.POnMouseEnter;
            EditTextBox.MouseLeave += OnMouseLeave;
            MouseMove += OnMouseMove;
        }

        void OnMouseLeave(object sender, MouseEventArgs e) {
            OnMouseLeave(e);
            CaptureMouse();
        }

        void CustomControlIsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var mousePosition = Mouse.GetPosition(this);
            if (mousePosition.Y < 0 || mousePosition.X < 0 || mousePosition.Y > ActualHeight ||
                mousePosition.X > ActualWidth)
                IsEditing = IsEditing;
        }

        void AddHandler() {
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, _handler, true);
        }

        void HandleClickOutsideOfControl(object sender, MouseButtonEventArgs e) {
            var mousePosition = Mouse.GetPosition(this);
            if (mousePosition.Y < 0 || mousePosition.X < 0 || mousePosition.Y > ActualHeight ||
                mousePosition.X > ActualWidth)
                IsEditing = false;
        }

        static void SelectAllText(TextBox tb) {
            tb.CaretIndex = tb.Text.Length;
            tb.SelectionStart = 0;
            tb.SelectionLength = tb.Text.Length;
        }

        void EditTextBox_OnLostFocus(object sender, RoutedEventArgs e) {
            IsEditing = false;
        }
    }
}