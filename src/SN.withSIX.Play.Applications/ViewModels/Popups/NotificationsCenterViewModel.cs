// <copyright company="SIX Networks GmbH" file="NotificationsCenterViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Helpers;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels;
using SN.withSIX.Play.Applications.DataModels.Notifications;
using SN.withSIX.Play.Applications.Extensions;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Popups
{
    [DoNotObfuscate]
    public class NotificationsCenterViewModel : PopupBase
    {
        readonly IMediator _mediator;
        readonly object _notificationsLock = new object();
        readonly Lazy<IPlayShellViewModel> _shellVm;
        bool _isOpen;

        public NotificationsCenterViewModel(Lazy<IPlayShellViewModel> shellVm, IMediator mediator,
            INotificationCenterMessageHandler handler) {
            _shellVm = shellVm;
            _mediator = mediator;
            this.SetCommand(x => x.Open);
            Open.Subscribe(x => IsOpen = !IsOpen);
            this.SetCommand(x => x.ClearNotifications);
            ReactiveUI.ReactiveCommand.CreateAsyncTask(x => {
                var notification = (NotificationBaseDataModel) x;
                return ExecuteNotificationCommand(notification);
            })
                .SetNewCommand(this, x => x.ExecuteCommand)
                .Subscribe(x => HandleSubscription((dynamic) x));

            ClearNotifications.Subscribe(x => {
                var notifications = Notifications.Where(y => y.OneTimeAction).ToReadOnlyClonedCollection();
                Notifications.RemoveAll(notifications);
            });
            Notifications = new ReactiveList<NotificationBaseDataModel>();
            Notifications.EnableCollectionSynchronization(_notificationsLock);

            handler.NotificationCenter.Subscribe(x => HandleNotification((dynamic) x));
        }

        protected NotificationsCenterViewModel() {}
        public ReactiveCommand ClearNotifications { get; private set; }
        public ReactiveCommand Open { get; private set; }
        public ReactiveCommand<NotificationBaseDataModel> ExecuteCommand { get; set; }
        public ReactiveList<NotificationBaseDataModel> Notifications { get; }
        void HandleSubscription(NotificationBaseDataModel x) {}

        void HandleSubscription(NewSoftwareUpdateAvailableNotificationDataModel x) {
            _shellVm.Value.ShowOverlay(_shellVm.Value.Factory.CreateSoftwareUpdate().Value);
        }

        void HandleSubscription(NewSoftwareUpdateDownloadedNotificationDataModel x) {
            _shellVm.Value.ShowOverlay(_shellVm.Value.Factory.CreateSoftwareUpdate().Value);
        }

        void HandleNotification(NewSoftwareUpdateAvailableNotificationDataModel x) {
            HandleDedup(x);
        }

        void HandleNotification(NewSoftwareUpdateDownloadedNotificationDataModel x) {
            HandleDedup(x);
        }

        void HandleDedup<T>(T x) where T : DefaultNotificationDataModel {
            lock (Notifications) {
                var dm = Notifications.OfType<T>().LastOrDefault();
                if (dm == null) {
                    AddNotification(x);
                    return;
                }
                if (dm.Message == x.Message)
                    return;
                RemoveNotification(dm);
                AddNotification(x);
            }
        }

        void HandleNotification(NotificationBaseDataModel x) {
            AddNotification(x);
        }

        void AddNotification(NotificationBaseDataModel x) {
            lock (Notifications)
                Notifications.Add(x);
        }

        void RemoveNotification(NotificationBaseDataModel x) {
            lock (Notifications)
                Notifications.Remove(x);
        }

        async Task<NotificationBaseDataModel> ExecuteNotificationCommand(NotificationBaseDataModel notification) {
            if (notification.OneTimeAction)
                Notifications.Remove(notification);

            var dispatchCommand = notification.CloseCommand;
            if (dispatchCommand != null)
                await dispatchCommand.Dispatch(_mediator).ConfigureAwait(false);

            return notification;
        }
    }

    public class DesignTimeNotificationsCenterViewModel : NotificationsCenterViewModel, IDesignTimeViewModel
    {
        public DesignTimeNotificationsCenterViewModel() {
            Notifications = new ReactiveList<NotificationBaseDataModel>();
            Header = "Notifications";
            IsOpen = true;
        }

        public new bool IsOpen { get; set; }
        public new ReactiveList<NotificationBaseDataModel> Notifications { get; private set; }
        public string Header { get; private set; }
    }
}