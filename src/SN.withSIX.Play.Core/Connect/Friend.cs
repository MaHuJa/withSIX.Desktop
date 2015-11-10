// <copyright company="SIX Networks GmbH" file="Friend.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Connect
{
    public class Friend : PropertyChangedBase, IContact
    {
        ServerAddress _playingOn;
        int _previousCount;
        Server _server;
        OnlineStatus _status;
        string _statusText;
        int _unreadPrivateMessages;

        public Friend(Account account) {
            Contract.Requires<ArgumentNullException>(account != null);
            Account = account;
        }

        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }
        public int PreviousCount
        {
            get { return _previousCount; }
            set { SetProperty(ref _previousCount, value); }
        }
        public Account Account { get; }
        public OnlineStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
        public ServerAddress PlayingOn
        {
            get { return _playingOn; }
            set { SetProperty(ref _playingOn, value); }
        }
        public int UnreadPrivateMessages
        {
            get { return _unreadPrivateMessages; }
            set { SetProperty(ref _unreadPrivateMessages, value); }
        }
        public Server Server
        {
            get { return _server; }
            set { SetProperty(ref _server, value); }
        }
        public string DisplayName
        {
            get { return Account.DisplayName; }
        }

        public Uri GetUri() {
            return Account.GetUri();
        }

        public Guid Id
        {
            get { return Account.Id; }
        }

        public bool ComparePK(object other) {
            var o1 = other as Friend;
            if (o1 != null)
                return ComparePK(o1.Account);
            var o2 = other as IHaveGuidId;
            return o2 != null && ComparePK(o2);
        }

        public bool ComparePK(IHaveGuidId other) {
            return other != null && other.Id.Equals(Account.Id);
        }

        public Uri GetOnlineConversationUrl() {
            return Account.GetOnlineConversationUrl();
        }
    }
}