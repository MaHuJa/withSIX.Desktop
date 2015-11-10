// <copyright company="SIX Networks GmbH" file="WpfNotificationProvider.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Presentation.Wpf.Services
{
    public class WpfNotificationProvider : INotificationProvider
    {
        public Task<bool?> Notify(string subject, string text, string icon = null, TimeSpan? expirationTime = null) {
            // TODO: How else to communicate with the MainWindow that hosts the TaskbarIcon??
            Cheat.MessageBus.SendMessage(new ShowTrayNotification(subject, text, icon, expirationTime));
            return Task.FromResult((bool?) false);
        }
    }
}