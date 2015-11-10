// <copyright company="SIX Networks GmbH" file="PickContactViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class PickContactViewModel : OverlayViewModelBase
    {
        readonly ConnectViewModel _connect;
        readonly IConnectApiHandler _handler;
        readonly object _itemsLock = new object();
        string _filterText;
        bool _isSending;
        int _selectedCount;
        ReactiveList<ContactDataModel> _selectedItems;
        // TODO: Convert to commands so we can just use the mediator instead of many deps
        public PickContactViewModel(ConnectViewModel connect, IConnectApiHandler handler) {
            _connect = connect;
            _handler = handler;
            Input = new ChatInput();

            Items = new ReactiveList<ContactDataModel>();
            UiHelper.TryOnUiThread(() => {
                Items.EnableCollectionSynchronization(_itemsLock);
                ItemsView =
                    Items.CreateCollectionView(new List<SortDescription> {
                        new SortDescription("SortKey", ListSortDirection.Ascending),
                        new SortDescription("Model.DisplayName", ListSortDirection.Ascending)
                    }, null, new List<string> {"Model.DisplayName"}, OnFilter, true);
            });
            SelectedItems = new ReactiveList<ContactDataModel>();
            SelectedItems.CountChanged.Subscribe(x => SelectedCount = x);
            SelectedCount = SelectedItems.Count;

            this.WhenAnyValue(x => x.FilterText)
                .Throttle(Common.AppCommon.DefaultFilterDelay)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ItemsView.TryRefreshIfHasView());

            this.SetCommand(x => x.OkCommand,
                this.WhenAnyValue(x => x.SelectedCount, x => x.Input.Body,
                    (x, y) => x > 0 && !string.IsNullOrWhiteSpace(y)), false)
                .RegisterAsyncTask(Process)
                .Subscribe();

            OkCommand.IsExecuting.Subscribe(x => IsSending = x);

            DisplayName = "Share with Contact";
        }

        public int SelectedCount
        {
            get { return _selectedCount; }
            set { SetProperty(ref _selectedCount, value); }
        }
        public string FilterText
        {
            get { return _filterText; }
            set { SetProperty(ref _filterText, value); }
        }
        public ICollectionView ItemsView { get; private set; }
        public ReactiveCommand OkCommand { get; private set; }
        public ReactiveList<ContactDataModel> Items { get; }
        public ReactiveList<ContactDataModel> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }
        public IContent Content { get; private set; }
        public ChatInput Input { get; }
        public bool IsSending
        {
            get { return _isSending; }
            set { SetProperty(ref _isSending, value); }
        }

        bool OnFilter(object obj) {
            var contact = obj as ContactDataModel;
            return contact != null &&
                   (string.IsNullOrWhiteSpace(FilterText) ||
                    contact.Model.DisplayName.NullSafeContainsIgnoreCase(FilterText));
        }

        [DoNotObfuscate]
        public void SelectionChanged(SelectionChangedEventArgs args) {
            SelectedItems.RemoveRange(args.RemovedItems.Cast<ContactDataModel>().ToArray());
            SelectedItems.AddRange(args.AddedItems.Cast<ContactDataModel>());
        }

        public void SetContent(IContent content) {
            Content = content;
            Input.Body = "Hey check out this awesome " + GetTypeString(content);
        }

        static string GetTypeString(IContent content) {
            if (content is Collection)
                return "collection!";
            if (content is ContentManager.FakeContent)
                return "content!";
            return (content is IMod ? "mod!" : "mission!");
        }

        public void SetCurrent(ContactDataModel contact) {
            ItemsView.MoveCurrentTo(contact);
            if (contact != null)
                SelectedItems.Add(contact);
            else
                SelectedItems.Clear();
        }

        public Task Load(IContent content) {
            return Task.Run(() => {
                SetContent(content);
                LoadContacts();
            });
        }

        void LoadContacts() {
            lock (_connect.Contacts)
                _connect.Contacts.OfType<GroupContactDataModel>()
                    .Concat<ContactDataModel>(_connect.Contacts.OfType<UserContactDataModel>())
                    .SyncCollection(Items);
        }

        [DoNotObfuscate]
        public void SendMessage() {
            if (SelectedItems.Any() && OkCommand.CanExecute(null))
                OkCommand.Execute(null);
        }

        async Task Process() {
            var input = new ChatInput {Body = Input.Body + "\n" + Content.ProfileUrl()};
            foreach (var item in SelectedItems.Select(x => x.Model).ToArray())
                await SendMessage(input, item).ConfigureAwait(false);
            TryClose(true);
        }

        Task SendMessage(ChatInput input, IContact item) {
            var friend = item as Friend;
            return friend != null ? SendMessage(friend, input) : SendMessage(item as Group, input);
        }

        Task SendMessage(Group group, ChatInput input) {
            return _handler.SendGroupChatMessage(input, new GroupChat(group.Id) {Group = group});
        }

        Task SendMessage(Friend friend, ChatInput input) {
            return _handler.SendPrivateChatMessage(input, new PrivateChat(friend.Id) {Friend = friend});
        }

        [DoNotObfuscate]
        public void Cancel() {
            TryClose(false);
        }
    }
}