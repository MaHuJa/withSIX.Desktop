// <copyright company="SIX Networks GmbH" file="ChatControl.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using SN.withSIX.Core.Presentation.Wpf.Behaviors;
using SN.withSIX.Core.Presentation.Wpf.Extensions;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl
    {
        public ChatControl() {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            var sv = ChatMessages.GetDescendantByType<ScrollViewer>();
            ScrollViewerEx.SetAutoScrollToEnd(sv, true);
        }
    }
}