// <copyright company="SIX Networks GmbH" file="UserControlExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ReactiveUI;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    public static class UserControlExtensions
    {
        public static IDisposable SetupPopup(this UserControl control, Popup popup) {
            var listBoxItem = control.FindItem<ListBoxItem>();
            return Tuple.Create(control, listBoxItem)
                .WhenAnyValue(x => x.Item1.IsMouseOver, x => x.Item2.IsSelected)
                .Throttle(TimeSpan.FromMilliseconds(333), RxApp.MainThreadScheduler)
                .Select(x => x.Item2 && (x.Item1 || popup.IsMouseOver))
                .BindTo(popup, p => p.IsOpen);
        }
    }
}