// <copyright company="SIX Networks GmbH" file="ContactList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Options;
using PropertyChangedBase = SN.withSIX.Core.Helpers.PropertyChangedBase;
using ServerAddress = SN.withSIX.Play.Core.ServerAddress;

namespace SN.withSIX.Play.Applications.Services
{
    [DoNotObfuscate]
    public class ContactList : PropertyChangedBase, IHandle<GameLaunchedEvent>, IHandle<GameTerminated>,
        IHandle<ServersAdded>,
        IHandle<ApiKeyUpdated>,
        IHandle<RefreshLoginRequest>,
        IHandle<MyActiveServerAddressChanged>,
        IHandle<ConnectionStateChanged>,
        IEnableLogging, IDomainService
    {
        readonly IConnectApiHandler _apiHandler;
        readonly Lazy<IContentManager> _contentManager;
        readonly Object _disposableLock = new object();
        readonly IEventAggregator _eventBus;
        readonly Lazy<LaunchManager> _launchManager;
        readonly IMediator _mediator;
        readonly UserSettings _settings;
        IChat _activeChat;
        ServerAddress _activeServerAddress;
        bool _addFriendSearching;
        ConnectedState _connectedState = ConnectedState.Disconnected;
        CompositeDisposable _disposables;
        bool _initialConnect;
        bool _isChatEnabled;
        ServerAddress _lastSuccessfulServerAddress;
        LoginState _loginState;
        bool _notificationsSet;
        OnlineStatus _onlineStatus = OnlineStatus.Online;
        DateTime _synchronizedAt;

        public ContactList(IEventAggregator ea,
            IConnectApiHandler handler, Lazy<LaunchManager> launchManager, Lazy<IContentManager> contentManager,
            IMediator mediator,
            UserSettings settings) {
            _eventBus = ea;
            _launchManager = launchManager;
            _contentManager = contentManager;
            _mediator = mediator;
            _settings = settings;
            _apiHandler = handler;
            _apiHandler.MessageBus.Listen<ConnectionStateChanged>().Subscribe(Handle);

            AddFriends = new ReactiveList<AddFriend>();
            LoginState = string.IsNullOrWhiteSpace(_settings.AccountOptions.AccessToken)
                ? LoginState.LoggedOut
                : LoginState.LoggedIn;

            this.WhenAnyValue(x => x.LoginState)
                .Skip(1)
                .Subscribe(HandleNewLogginState);

            // TODO: Not good
            /*
            _apiHandler.MessageBus.Listen<ConnectionStateChanged>()
                .Subscribe(x => {
                    if (LoginState == LoginState.LoggedIn)
                        ConnectedState = x.IsConnected ? ConnectedState.Connected : ConnectedState.Connecting;
                });
             */
        }

        public ConnectedState ConnectedState
        {
            get { return _connectedState; }
            set
            {
                if (_connectedState == value)
                    return;
                _connectedState = value;
                OnPropertyChanged();
            }
        }
        public LoginState LoginState
        {
            get { return _loginState; }
            set
            {
                if (_loginState == value)
                    return;
                _loginState = value;
                OnPropertyChanged();
            }
        }
        public MyAccount UserInfo
        {
            get { return _apiHandler.Me; }
        }
        public OnlineStatus OnlineStatus
        {
            get { return _onlineStatus; }
            set { SetProperty(ref _onlineStatus, value); }
        }
        public ReactiveList<AddFriend> AddFriends { get; }
        public DateTime SynchronizedAt
        {
            get { return _synchronizedAt; }
            set { SetProperty(ref _synchronizedAt, value); }
        }
        public bool AddFriendSearching
        {
            get { return _addFriendSearching; }
            set { SetProperty(ref _addFriendSearching, value); }
        }
        public ServerAddress ActiveServerAddress
        {
            get { return _activeServerAddress; }
            set { SetProperty(ref _activeServerAddress, value); }
        }
        public IChat ActiveChat
        {
            get { return _activeChat; }
            set { SetProperty(ref _activeChat, value); }
        }
        public bool IsChatEnabled
        {
            get { return _isChatEnabled; }
            set { SetProperty(ref _isChatEnabled, value); }
        }

        public async void Handle(ApiKeyUpdated message) {
            LoginState = string.IsNullOrWhiteSpace(message.ApiKey) ? LoginState.LoggedOut : LoginState.LoggedIn;
            await RefreshConnection().ConfigureAwait(false);
        }

        public void Handle(ConnectionStateChanged message) {
            if (message.ConnectedState == ConnectedState.Connected && _initialConnect)
                return;
            ConnectedState = message.ConnectedState;
        }

        public void Handle(GameLaunchedEvent message) {
            _eventBus.PublishOnCurrentThread(
                new MyActiveServerAddressChanged(message.Server != null ? message.Server.Address : null));
        }

        public void Handle(GameTerminated message) {
            _eventBus.PublishOnCurrentThread(new MyActiveServerAddressChanged(null));
        }

        public async void Handle(MyActiveServerAddressChanged message) {
            ActiveServerAddress = message.Address;
            var lastSuccessfulServerAddress = _lastSuccessfulServerAddress;
            if (ConnectedState != ConnectedState.Connected ||
                (lastSuccessfulServerAddress == null && message.Address == null))
                return;

            if (lastSuccessfulServerAddress == null || !lastSuccessfulServerAddress.Equals(message.Address))
                await SetServerAddress(message.Address).ConfigureAwait(false);
        }

        public async void Handle(RefreshLoginRequest message) {
            await RefreshConnection().ConfigureAwait(false);
        }

        public void Handle(ServersAdded message) {
            var friends = UserInfo.Friends.ToArray();

            foreach (var server in message.Servers) {
                foreach (
                    var friend in
                        friends.Where(
                            x =>
                                x.PlayingOn != null && x.PlayingOn.Equals(server.Address))
                    ) {
                    var s = friend.Server;
                    if (s != null)
                        s.RemoveFriend(friend);
                    server.AddFriend(friend);
                }
            }
        }

        async Task RefreshConnection() {
            ConnectedState = ConnectedState.Disconnected;
            await HandleConnection().ConfigureAwait(false);
        }

        void HandleNewLogginState(LoginState loginState) {
            DomainEvilGlobal.Settings.RaiseChanged();
            if (loginState != LoginState.LoggedIn)
                ClearLists();
        }

        public Task JoinServer(ServerAddress addr) {
            return _launchManager.Value.JoinServer(addr);
        }

        public bool IsFriend(Guid guid) {
            Contract.Requires<ArgumentNullException>(guid != null);
            return FindFriend(guid) != null;
        }

        public bool HasInviteRequest(Guid guid) {
            return FindInviteRequest(guid) != null;
        }

        public bool VisitProfile(IContact entity) {
            Contract.Requires<ArgumentNullException>(entity != null);
            return VisitProfile(entity.GetUri());
        }

        public void VisitConversationOnline(IContact model) {
            Contract.Requires<ArgumentNullException>(model != null);
            BrowserHelper.TryOpenUrlIntegrated(model.GetOnlineConversationUrl());
        }

        public void InviteToServer(IContact model) {
            Contract.Requires<ArgumentNullException>(model != null);
            var friend = model as Friend;
            if (friend != null)
                FriendInviteToServer(friend);
            else {
                var group = model as Group;
                if (group != null)
                    GroupInviteToServer(group);
            }
        }

        public async Task UpdateAddFriends(string search) {
            AddFriends.Clear();
            if (String.IsNullOrWhiteSpace(search) || search.Length < 3)
                return;

            AddFriendSearching = true;

            await
                TryGetAddFriendUsers(search).ConfigureAwait(false);
        }

        public void AdvancedFriends() {
            BrowserHelper.TryOpenUrlIntegrated(Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "me", "friends"));
        }

