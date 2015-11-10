// <copyright company="SIX Networks GmbH" file="NotificationCenterMessageHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Events;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels;
using SN.withSIX.Play.Applications.DataModels.Notifications;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.ViewModels;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    [StayPublic]
    public class NotificationCenterMessageHandler :
        INotificationHandler<MinimizedEvent>,
        INotificationHandler<NewVersionAvailable>, INotificationHandler<NewVersionDownloaded>,
        INotificationHandler<CollectionCreatedEvent>, INotificationHandler<QueuedServerReadyEvent>,
        IDisposable, INotificationCenterMessageHandler
    {
        readonly ISubject<NotificationBaseDataModel, NotificationBaseDataModel> _notificationCenterObservable;
        readonly UserSettings _settings;
        readonly CompositeDisposable _subjects = new CompositeDisposable();
        readonly ISubject<ITrayNotification, ITrayNotification> _trayNotificationObservable;

        public NotificationCenterMessageHandler(UserSettings settings) {
            _settings = settings;

            var subject1 = new Subject<ITrayNotification>();
            _subjects.Add(subject1);
            _trayNotificationObservable = Subject.Synchronize(subject1);
            TrayNotification = _trayNotificationObservable.AsObservable();

            var subject2 = new Subject<NotificationBaseDataModel>();
            _subjects.Add(subject2);
            _notificationCenterObservable = Subject.Synchronize(subject2);
            NotificationCenter = _notificationCenterObservable.AsObservable();
        }

        public void Dispose() {
            _subjects.Dispose();
        }

        public IObservable<NotificationBaseDataModel> NotificationCenter { get; }
        public IObservable<ITrayNotification> TrayNotification { get; }

        public void Handle(CollectionCreatedEvent notification) {
            NotifyTray(new TrayNotification("Collection created",
                "Add mods by clicking 'Add to ...' button on the mods or drag and drop mods into the collection."));
        }

        public void Handle(MinimizedEvent notification) {
            NotifyTray(new TrayNotification("'Play withSIX' minimized to tray",
                "This behavior can be changed in 'settings'. Force exit by holding CTRL when clicking 'close'."));
        }

        public void Handle(NewVersionAvailable notification) {
            NotifyCenter(new NewSoftwareUpdateAvailableNotificationDataModel("New software version available",
                notification.Version.ToString()));
        }

        public void Handle(NewVersionDownloaded notification) {
            NotifyCenter(
                new NewSoftwareUpdateDownloadedNotificationDataModel(
                    "New software version downloaded, ready to install",
                    notification.Version.ToString(),
                    new DispatchCommand<UpdateAvailableCommand>(new UpdateAvailableCommand())) {OneTimeAction = false});
        }

        public void Handle(QueuedServerReadyEvent notification) {
            if (_settings.AppOptions.QueueStatusNotify) {
                NotifyTray(new TrayNotification("Server ready",
                    String.Format("Wait time in queue over, now joining: {0}", notification.Queued.Name)));
            }
        }

        void NotifyTray(ITrayNotification trayNotification) {
            _trayNotificationObservable.OnNext(trayNotification);
        }

        void NotifyCenter(NotificationBaseDataModel notification) {
            _notificationCenterObservable.OnNext(notification);
        }
    }

    public class MinimizedEvent {}
}