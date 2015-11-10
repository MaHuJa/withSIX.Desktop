// <copyright company="SIX Networks GmbH" file="NotesView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Overlays
{
    [DoNotObfuscate]
    public partial class NotesView : UserControl, IDisposable
    {
        public NotesView() {
            InitializeComponent();

            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
            Loaded += OnLoaded;
        }

        protected NotesViewModel ViewModel
        {
            get { return (NotesViewModel) DataContext; }
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        void OnLoaded(object sender, RoutedEventArgs args) {
            FocusNotesEntry();
        }

        void FocusNotesEntry() {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new ThreadStart(() => NotesEntry.Focus()));
        }

        void OnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl)) {
                NotesEntry.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                ((NotesViewModel) DataContext).TryClose();
            }
        }

        void OnKeyUp(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Escape)
                NotesEntry.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                KeyUp -= OnKeyUp;
                KeyDown -= OnKeyDown;
                Loaded -= OnLoaded;
            }
            // free native resources
        }
    }
}