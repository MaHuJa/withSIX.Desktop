// <copyright company="SIX Networks GmbH" file="SoftwareUpdateView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using CefSharp;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Presentation.Wpf.Views
{
    [DoNotObfuscate]
    public partial class SoftwareUpdateView : UserControl
    {
        bool _first = true;

        public SoftwareUpdateView() {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            if (_first) {
                _first = false;
                return;
            }

            if (!webControl.IsLoading)
                webControl.Reload(false);
        }
    }
}