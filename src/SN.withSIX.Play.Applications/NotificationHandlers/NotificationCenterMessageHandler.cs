// <copyright company="SIX Networks GmbH" file="NotificationCenterMessageHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Events;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.DataModels;
using SN.withSIX.Play.Applications.DataModels.Notifications;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.ViewModels;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    [StayPublic]
    public class NotificationCenterMessageHandler :
        INotificationHandler<GroupChatMessageReceived>,
        INotificationHandler<PrivateMessageReceived>,
        INotificationHandler<FriendServerAddressChanged>,
        INotificationHandler<InviteRequestReceived>, INotificationHandler<FriendAdded>,
        INotificationHandler<FriendRemoved>,
        INotificationHandler<MinimizedEvent>,
        INotificationHandler<FriendOnline>, INotificationHandler<GroupImageUploadFailedEvent>,
        INotificationHandler<NewVersionAvailable>, INotificationHandler<NewVersionDownloaded>,
        INotificationHandler<CollectionCreatedEvent>, INotificationHandler<QueuedServerReadyEvent>,
        IDisposable, INotificationCenterMessageHandler
    {
        readonly ContactList _contactList;
        readonly Lazy<IContentManager> _contentManager;
        readonly Func<Task> _emptyTask = () => TaskExt.Default;
        readonly ISubject<NotificationBaseDataModel, NotificationBaseDataModel> _notificationCenterObservable;
        readonly UserSettings _settings;
        readonly CompositeDisposable _subjects = new CompositeDisposable();
        readonly ISubject<ITrayNotification, ITrayNotification> _trayNotificationObservable;

        public NotificationCenterMessageHandler(UserSettings settings, Lazy<IContentManager> contentManager,
            ContactList contactList) {
            _settings = settings;
            _contentManager = contentManager;
            _contactList = contactList;

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

        public void Handle(FriendAdded notification) {
            NotifyCenter(new DefaultNotificationDataModel(notification.Friend.DisplayName, "Added as friend"));
        }

        public void Handle(FriendOnline notification) {
            if (_settings.AppOptions.FriendOnlineNotify) {
                NotifyTray(new TrayNotification("Friend came online",
                    String.Format("{0} has just come online.", notification.Friend.Account.DisplayName)));
            }
        }

        public void Handle(FriendRemoved notification) {
            NotifyCenter(new DefaultNotificationDataModel(notification.Friend.DisplayName, "Removed as friend"));
        }

        public void Handle(FriendServerAddressChanged notification) {
            var server = notification.Address == null
                ? null
                : _contentManager.Value.ServerList.FindOrCreateServer(notification.Address);
            NotifyCenter(new FriendServerChangedDataModel(notification.Source.DisplayName,
                server == null ? null : server.Name));

            if (!_settings.AppOptions.FriendJoinNotify)
                return;

            var friend = notification.Source;
            if (server == null)
                return;

            if (String.IsNullOrWhiteSpace(server.Name))
                return;

            NotifyTray(new TwoChoiceTrayNotification("Friend joined server",
                String.Format("{0} has joined server: {1}\n\nDo you want to join him?", friend.DisplayName,
                    server.Name),
                new Func<Task>(() => _contactList.JoinServer(notification.Address)).ToAsyncCommand("JoinServer"),
                _emptyTask.ToAsyncCommand("DontJoinServer?")));
        }

        public void Handle(GroupChatMessageReceived notification) {
            if (ShouldSuppressNotification(notification))
                return;

            NotifyCenter(new ChatReceivedDataModel(notification.ChatMessage.Author.DisplayName,
                notification.ChatMessage.Author.Avatar,
                "In: " + notification.GroupChat.Group.Name + "\n" + notification.ChatMessage.Body));

            if (!_settings.AppOptions.ChatMessageNotify)
                return;

            NotifyTray(new TrayNotification(
                "New Chat message received", String.Format("in: {0}, from: {1}\n{2}", notification.Chat.Title,
                    notification.ChatMessage.Author.DisplayName, notification.ChatMessage.Body)));
        }

        public void Handle(GroupImageUploadFailedEvent notification) {
            NotifyCenter(
                new DefaultNotificationDataModel("Group image upload failed for: " + notification.Group.DisplayName,
                    notification.UploadMessage));
        }

        // TODO: Not happy with adding the Accept/Decline/Ignore callbacks into the events.
        // Probably better that they become commands that can be attached and dispatched from this layer instead.
        public void Handle(InviteRequestReceived notification) {
            NotifyCenter(new DefaultNotificationDataModel(notification.Request.DisplayName, "Invite as friend received"));

            if (_settings.AppOptions.FriendRequestNotify) {
                NotifyTray(new ThreeChoiceTrayNotification("New friend request received",
                    String.Format("{0} wants to add you to his friend list?", notification.Request.Account.DisplayName),
                    new Func<Task>(() => _contactList.ApproveInvite(notification.Request)).ToAsyncCommand(
                        "FriendRequestAccept"),
                    new Func<Task>(() => _contactList.DeclineInvite(notification.Request)).ToAsyncCommand(
                        "FriendRequestDecline"),
                    _emptyTask.ToAsyncCommand("FriendRequestIgnore")));
            }
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

        public void Handle(PrivateMessageReceived notification) {
            if (ShouldSuppressNotification(notification))
                return;
            NotifyTray(new TrayNotification("New Private message received", String.Format("from: {0}",
                notification.ChatMessage.Author.DisplayName)));

            NotifyCenter(new DefaultNotificationDataModel(notification.ChatMessage.Author.DisplayName,
                "Private message received"));
        }

        public void Handle(QueuedServerReadyEvent notification) {
            if (_settings.AppOptions.QueueStatusNotify) {
                NotifyTray(new TrayNotification("Server ready",
                    String.Format("Wait time in queue over, now joining: {0}", notification.Queued.Name)));
            }
        }

        bool ShouldSuppressNotification(ChatMessageReceived notification) {
            return notification.ChatMessage.IsMyMessage ||
                   (notification.Chat == _contactList.ActiveChat && _contactList.IsChatEnabled);
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