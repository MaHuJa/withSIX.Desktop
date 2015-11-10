// <copyright company="SIX Networks GmbH" file="TrayNotificationViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using ReactiveUI;
using ShortBus;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public interface ITrayNotificationViewModel
    {
        string Title { get; }
        string Text { get; }
        IReactiveCommand<object> Close { get; }
        TimeSpan? CloseIn { get; }
        ICollection<TrayAction> Actions { get; }
    }

    public class TrayNotificationViewModel : ITrayNotificationViewModel
    {
        public TrayNotificationViewModel(string title, string text, TimeSpan? closeIn = null,
            ICollection<TrayAction> actions = null) {
            Title = title;
            Text = text;
            CloseIn = closeIn;
            Actions = actions;
            if (actions != null) {
                foreach (var a in actions)
                    a.Command.Subscribe(x => Close.Execute(null));
            }
        }

        public string Title { get; }
        public string Text { get; }
        public IReactiveCommand<object> Close { get; } = ReactiveCommand.Create();
        public TimeSpan? CloseIn { get; }
        public ICollection<TrayAction> Actions { get; }
    }

    public class TrayAction
    {
        public string DisplayName { get; set; }
        public IReactiveCommand<UnitType> Command { get; set; }
    }
}