        public void AdvancedGroups() {
            BrowserHelper.TryOpenUrlIntegrated(Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "groups"));
        }

        public Task ApproveInvite(InviteRequest request) {
            Contract.Requires<ArgumentNullException>(request != null);

            return TryApproveInvite(request);
        }

        public async Task DeclineInvite(InviteRequest request) {
            Contract.Requires<ArgumentNullException>(request != null);

            await _apiHandler.DeclineFriend(request);
            UserInfo.InviteRequests.Remove(request);
        }

        public async Task HandleConnection() {
            _initialConnect = true;
            try {
                await TryConnect().ConfigureAwait(false);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
            _initialConnect = false;
        }

        public async Task RemoveFriend(Friend friend) {
            Contract.Requires<ArgumentNullException>(friend != null);

            await TryRemoveFriend(friend).ConfigureAwait(false);
        }

        public Task LeaveGroup(Group group) {
            Contract.Requires<ArgumentNullException>(group != null);

            return _apiHandler.LeaveGroup(group);
        }

        [SmartAssembly.ReportUsage.ReportUsage]
        public void RetrieveApiKey() {
            //Tools.OpenUrl(API_KEY_URL);
            _eventBus.PublishOnCurrentThread(new RequestOpenLogin());
        }

        public Friend FindFriend(Guid uuid) {
            return UserInfo.Friends.FirstOrDefault(x => x.Account.Id == uuid);
        }

        public Friend FindFriend(Account account) {
            return FindFriend(account.Id);
        }

        public InviteRequest FindInviteRequest(Guid uuid) {
            return UserInfo.InviteRequests.FirstOrDefault(x => x.Account.Id == uuid || x.Target.Id == uuid);
        }

        public bool IsMe(Account user) {
            return IsMe(user.Id);
        }

        public async Task<InviteRequest> Befriend(Account user) {
            var request = await _apiHandler.AddFriendshipRequest(user).ConfigureAwait(false);
            UserInfo.InviteRequests.UpdateOrAdd(request);
            return request;
        }

        bool IsMe(Guid uuid) {
            var me = UserInfo;
            return me != null && uuid == me.Account.Id;
        }

        static bool VisitProfile(Uri uri) {
            return BrowserHelper.TryOpenUrlIntegrated(uri);
        }

        void GroupInviteToServer(Group group) {
            Contract.Requires<ArgumentNullException>(group != null);
            throw new NotImplementedException();
            UsageCounter.ReportUsage("Dialog - Invite group to server");
        }

        void FriendInviteToServer(Friend friend) {
            Contract.Requires<ArgumentNullException>(friend != null);
            throw new NotImplementedException();
            UsageCounter.ReportUsage("Dialog - Invite friend to server");
        }

        async Task TryGetAddFriendUsers(string search) {
            try {
                var users = await _apiHandler.SearchUsers(search).ConfigureAwait(false);
                AddFriends.Clear();
                AddFriends.AddRange(users.Select(ConstructAddFriend));
            } finally {
                AddFriendSearching = false;
            }
        }

        AddFriend ConstructAddFriend(Account account) {
            // TODO: Can use automapper for this too??
            var isFriend = FindFriend(account.Id) != null;
            var isInvite = FindInviteRequest(account.Id) != null;
            return new AddFriend(account) {IsContact = isFriend || isInvite, IsMutualFriend = isFriend};
        }

        async Task TryApproveInvite(InviteRequest request) {
            var friend = await _apiHandler.ApproveFriend(request).ConfigureAwait(false);
            UserInfo.InviteRequests.Remove(request);
            UserInfo.Friends.UpdateOrAdd(friend);
        }

        async Task SetServerAddress(ServerAddress address) {
            try {
                await TrySetServerAddress(address).ConfigureAwait(false);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
        }

        async Task TrySetServerAddress(ServerAddress address) {
            if (ConnectedState != ConnectedState.Connected || LoginState != LoginState.LoggedIn)
                return;
            await
                _apiHandler.SetServerAddress(address).ConfigureAwait(false);
            _lastSuccessfulServerAddress = address;
        }

        async Task TryConnect() {
            var apiKey = _settings.AccountOptions.AccessToken;
            ConnectedState = ConnectedState.Connecting;
            var isLoggedIn = !apiKey.IsBlankOrWhiteSpace();

            // TODO: Deal with Disconnect on logout, and Connect on relogin etc.
            try {
                await _apiHandler.Initialize(apiKey).ConfigureAwait(false);
                _settings.AccountOptions.AccountId = isLoggedIn ? _apiHandler.Me.Account.Id : Guid.Empty;
            } catch (UnauthorizedException ex) {
                this.Logger().FormattedWarnException(ex);
                ConnectedState = ConnectedState.ConnectingFailed;
                _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(CommonUrls.LoginUrl));
                return;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                ConnectedState = ConnectedState.ConnectingFailed;
                throw;
            }

            ConnectedState = ConnectedState.Connected;

            if (!isLoggedIn) {
                ClearLists();
                LoginState = LoginState.LoggedOut;
                return;
            }

            LoginState = LoginState.LoggedIn;
            SetupHandlers();
            SynchronizedAt = Tools.Generic.GetCurrentUtcDateTime;
            await SetServerAddress(ActiveServerAddress).ConfigureAwait(false);

            await _mediator.NotifyAsync(new LoggedInEvent()).ConfigureAwait(false);
        }

        void SetupHandlers() {
            if (_notificationsSet)
                return;

            lock (_disposableLock) {
                HandleDisposables();

                _disposables = new CompositeDisposable();
                // Not using collection initializer because otherwise we might end up with some subscriptions in limbo, when later subscription calls fail...
                _disposables.Add(
                    UserInfo.InviteRequests.ItemsAdded.Where(x => !x.IsMine)
                        .Subscribe(x => _mediator.Notify(new InviteRequestReceived(x))));
                _disposables.Add(
                    UserInfo.InviteRequests.ItemsRemoved.Where(x => !x.IsMine)
                        .Subscribe(x => _mediator.Notify(new InviteRequestRemoved(x))));
                _disposables.Add(UserInfo.Friends.ItemsAdded.Subscribe(x => _mediator.Notify(new FriendAdded(x))));
                _disposables.Add(UserInfo.Friends.ItemsRemoved.Subscribe(x => _mediator.Notify(new FriendRemoved(x))));
                _disposables.Add(UserInfo.OnlineFriends.ItemsAdded.Subscribe(x => _mediator.Notify(new FriendOnline(x))));
                _disposables.Add(UserInfo.Friends.ItemChanged
                    .Where(x => x.PropertyName == String.Empty || x.PropertyName == "UnreadPrivateMessages")
                    .Select(x => x.Sender)
                    .Subscribe(x => _mediator.Notify(new EntityUnreadMessagesChanged(x, x.UnreadPrivateMessages))));
                _disposables.Add(UserInfo.Friends.ItemsAdded
                    .Where(x => x.PlayingOn != null)
                    .Subscribe(x => _mediator.Notify(new FriendServerAddressChanged(x.PlayingOn, x))));
                _disposables.Add(UserInfo.Friends.ItemChanged
                    .Where(x => x.PropertyName == String.Empty || x.PropertyName == "PlayingOn")
                    .Subscribe(x => ServerAddressChanged(x.Sender, x.Sender.PlayingOn)));

                _notificationsSet = true;
            }
        }

        void HandleDisposables() {
            lock (_disposableLock) {
                var disposables = _disposables;
                if (disposables == null)
                    return;
                disposables.Dispose();
                _disposables = null;
                _notificationsSet = false;
            }
        }

        void ServerAddressChanged(Friend user, ServerAddress serverAddress) {
            var s = user.Server;
            if (s != null)
                s.RemoveFriend(user);

            if (serverAddress == null)
                return;

            var server = _contentManager.Value.ServerList.FindOrCreateServer(serverAddress);
            if (server == null)
                return;

            server.AddFriend(user);
            server.TryUpdateAsync().ConfigureAwait(false);
            _mediator.Notify(new FriendServerAddressChanged(serverAddress, user));
        }

        async Task TryRemoveFriend(Friend friend) {
            await _apiHandler.RemoveFriend(friend.Account).ConfigureAwait(false);
            UserInfo.Friends.Remove(friend);
        }

        public Task GetChatData(IChat chat) {
            Contract.Requires<ArgumentNullException>(chat != null);
            return chat.Refresh(_apiHandler);
        }

        public void LeaveChat(IChat chat) {
            Contract.Requires<ArgumentNullException>(chat != null);
            UserInfo.GroupChats.Remove(chat as GroupChat);
            UserInfo.PublicChats.Remove(chat as PublicChat);
            UserInfo.PrivateChats.Remove(chat as PrivateChat);
        }

        public async Task<GroupChat> GetOrRetrieveChat(Group @group) {
            return UserInfo.GroupChats
                .FirstOrDefault(x => x.Group == @group)
                   ?? await GetChat(@group).ConfigureAwait(false);
        }

        public async Task<PrivateChat> GetOrRetrieveChat(Friend friend) {
            return UserInfo.PrivateChats
                .FirstOrDefault(x => x.User == friend.Account)
                   ?? await GetChat(friend).ConfigureAwait(false);
        }

        public Task SendMessage(IChat model, ChatInput cm) {
            return model.SendMessage(cm, _apiHandler);
        }

        public Task MarkAsRead(Account account) {
            return _apiHandler.MarkAsRead(account);
        }

        async Task<GroupChat> GetChat(Group @group) {
            var chat = await _apiHandler.GetGroupChat(group).ConfigureAwait(false);
            if (chat == null)
                return null;
            UserInfo.GroupChats.AddWhenMissing(chat);
            return chat;
        }

        async Task<PrivateChat> GetChat(Friend friend) {
            var chat = await _apiHandler.GetPrivateChat(friend).ConfigureAwait(false);
            if (chat == null)
                return null;
            UserInfo.PrivateChats.AddWhenMissing(chat);
            return chat;
        }

        void ClearLists() {
            HandleDisposables();
            AddFriends.Clear();
            UserInfo.Clear();
        }
    }
}