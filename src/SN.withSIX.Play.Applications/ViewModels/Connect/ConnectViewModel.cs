// <copyright company="SIX Networks GmbH" file="ConnectViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using MoreLinq;
using ReactiveUI;
using ReactiveUI.Legacy;
using ShortBus;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.UseCases.Groups;
using SN.withSIX.Play.Applications.ViewModels.Connect.Overlays;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Filters;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    [DoNotObfuscate]
    public class ConnectViewModel : ScreenBase<IPlayShellViewModel>, IHandle<ApiKeyUpdated>, IDropTarget
    {
        readonly ISubject<int> _addFriendFilterChanged;
        readonly object _addFriendsLock = new object();
        readonly ExportFactory<AddMemberToGroupViewModel> _addMemberToGroupFactory;
        readonly object _chatsLock = new object();
        readonly object _contactsLock = new object();
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly IMediator _mediator;
        readonly ExportFactory<PickContactViewModel> _pickContactFactory;
        readonly ExportFactory<ConnectPopoutViewModel> _popoutFactory;
        readonly UserSettings _settings;
        readonly IWindowManager _windowManager;
        string _addFriendInput;
        string _addGroupInput;
        bool _chatVisible;
        bool _isEnabled;
        bool _isProfileShown;
        ChatViewModel _previousChat;
        AddFriend _selectedAddFriend;
        ChatViewModel _selectedChat;
        ContactDataModel _selectedContact;
        bool _showContactsMenu;
        bool _showFindFriend;
        bool _showFindGroup;
        UserContactDataModel _srcFriend;
        GroupContactDataModel _targetGroup;
        bool _wasEnabled;

        public ConnectViewModel(ContactList contactList, IDialogManager dialogManager,
            IWindowManager windowManager,
            ModsViewModel mods, MissionsViewModel missions, IMediator mediator,
            ExportFactory<ConnectPopoutViewModel> popoutFactory, ExportFactory<PickContactViewModel> pickContactFactory,
            ExportFactory<AddMemberToGroupViewModel> addMemberToGroupFactory, UserSettings settings,
            IEventAggregator ea) {
            ContactList = contactList;
            _dialogManager = dialogManager;
            _windowManager = windowManager;
            _mediator = mediator;
            _popoutFactory = popoutFactory;
            _settings = settings;
            _eventBus = ea;
            Mods = mods;
            Missions = missions;
            _pickContactFactory = pickContactFactory;
            _addMemberToGroupFactory = addMemberToGroupFactory;

            _wasEnabled = IsEnabled;
            IsEnabled = false;

            Chats = new ReactiveList<IChat>();
            Contacts = new ReactiveList<ContactDataModel>();
            ContactList.AddFriends.EnableCollectionSynchronization(_addFriendsLock);
            Contacts.EnableCollectionSynchronization(_contactsLock);
            Chats.EnableCollectionSynchronization(_chatsLock);

            ContactFilter = _settings.AccountOptions.Filter;

            UserContactContextMenu = new UserContactContextMenu(this);
            GroupMemberContextMenu = new GroupMemberContextMenu(this);
            GroupContextMenu = new GroupContextMenu(this, mediator, _dialogManager);
            InviteRequestContextMenu = new InviteRequestContextMenu(this);
            MessageContextMenu = new MessageContextMenu(this);
            ChatContextMenu = new ChatContextMenu(this);

            _addFriendFilterChanged = new Subject<int>();
        }

        protected ConnectViewModel() {}
        public ReactiveCommand RemoveFriendFromGroupCommand { get; private set; }
        public GroupMemberContextMenu GroupMemberContextMenu { get; private set; }
        public ReactiveList<IChat> Chats { get; }
        public ReactiveCommand SwitchShowAll { get; private set; }
        public ReactiveCommand SwitchShowOnlyOnline { get; private set; }
        public ReactiveCommand SwitchShowOnlyIngame { get; private set; }
        public ReactiveCommand LogoutCommand { get; private set; }
        public bool ShowContactsMenu
        {
            get { return _showContactsMenu; }
            set { SetProperty(ref _showContactsMenu, value); }
        }
        public bool IsProfileShown
        {
            get { return _isProfileShown; }
            set { SetProperty(ref _isProfileShown, value); }
        }
        public ModsViewModel Mods { get; set; }
        public UserContactContextMenu UserContactContextMenu { get; private set; }
        public WebBrowser ApiBrowser { get; set; }
        public ReactiveCommand SwitchContactListCommand { get; private set; }
        public ContactList ContactList { get; }
        public ReactiveCommand JoinFriendServer { get; private set; }
        public ReactiveCommand VisitProfileCommand { get; private set; }
        public ReactiveCommand EditProfileCommand { get; private set; }
        public ReactiveCommand RetryConnectionCommand { get; private set; }
        public ReactiveCommand LoginCommand { get; private set; }
        public ReactiveCommand ContactsMenuCommand { get; private set; }
        public ReactiveCommand RegisterCommand { get; private set; }
        public ReactiveCommand AddFriendCommand { get; private set; }
        public ReactiveCommand AdvancedChatCommand { get; private set; }
        public ReactiveCommand AdvancedFriendsCommand { get; private set; }
        public ReactiveCommand AdvancedGroupsCommand { get; private set; }
        public ReactiveCommand FindFriendCommand { get; protected set; }
        public ReactiveCommand FindGroupCommand { get; protected set; }
        public ReactiveCommand NewGroupCommand { get; protected set; }
        public ReactiveCommand ResetHiddenInviteRequests { get; protected set; }
        public ReactiveCommand OChat { get; private set; }
        public ReactiveCommand Invite { get; private set; }
        public ReactiveCommand Remove { get; private set; }
        public ReactiveCommand ApproveCommand { get; private set; }
        public ReactiveCommand DeclineCommand { get; private set; }
        public ReactiveCommand HideCommand { get; private set; }
        public ReactiveCommand AddAsFriend { get; private set; }
        public ReactiveCommand ReplyToUser1 { get; private set; }
        public ReactiveCommand ReplyToUser2 { get; private set; }
        public ReactiveCommand VisitUserProfile { get; private set; }
        public ReactiveCommand LeaveChatCommand { get; private set; }
        public ContactFilter ContactFilter { get; }
        public ReactiveList<ContactDataModel> Contacts { get; }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
        public bool ShowFindFriend
        {
            get { return _showFindFriend; }
            set
            {
                if (!SetProperty(ref _showFindFriend, value))
                    return;
                if (value)
                    ShowFindGroup = false;
            }
        }
        public bool ShowFindGroup
        {
            get { return _showFindGroup; }
            set
            {
                if (!SetProperty(ref _showFindGroup, value))
                    return;
                if (value)
                    ShowFindFriend = false;
            }
        }
        public string AddFriendInput
        {
            get { return _addFriendInput; }
            set
            {
                if (_addFriendInput == value)
                    return;

                //OnPropertyChanging();
                if (value != null) {
                    var str = value;
                    _addFriendInput = str.Trim();
                } else
                    _addFriendInput = null;
                OnPropertyChanged();
                PublishFilter();
            }
        }
        public string AddGroupInput
        {
            get { return _addGroupInput; }
            set
            {
                if (_addGroupInput == value)
                    return;

                //OnPropertyChanging();
                if (value != null) {
                    var str = value;
                    _addGroupInput = str.Trim();
                } else
                    _addGroupInput = null;

                OnPropertyChanged();
            }
        }
        public ContactDataModel SelectedContact
        {
            get { return _selectedContact; }
            set { SetProperty(ref _selectedContact, value); }
        }
        public ChatViewModel SelectedChat
        {
            get { return _selectedChat; }
            private set { SetProperty(ref _selectedChat, value); }
        }
        public AddFriend SelectedAddFriend
        {
            get { return _selectedAddFriend; }
            set { SetProperty(ref _selectedAddFriend, value); }
        }
        public GroupContextMenu GroupContextMenu { get; private set; }
        public MessageContextMenu MessageContextMenu { get; }
        public ChatContextMenu ChatContextMenu { get; private set; }
        public InviteRequestContextMenu InviteRequestContextMenu { get; private set; }
        public MissionsViewModel Missions { get; private set; }
        public bool ChatVisible
        {
            get { return _chatVisible; }
            set { SetProperty(ref _chatVisible, value); }
        }
        public ReactiveCommand AddFriendToGroupCommand { get; private set; }

        public void DragOver(IDropInfo dropInfo) {
            if (dropInfo.Data == dropInfo.TargetItem)
                return;

            var srcItem = dropInfo.Data as UserContactDataModel;
            if (srcItem == null)
                return;

            var targetItem = dropInfo.TargetItem as GroupContactDataModel;
            if (targetItem == null || !targetItem.Group.IsMine)
                return;

            if (targetItem.Members.Any(x => x.Friend == srcItem.Friend))
                return;

            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Copy;
            dropInfo.DestinationText = "Add to " + targetItem.Model.DisplayName;
        }

        public void Drop(IDropInfo dropInfo) {
            if (dropInfo.Data == dropInfo.TargetItem)
                return;

            var srcItem = dropInfo.Data as UserContactDataModel;
            if (srcItem == null)
                return;

            var targetItem = dropInfo.TargetItem as GroupContactDataModel;
            if (targetItem == null || !targetItem.Group.IsMine)
                return;

            if (targetItem.Members.Any(x => x.Friend == srcItem.Friend))
                return;

            _targetGroup = targetItem;
            _srcFriend = srcItem;

            AddFriendToGroupCommand.Execute(null);
        }

        #region IHandle events

        public void Handle(ApiKeyUpdated message) {
            if (string.IsNullOrWhiteSpace(message.ApiKey))
                IsEnabled = false;
        }

        #endregion

        [DoNotObfuscate]
        public void Popout() {
            _windowManager.ShowWindow(_popoutFactory.CreateExport().Value);
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            this.WhenAnyValue(x => x.IsEnabled).Where(x => x).Subscribe(x => _wasEnabled = x);

            this.WhenAnyValue(x => x.ContactList.OnlineStatus)
                .Subscribe(x => IsProfileShown = false);

            this.WhenAnyValue(x => x.ContactList.ActiveChat)
                .Subscribe(x => {
                    var vm = x == null ? null : new ChatViewModel(x, ContactList);
                    SelectedChat = vm;
                    if (_previousChat != null)
                        _previousChat.Dispose();
                    _previousChat = vm;
                    if (x == null)
                        return;
                    if (!x.Loaded && !Execute.InDesignMode)
                        UpdateChat(x);
                });

            MonitorChats();
            MonitorContacts();

            if (!Execute.InDesignMode) {
                _addFriendFilterChanged
                    .Throttle(Common.AppCommon.DefaultFilterDelay).Subscribe(RefreshAddFriends);
            }

            this.SetCommand(x => x.AddFriendToGroupCommand).RegisterAsyncTask(AddFriendToGroup).Subscribe();
            this.SetCommand(x => x.RemoveFriendFromGroupCommand).RegisterAsyncTask(RemoveFriendFromGroup).Subscribe();
            this.SetCommand(x => ContactsMenuCommand).Subscribe(x => ShowContactsMenu = !ShowContactsMenu);
            this.SetCommand(x => x.RegisterCommand).Subscribe(x => Register());
            this.SetCommand(x => x.ResetHiddenInviteRequests).Subscribe(ResetHiddenRequests);
            this.SetCommand(x => x.LoginCommand).Subscribe(x => Login());
            this.SetCommand(x => x.RetryConnectionCommand).RegisterAsyncTask(ContactList.HandleConnection).Subscribe();
            this.SetCommand(x => x.EditProfileCommand).Subscribe(x => ShowEditProfile());
            this.SetCommand(x => x.LogoutCommand).RegisterAsyncTask(x => Task.Run(() => Logout())).Subscribe();
            this.SetCommand(x => x.VisitProfileCommand).Subscribe(x => VisitProfile((ContactDataModel) x));
            this.SetCommand(x => x.JoinFriendServer)
                .RegisterAsyncTask(x => JoinServer((Friend) ((ContactDataModel) x).Model))
                .Subscribe();
            this.SetCommand(x => x.OChat).RegisterAsyncTask(target => OpenChat((ContactDataModel) target)).Subscribe();
            this.SetCommand(x => x.Remove).Subscribe(x => RemoveContact((UserContactDataModel) x));
            this.SetCommand(x => x.Invite).Subscribe(InviteToServer);
            this.SetCommand(x => x.FindFriendCommand).Subscribe(x => ShowFindFriendDialog());
            this.SetCommand(x => x.FindGroupCommand).Subscribe(x => ShowFindGroupDialog());
            this.SetCommand(x => x.NewGroupCommand).Subscribe(x => ShowNewGroupDialog());
            this.SetCommand(x => x.AddFriendCommand).RegisterAsyncTaskVoid<AddFriend>(AddFriend).Subscribe();
            this.SetCommand(x => x.AdvancedChatCommand).Subscribe(x => AdvancedChat());
            this.SetCommand(x => x.AdvancedFriendsCommand).Subscribe(x => AdvancedFriends());
            this.SetCommand(x => x.AdvancedGroupsCommand).Subscribe(x => AdvancedGroups());
            this.SetCommand(x => x.LeaveChatCommand).Subscribe(LeaveChat);
            this.SetCommand(x => x.SwitchShowOnlyIngame).Subscribe(x => ContactFilter.SwitchShowOnlyIngame());
            this.SetCommand(x => x.SwitchShowOnlyOnline).Subscribe(x => ContactFilter.SwitchShowOnlyOnline());
            this.SetCommand(x => x.SwitchShowAll).Subscribe(x => ContactFilter.ShowAll());
            this.SetCommand(x => x.AddAsFriend).RegisterAsyncTaskVoid<ChatMessage>(AddAsFriendAction).Subscribe();
            this.SetCommand(x => x.ReplyToUser1).Subscribe(x => Reply((ChatMessage) x));
            this.SetCommand(x => x.ReplyToUser2).Subscribe(x => Reply((Account) x));
            this.SetCommand(x => x.VisitUserProfile).Subscribe(x => {
                var message = x as ChatMessage;
                if (message != null)
                    VisitProfile(message.Author);
                else {
                    var user = x as Account;
                    if (user != null)
                        VisitProfile(user);
                }
            });

            this.SetCommand(x => x.SwitchContactListCommand).Subscribe(SwitchContactList);
            this.SetCommand(x => x.ApproveCommand).RegisterAsyncTaskVoid<ContactDataModel>(ApproveInvite).Subscribe();
            this.SetCommand(x => x.DeclineCommand).RegisterAsyncTaskVoid<ContactDataModel>(DeclineInvite).Subscribe();
            this.SetCommand(x => x.HideCommand).Subscribe(x => HideInvite((ContactDataModel) x));

            this.WhenAnyValue(x => x.ChatVisible)
                .Skip(1)
                .Subscribe(value => UsageCounter.ReportUsage(!value ? "Tab Friends" : "Tab Chat"));

            this.WhenAnyValue(x => x.SelectedContact)
                .Where(x => x != null)
                .Subscribe(value => value.Selected());

            this.WhenAnyValue(x => x.SelectedChat.SelectedChatMessage)
                .Where(x => x != null)
                .Subscribe(x => MessageContextMenu.SetNextItem(x));

            ContactFilter.PublishFilterInternal();

            this.WhenAnyValue(x => x.ContactList.LoginState, x => x.ContactList.ConnectedState,
                (loggedIn, connected) => new {LoggedIn = loggedIn, Connected = connected})
                .Subscribe(value => LoginOrConnectedStateChanged(value.LoggedIn, value.Connected));

            this.WhenAnyValue(x => x.IsEnabled, x => x.ChatVisible, (x, y) => x && y)
                .Subscribe(x => ContactList.IsChatEnabled = x);

            this.WhenAnyValue(x => x.ChatVisible)
                .Skip(1)
                .Where(x => !x)
                .Subscribe(x => {
                    var chat = ContactList.ActiveChat;
                    if (chat != null)
                        chat.MarkAllAsRead();
                });
        }

        void MonitorChats() {
            ContactList.UserInfo.PublicChats.TrackChangesOnUiThread(
                Chats.Add,
                x => Chats.Remove(x),
                reset => reset.SyncCollectionOfTypeLocked(Chats));

            ContactList.UserInfo.GroupChats.TrackChangesOnUiThread(
                Chats.Add,
                x => Chats.Remove(x),
                reset => reset.SyncCollectionOfTypeLocked(Chats));

            ContactList.UserInfo.PrivateChats.TrackChangesOnUiThread(
                Chats.Add,
                x => Chats.Remove(x),
                reset => reset.SyncCollectionOfTypeLocked(Chats));
        }

        void MonitorContacts() {
            ContactList.UserInfo.PublicChats.TrackChangesOnUiThread(CreateAndAddChatContact,
                RemoveContactByEntity,
                reset => reset.SyncCollectionConvertCustomLockedPK<IContact, ContactDataModel>(Contacts,
                    x => CreateChatContact((PublicChat) x), g => g.Model is PublicChat));


            ContactList.UserInfo.Groups.TrackChangesOnUiThread(
                CreateAndAddGroupContact,
                RemoveContactByEntity,
                reset => reset.SyncCollectionConvertCustomLockedPK<IContact, ContactDataModel>(Contacts,
                    x => CreateGroupContact((Group) x), g => g.Model is Group));

            ContactList.UserInfo.Friends.TrackChangesOnUiThread(
                CreateAndAddFriendContact,
                RemoveContactByEntity,
                reset => reset.SyncCollectionConvertCustomLockedPK<IContact, ContactDataModel>(Contacts,
                    x => CreateFriendContact((Friend) x), g => g.Model is Friend));

            ContactList.UserInfo.InviteRequests.TrackChangesOnUiThread(
                CreateAndAddInviteRequestContact,
                RemoveContactByEntity,
                reset => reset.SyncCollectionConvertCustomLockedPK<IContact, ContactDataModel>(Contacts,
                    CreateInviteRequestContact, g => {
                        var request = g.Model as InviteRequest;
                        return request != null;
                    }), x => !x.IsMine);

            ContactList.UserInfo.InviteRequests.TrackChangesOnUiThread(
                CreateAndAddInviteRequestContact,
                RemoveContactByEntity,
                reset =>
                    reset.SyncCollectionConvertCustomLockedPK<IContact, ContactDataModel>(Contacts,
                        CreateInviteRequestContact,
                        g => {
                            var request = g.Model as InviteRequest;
                            return request != null;
                        }), x => x.IsMine);
        }

        Task RemoveFriendFromGroup() {
            return RemoveFriendFromGroup(_srcFriend.Friend, _targetGroup.Group);
        }

        async Task RemoveFriendFromGroup(Friend friend, Group @group) {
            if (
                (await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(
                        String.Format("Do you really want to remove: {0} from the {1} group?", friend.DisplayName,
                            @group.DisplayName), "Remove user from group?", SixMessageBoxButton.YesNo))).IsYes()) {
                await
                    _mediator.RequestAsyncWrapped(new RemoveUserFromGroupCommand(friend.Id, @group.Id))
                        .ConfigureAwait(false);
            }
        }

        async Task AddFriendToGroup() {
            if (
                (await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(
                        String.Format("Do you really want to add: {0} to the {1} group?", _srcFriend.Friend.DisplayName,
                            _targetGroup.Group.DisplayName), "Add user to group?", SixMessageBoxButton.YesNo))).IsYes()) {
                await
                    _mediator.RequestAsyncWrapped(new AddFriendToGroupCommand(_srcFriend.Friend.Id,
                        _targetGroup.Group.Id)).ConfigureAwait(false);
            }
        }

        UserContactDataModel CreateFriendContact(Friend arg) {
            return new UserContactDataModel(arg, ContactList, this);
        }

        void CreateAndAddInviteRequestContact(InviteRequest entity) {
            Contacts.Add(CreateInviteRequestContact(entity));
        }

        InviteRequestContactDataModel CreateInviteRequestContact(IContact entity) {
            return new InviteRequestContactDataModel(entity, ContactList, this);
        }

        void CreateAndAddChatContact(PublicChat entity) {
            Contacts.Add(CreateChatContact(entity));
        }

        ChatContactDataModel CreateChatContact(PublicChat entity) {
            return new ChatContactDataModel(entity, ContactList, this);
        }

        void CreateAndAddFriendContact(Friend obj) {
            Contacts.Add(CreateFriendContact(obj));
        }

        GroupContactDataModel CreateGroupContact(Group arg) {
            return new GroupContactDataModel(arg, ContactList, this);
        }

        void CreateAndAddGroupContact(Group obj) {
            Contacts.Add(CreateGroupContact(obj));
        }

        void LoginOrConnectedStateChanged(LoginState loginState, ConnectedState connectedState) {
            if (connectedState == ConnectedState.ConnectingFailed || connectedState == ConnectedState.Disconnected ||
                loginState == LoginState.LoggedOut || loginState == LoginState.InvalidLogin ||
                connectedState == ConnectedState.Connecting)
                IsEnabled = false;

            if (connectedState == ConnectedState.Connected && loginState == LoginState.LoggedIn)
                IsEnabled = _wasEnabled;
        }

        [DoNotObfuscate]
        public void SwitchProfileShown() {
            IsProfileShown = !IsProfileShown;
        }

        [DoNotObfuscate]
        public void ToggleChat() {
            ChatVisible = !ChatVisible;
        }

        public async Task JoinServer(Friend friend) {
            var addr = friend.PlayingOn;
            if (addr == null) {
                UsageCounter.ReportUsage("Dialog - The user doesn't appear to be playing");
                await
                    _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("The user doesn't appear to be playing"));
                return;
            }
            await ContactList.JoinServer(addr).ConfigureAwait(false);
        }

        [DoNotObfuscate]
        public void SendMessage() {
            var chat = SelectedChat;
            if (chat == null)
                return;
            chat.SendMessageCommand.Execute(null);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void LeaveChat(object x) {
            ContactList.LeaveChat(((ChatViewModel) x).Model);
        }

        public void ExecuteJS(string function, params object[] pars) {
            ApiBrowser.InvokeScript(function, pars);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async Task ApproveInvite(InviteRequest request) {
            Contract.Requires<ArgumentNullException>(request != null);
            try {
                await ContactList.ApproveInvite(request);
            } catch (RestExceptionBase e) {
                this.Logger().FormattedWarnException(e);
                UsageCounter.ReportUsage("Dialog - Failed to approve user");
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(String.Format("Failed to Approve {0}", request.Account.DisplayName)));
            }
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async Task DeclineInvite(InviteRequest request) {
            Contract.Requires<ArgumentNullException>(request != null);
            try {
                await ContactList.DeclineInvite(request);
            } catch (RestExceptionBase e) {
                this.Logger().FormattedWarnException(e);
                UsageCounter.ReportUsage("Dialog - Failed to block user");
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(String.Format("Failed to Block {0}", request.Account.DisplayName)));
            }
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void HideInvite(InviteRequest request) {
            Contract.Requires<ArgumentNullException>(request != null);
            request.IsHidden = true;
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void VisitProfile(Account account) {
            Contract.Requires<ArgumentNullException>(account != null);
            ContactList.VisitProfile(account);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void VisitProfile(ContactDataModel contact) {
            Contract.Requires<ArgumentNullException>(contact != null);
            ContactList.VisitProfile(contact.Model);
        }

        public Task<IChat> OpenChat(ContactDataModel contact) {
            return OpenChat(contact.Model);
        }

        public Task RemoveContact(UserContactDataModel contact) {
            return ContactList.RemoveFriend(contact.Friend);
        }

        public Task RemoveContact(Friend contact) {
            return ContactList.RemoveFriend(contact);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void AddGroupOK() {
            CloseAddGroup();
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void AddGroupCancel() {
            CloseAddGroup();
        }

        [DoNotObfuscate]
        public void DoubleClicked(RoutedEventArgs Args) {
            if (Args.FilterControlFromDoubleClick())
                return;

            var contact = Args.FindListBoxItem<ContactDataModel>();
            if (contact != null)
                OpenChat(contact);

            Args.Handled = true;
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ShowFindFriendDialog() {
            ShowContactsMenu = false;
            ShowFindFriend = !ShowFindFriend;
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ShowFindGroupDialog() {
            ShowContactsMenu = false;
            ShowFindGroup = !ShowFindGroup;
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async void ShowNewGroupDialog() {
            ShowContactsMenu = false;
            var vm = _mediator.Request(new ShowNewGroupDialogQuery());
            await _dialogManager.ShowDialogAsync(vm);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void Reply(ChatMessage message) {
            Reply(message.Author);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void Reply(Account message) {
            var chat = SelectedChat;
            if (chat != null)
                chat.ChatMessageEditor.Body += String.Format("@{0}: ", message.DisplayName);
        }

        public ContactDataModel FindContact(IContact e) {
            return Contacts.FirstOrDefault(x => x.Model == e)
                   ?? Contacts.FirstOrDefault(x => x.Model.Id == e.Id);
        }

        public ContactDataModel FindContact(Friend friend) {
            return Contacts.OfType<UserContactDataModel>().FirstOrDefault(x => x.Friend == friend);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void AdvancedChat() {
            var chat = SelectedChat;
            //ContactList.AdvancedChat(chat == null ? null : chat.Model);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void AdvancedFriends() {
            ContactList.AdvancedFriends();
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void AdvancedGroups() {
            ContactList.AdvancedGroups();
        }

        public void ViewConversationOnline(ContactDataModel entity) {
            ContactList.VisitConversationOnline(entity.Model);
        }

        public async Task AddFriend(Account user) {
            if (ContactList.IsMe(user))
                return;

            var bud = ContactList.FindFriend(user);
            var invite = ContactList.FindInviteRequest(user.Id);
            if (bud != null)
                SelectContact(bud);
            else if (invite != null)
                SelectContact(invite);
            else
                await TryAddFriend(user).ConfigureAwait(false);
        }

        /*
        public async Task ShareSelectedMod(UserContactDataModel entity) {
            var mod = Mods.GetSelectedMod();
            if (mod == null)
                return;

            await ShareToContact(entity, mod).ConfigureAwait(false);
        }

        public async Task ShareSelectedMod(GroupContactDataModel entity) {
            var mod = Mods.GetSelectedMod();
            if (mod == null)
                return;

            await ShareToContact(entity, mod).ConfigureAwait(false);
        }

        public async Task ShareSelectedMission(UserContactDataModel entity) {
            var mission = Missions.GetSelectedMission();
            if (mission == null)
                return;

            await ShareToContact(entity, mission).ConfigureAwait(false);
        }

        public async Task ShareSelectedMission(GroupContactDataModel entity) {
            var mission = Missions.GetSelectedMission();
            if (mission == null)
                return;

            await ShareToContact(entity, mission).ConfigureAwait(false);
        }
*/

        public async Task MarkAsRead(Friend friend) {
            await ContactList.MarkAsRead(friend.Account).ConfigureAwait(false);
            friend.UnreadPrivateMessages = 0;
        }

        Task DeclineInvite(ContactDataModel contact) {
            return DeclineInvite((InviteRequest) contact.Model);
        }

        [SmartAssembly.Attributes.ReportUsage]
        void Login() {
            ContactList.RetrieveApiKey();
        }

        [SmartAssembly.Attributes.ReportUsage]
        void Register() {
            _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(CommonUrls.RegisterUrl));
        }

        [SmartAssembly.Attributes.ReportUsage]
        void ShowEditProfile() {
            BrowserHelper.TryOpenUrlIntegrated(CommonUrls.AccountSettingsUrl);
            IsProfileShown = false;
        }

        [SmartAssembly.Attributes.ReportUsage("AddFriendFilter: PublishFilter")]
        void PublishFilter() {
            _addFriendFilterChanged.OnNext(1);
        }

        Task ApproveInvite(ContactDataModel contact) {
            return ApproveInvite((InviteRequest) contact.Model);
        }

        void Logout() {
            if (String.IsNullOrWhiteSpace(_settings.AccountOptions.AccessToken)) {
                UsageCounter.ReportUsage("Dialog - Appear logged out");
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams("You already appear logged out"));
                Common.App.PublishEvent(new DoLogout());
                return;
            }

            if (!_settings.AppOptions.RememberWarnOnLogout) {
                var r =
                    _dialogManager.MessageBoxSync(new MessageBoxDialogParams("Do you want to log out?", "Are you sure?",
                        SixMessageBoxButton.YesNo) {RememberedState = false});

                if (r == SixMessageBoxResult.YesRemember) {
                    _settings.AppOptions.RememberWarnOnLogout = true;
                    _settings.AppOptions.WarnOnLogout = true;
                    UsageCounter.ReportUsage("Dialog - Remember Warn On Logout");
                } else if (r == SixMessageBoxResult.NoRemember) {
                    _settings.AppOptions.RememberWarnOnLogout = true;
                    _settings.AppOptions.WarnOnLogout = false;
                    UsageCounter.ReportUsage("Dialog - Don't remember Warn On Logout");
                }

                if (r.IsYes())
                    DoLogout();
            } else {
                if (_settings.AppOptions.WarnOnLogout)
                    DoLogout();
            }
        }

        void DoLogout() {
            _settings.AccountOptions.AccessToken = null;
            IsProfileShown = false;
            Common.App.PublishEvent(new DoLogout());
            //            Common.App.PublishEvent(new RequestOpenBrowser(CommonUrls.LogoutUrl));
        }

        void HideInvite(ContactDataModel contact) {
            Contract.Requires<ArgumentNullException>(contact != null);

            var request = contact.Model as InviteRequest;
            if (request == null)
                return;

            HideInvite(request);
        }

        void SelectContact(IContact contact) {
            ChatVisible = false;
            SelectedContact = FindContact(contact);
        }

        void CloseAddFriend() {
            AddFriendInput = null;
            ShowFindFriend = false;
        }

        Task<IChat> OpenChat(IContact entity) {
            var bud = entity as Friend;
            if (bud != null)
                return OpenChat(bud);

            var chat = entity as PublicChat;
            if (chat != null)
                return Task.FromResult(OpenChat(chat));

            var group = entity as Group;
            return group != null ? OpenChat(group) : Task.FromResult((IChat) null);
        }

        async Task<IChat> OpenChat(Group group) {
            var chat = await ContactList.GetOrRetrieveChat(group).ConfigureAwait(false);
            SelectChat(chat, group);
            return chat;
        }

        IChat OpenChat(PublicChat chat) {
            SelectChat(chat);
            return chat;
        }

        async Task<IChat> OpenChat(Friend friend) {
            // Scizophrene...
            if (friend.Id == ContactList.UserInfo.Account.Id)
                return null;

            var chat = await ContactList.GetOrRetrieveChat(friend).ConfigureAwait(false);
            friend.UnreadPrivateMessages = 0;
            SelectChat(chat);
            return chat;
        }

        void CloseAddGroup() {
            AddGroupInput = null;
            ShowFindGroup = false;
        }

        // Used because async subscriptions break in SmartAssembly
        async void UpdateChat(IChat chat) {
            await ContactList.GetChatData(chat).ConfigureAwait(false);
        }

        [SmartAssembly.Attributes.ReportUsage]
        void SwitchContactList(object x) {
            if (ContactList.ConnectedState == ConnectedState.ConnectingFailed ||
                ContactList.ConnectedState == ConnectedState.Disconnected ||
                ContactList.LoginState == LoginState.LoggedOut || ContactList.LoginState == LoginState.InvalidLogin) {
                IsEnabled = false;
                return;
            }
            IsEnabled = !IsEnabled;
        }

        [SmartAssembly.Attributes.ReportUsage]
        Task AddAsFriendAction(ChatMessage message) {
            return AddFriend(message.Author);
        }

        [SmartAssembly.Attributes.ReportUsage]
        void InviteToServer(object x) {
            ContactList.InviteToServer(((ContactDataModel) x).Model);
        }

        [SmartAssembly.Attributes.ReportUsage]
        void ResetHiddenRequests(object x) {
            ContactList.UserInfo.InviteRequests.ForEach(y => y.IsHidden = false);
        }

        // Used because async subscriptions break in SmartAssembly
        async void RefreshAddFriends(int i) {
            try {
                await ContactList.UpdateAddFriends(AddFriendInput).ConfigureAwait(false);
            } catch (RestExceptionBase e) {
                this.Logger().FormattedWarnException(e);
                UsageCounter.ReportUsage("Dialog - Failed to Retrieve list of users");
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(String.Format("Failed to Retrieve list of users for {0}", AddFriendInput)));
            }
        }

        void RemoveContactByEntity(IContact e) {
            var contact = FindContact(e);
            if (contact != null)
                Contacts.Remove(contact);
        }

        [SmartAssembly.Attributes.ReportUsage]
        async Task AddFriend(AddFriend addFriend) {
            await AddFriend(addFriend.Account).ConfigureAwait(false);
            addFriend.IsContact = true;
        }

        async Task TryAddFriend(Account friend) {
            try {
                var request = await ContactList.Befriend(friend).ConfigureAwait(false);
                SelectContact(request);
                CloseAddFriend();
            } catch (RestExceptionBase e) {
                this.Logger().FormattedWarnException(e);
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(String.Format("Failed to add {0}:\n{1}", friend.DisplayName, e.Message)));
            }
        }

        void SelectChat(IChat chat) {
            ContactList.ActiveChat = chat;
            ChatVisible = true;
        }

        void SelectChat(GroupChat chat, Group group) {
            var title = "Room: ";
            if (!String.IsNullOrWhiteSpace((group.Name)))
                title += group.Name;

            chat.Title = title;
            ContactList.ActiveChat = chat;
            ChatVisible = true;
        }

        async Task ShareToContact(ContactDataModel model, IContent item) {
            using (var vm = _pickContactFactory.CreateExport()) {
                await vm.Value.Load(item);
                vm.Value.SetCurrent(model);
                ParentShell.ShowOverlay(vm.Value);
            }
        }

        [DoNotObfuscate]
        public void AddMemberToGroup() {
            var chat = (GroupChat) ContactList.ActiveChat;
            AddMemberToGroup(chat.Group);
        }

        public void AddMemberToGroup(Group group) {
            using (var vm = _addMemberToGroupFactory.CreateExport()) {
                vm.Value.SetGroup(group);
                vm.Value.LoadContacts();
                ParentShell.ShowOverlay(vm.Value);
            }
        }

        public Task RemoveGroupMember(GroupMemberContactDataModel entity) {
            return RemoveFriendFromGroup(entity.Friend, entity.Group);
        }

        [DoNotObfuscate]
        public void VisitHomepage() {
            var chat = (GroupChat) ContactList.ActiveChat;
            VisitHomepage(chat.Group);
        }

        [DoNotObfuscate]
        public void VisitHomepage(GroupContactDataModel entity) {
            VisitHomepage(entity.Group);
        }

        [DoNotObfuscate]
        public void VisitHomepage(Group entity) {
            BrowserHelper.TryOpenUrlIntegrated(entity.Homepage);
        }

        bool Filter(IContact entity) {
            return entity != null && ContactFilter.Handler(entity);
        }

        public bool Filter(object o) {
            var cvm = (ContactDataModel) o;
            if (cvm == null)
                return false;
            var contact = cvm.Model;
            var request = contact as InviteRequest;
            if (request != null && request.IsHidden)
                return false;
            return contact != null && Filter(contact);
        }
    }

    public class DoLogout {}
}