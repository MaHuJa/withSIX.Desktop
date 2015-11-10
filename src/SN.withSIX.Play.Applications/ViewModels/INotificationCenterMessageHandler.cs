// <copyright company="SIX Networks GmbH" file="INotificationCenterMessageHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Applications.Events;
using SN.withSIX.Play.Applications.DataModels;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public interface IHandleTrayNotifications
    {
        IObservable<ITrayNotification> TrayNotification { get; }
    }

    public interface IHandleNotificationCenter
    {
        IObservable<NotificationBaseDataModel> NotificationCenter { get; }
    }

    public interface INotificationCenterMessageHandler : IHandleTrayNotifications, IHandleNotificationCenter {}
}