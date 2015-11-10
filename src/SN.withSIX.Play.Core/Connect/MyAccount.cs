// <copyright company="SIX Networks GmbH" file="MyAccount.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Connect
{
    public class MyAccountModel
    {
        public Account Account { get; set; }
        public IList<Friend> Friends { get; set; }
        public IList<Group> Groups { get; set; }
        public IList<InviteRequest> InviteRequests { get; set; }
        public int UnreadPrivateMessages { get; set; }
    }

    public class MyAccount : PropertyChangedBase
    {
        Account _account;
        ReactiveList<Friend> _friends;
        ReactiveList<Group> _groups;
        ReactiveList<InviteRequest> _inviteRequests;
        int _unreadPrivateMessages;

        public MyAccount() {
            Friends = new ReactiveList<Friend> {ChangeTrackingEnabled = true};
            Groups = new ReactiveList<Group>();
            InviteRequests = new ReactiveList<InviteRequest>();
            PublicChats = new ReactiveList<PublicChat>();
            GroupChats = new ReactiveList<GroupChat>();
            PrivateChats = new ReactiveList<PrivateChat>();
            OnlineFriends = new ReactiveList<Friend>();
            Gaming = new ReactiveList<Friend>();

            Friends.TrackChanges(OnFriendAdd, remove => OnlineFriends.Remove(remove),
                reset =>
                    reset.Where(x => x.Status != OnlineStatus.Offline).SyncCollectionLocked(OnlineFriends));
            Friends.ItemChanged
                .Where(x => x.PropertyName == String.Empty || x.PropertyName == "Status")
                .Select(x => x.Sender)
                .Subscribe(x => {
                    lock (OnlineFriends) {
                        if (x.Status != OnlineStatus.Offline)
                            OnlineFriends.AddWhenMissing(x);
                        else {
                            if (OnlineFriends.Contains(x))
                                OnlineFriends.Remove(x);
                        }
                    }
                });

            Friends.ItemChanged
                .Where(x => x.PropertyName == String.Empty || x.PropertyName == "PlayingOn")
                .Select(x => x.Sender)
                .Subscribe(x => {
                    lock (Gaming) {
                        if (x.PlayingOn != null)
                            Gaming.AddWhenMissing(x);
                        else {
                            if (Gaming.Contains(x))
                                Gaming.Remove(x);
                        }
                    }
                });
        }

        public ReactiveList<PrivateChat> PrivateChats { get; }
        public ReactiveList<GroupChat> GroupChats { get; }
        public ReactiveList<Friend> Gaming { get; }
        public ReactiveList<Friend> OnlineFriends { get; }
        public Account Account
        {
            get { return _account; }
            set { SetProperty(ref _account, value); }
        }
        public ReactiveList<Friend> Friends
        {
            get { return _friends; }
            set { SetProperty(ref _friends, value); }
        }
        public ReactiveList<Group> Groups
        {
            get { return _groups; }
            set { SetProperty(ref _groups, value); }
        }
        public ReactiveList<InviteRequest> InviteRequests
        {
            get { return _inviteRequests; }
            set { SetProperty(ref _inviteRequests, value); }
        }
        public int UnreadPrivateMessages
        {
            get { return _unreadPrivateMessages; }
            set { SetProperty(ref _unreadPrivateMessages, value); }
        }
        public ReactiveList<PublicChat> PublicChats { get; }

        void OnFriendAdd(Friend addFriend) {
            if (addFriend.Status == OnlineStatus.Offline)
                return;
            lock (OnlineFriends)
                OnlineFriends.Add(addFriend);
        }

        public void Clear() {
            Friends.Clear();
            Groups.Clear();
            InviteRequests.Clear();
            PublicChats.Clear();
            PrivateChats.Clear();
            GroupChats.Clear();
        }
    }
}