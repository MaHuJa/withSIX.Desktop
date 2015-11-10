// <copyright company="SIX Networks GmbH" file="Chat.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Core.Connect
{
    public abstract class ChatBase<T> : ConnectModelBase, IChat where T : ChatMessage, IComparePK<T>
    {
        bool _loaded;
        ReactiveList<T> _messages;
        string _title;
        int _unreadCount;

        protected ChatBase(Guid id) : base(id) {
            _messages = new ReactiveList<T> {ChangeTrackingEnabled = true};
            _messages.TrackChanges(x => UnreadCount += 1, y => UnreadCount -= 1,
                collection => UnreadCount = collection.Count(), arg => arg.IsUnread);
            _messages.ItemChanged.Where(x => x.PropertyName == "IsUnread")
                .Subscribe(x => HandleCount(x.Sender.IsUnread));
        }

        public int UnreadCount
        {
            get { return _unreadCount; }
            set { SetProperty(ref _unreadCount, value); }
        }
        public ReactiveList<T> Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public async Task<ChatMessage> SendMessage(ChatInput cm, IConnectApiHandler handler) {
            var message = await SendMessageInternal(cm, handler).ConfigureAwait(false);
            ProcessSentMessage(message);
            return message;
        }

        public void MarkAllAsRead() {
            lock (Messages) {
                foreach (var m in Messages)
                    m.IsUnread = false;
            }
        }

        public abstract Task Refresh(IConnectApiHandler apiHandler);
        public bool Loaded
        {
            get { return _loaded; }
            set { SetProperty(ref _loaded, value); }
        }
        protected abstract Task<T> SendMessageInternal(ChatInput cm, IConnectApiHandler handler);

        void HandleCount(bool flag) {
            if (flag)
                UnreadCount += 1;
            else
                UnreadCount -= 1;
        }

        public void UpdateOrAddMessage(T message) {
            if (Loaded) {
                if (ReferenceEquals(message, Messages.UpdateOrAdd(message)))
                    PublishInternal(message);
            } else
                Loaded = true;
        }

        void PublishInternal(T message) {
            if (!message.IsMyMessage)
                message.IsUnread = true;
            Publish(message);
        }

        public void UpdateOrAddMessages(IEnumerable<T> messages) {
            var newMessages = Messages.UpdateOrAdd(messages, false, new[] {"IsUnread"});
            if (Loaded) {
                if (!newMessages.Any())
                    return;
                foreach (var x in newMessages)
                    PublishInternal(x);
            } else
                Loaded = true;
        }

        protected abstract void Publish(T message);

        void ProcessSentMessage(T message) {
            UpdateOrAddMessage(message);
            MarkAllAsRead();
        }
    }

    public abstract class Chat : ChatBase<ChatMessage>
    {
        ReactiveList<Account> _users;

        protected Chat(Guid id)
            : base(id) {
            _users = new ReactiveList<Account>();
        }

        public ReactiveList<Account> Users
        {
            get { return _users; }
            set { SetProperty(ref _users, value); }
        }
    }

    public interface IChat : IHaveGuidId
    {
        string Title { get; }
        bool Loaded { get; set; }
        Task<ChatMessage> SendMessage(ChatInput cm, IConnectApiHandler handler);
        void MarkAllAsRead();
        Task Refresh(IConnectApiHandler apiHandler);
    }

    public class GroupChat : Chat
    {
        public GroupChat(Guid id) : base(id) {}
        public Group Group { get; set; }

        public override async Task Refresh(IConnectApiHandler apiHandler) {
            UpdateOrAddMessages(await apiHandler.GetChatMessages(this).ConfigureAwait(false));
        }

        protected override Task<ChatMessage> SendMessageInternal(ChatInput cm, IConnectApiHandler handler) {
            return handler.SendGroupChatMessage(cm, this);
        }

        protected override void Publish(ChatMessage message) {
            Common.App.PublishDomainEvent(new GroupChatMessageReceived(this, message));
        }
    }

    public class PublicChat : Chat, IContact
    {
        public static Guid GlobalChatObjectId = new Guid("f983d60b-a4c0-4218-ae71-19691ff05974");

        public PublicChat(Guid id) : base(id) {
            Title = "Global Chat";
        }

        public Guid ObjectUuid
        {
            get { return GlobalChatObjectId; }
        }
        public string DisplayName
        {
            get { return Title; }
        }

        public Uri GetUri() {
            throw new NotImplementedException();
        }

        public Uri GetOnlineConversationUrl() {
            throw new NotImplementedException();
        }

        public override async Task Refresh(IConnectApiHandler apiHandler) {
            UpdateOrAddMessages(await apiHandler.GetChatMessages(this).ConfigureAwait(false));
        }

        protected override Task<ChatMessage> SendMessageInternal(ChatInput cm, IConnectApiHandler handler) {
            return handler.SendPublicChatMessage(cm, this);
        }

        protected override void Publish(ChatMessage message) {
            Common.App.PublishDomainEvent(new PublicChatMessageReceived(this, message));
        }
    }

    public class PrivateChat : ChatBase<PrivateMessage>
    {
        Account _user;
        public PrivateChat(Guid id) : base(id) {}
        public Friend Friend { get; set; }
        public Account User
        {
            get { return _user; }
            set
            {
                if (!SetProperty(ref _user, value))
                    return;
                Title = value == null ? null : value.DisplayName;
            }
        }

        public override async Task Refresh(IConnectApiHandler apiHandler) {
            UpdateOrAddMessages(await apiHandler.GetPrivateChatMessages(User).ConfigureAwait(false));
        }

        protected override Task<PrivateMessage> SendMessageInternal(ChatInput cm, IConnectApiHandler handler) {
            return handler.SendPrivateChatMessage(cm, this);
        }

        protected override void Publish(PrivateMessage message) {
            Common.App.PublishDomainEvent(new PrivateMessageReceived(this, message));
        }
    }
}