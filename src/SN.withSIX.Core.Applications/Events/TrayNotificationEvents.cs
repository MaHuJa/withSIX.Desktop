// <copyright company="SIX Networks GmbH" file="TrayNotificationEvents.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows.Input;

namespace SN.withSIX.Core.Applications.Events
{
    public interface ITrayNotification
    {
        string Message { get; }
        string Title { get; }
    }

    public class TrayNotification : EventArgs, ITrayNotification
    {
        public TrayNotification(string title, string message) {
            Title = title;

            Message = message;
        }

        public string Message { get; }
        public string Title { get; }
    }

    public class TwoChoiceTrayNotification : EventArgs, ITrayNotification
    {
        public readonly ICommand NoCommand;
        public readonly ICommand YesCommand;

        public TwoChoiceTrayNotification(string title, string message, ICommand yesCommand, ICommand noCommand) {
            Title = title;
            Message = message;
            YesCommand = yesCommand;
            NoCommand = noCommand;
        }

        public string Message { get; }
        public string Title { get; }
    }

    public class ThreeChoiceTrayNotification : EventArgs, ITrayNotification
    {
        public readonly ICommand AcceptCommand;
        public readonly ICommand DeclineCommand;
        public readonly ICommand IgnoreCommand;

        public ThreeChoiceTrayNotification(string title, string message, ICommand acceptCommand,
            ICommand declineCommand,
            ICommand ignoreCommand) {
            Title = title;
            Message = message;
            AcceptCommand = acceptCommand;
            DeclineCommand = declineCommand;
            IgnoreCommand = ignoreCommand;
        }

        public string Message { get; }
        public string Title { get; }
    }
}