// <copyright company="SIX Networks GmbH" file="AddMemberToGroupViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.UseCases.Groups;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Connect;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Connect.Overlays
{
    [DoNotObfuscate]
    public class AddMemberToGroupViewModel : OverlayViewModelBase
    {
        readonly ConnectViewModel _connect;
        readonly object _itemsLock = new object();
        readonly IMediator _mediator;
        string _filterText;
        bool _isSending;
        int _selectedCount;
        ReactiveList<ContactDataModel> _selectedItems;

        public AddMemberToGroupViewModel(ConnectViewModel connect, IMediator mediator) {
            _connect = connect;
            _mediator = mediator;

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

            this.SetCommand(x => x.OkCommand, this.WhenAnyValue(x => x.SelectedCount, x => x > 0), false)
                .RegisterAsyncTask(Process)
                .Subscribe();

            DisplayName = "Add member to group";
        }

        public Group Group { get; private set; }
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
        public bool IsSending
        {
            get { return _isSending; }
            set { SetProperty(ref _isSending, value); }
        }

        public void SetGroup(Group group) {
            Contract.Requires<ArgumentNullException>(group != null);
            Group = group;
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

        public void SetCurrent(ContactDataModel contact) {
            ItemsView.MoveCurrentTo(contact);
            if (contact != null)
                SelectedItems.Add(contact);
            else
                SelectedItems.Clear();
        }

        public void LoadContacts() {
            _connect.Contacts.OfType<UserContactDataModel>()
                .Where(x => !Group.Members.Select(m => m.Id).Contains(x.Friend.Id))
                .SyncCollection(Items);
        }

        [DoNotObfuscate]
        public void SendMessage() {
            if (SelectedItems.Any() && OkCommand.CanExecute(null))
                OkCommand.Execute(null);
        }

        async Task Process() {
            IsSending = true;
            try {
                await
                    _mediator.RequestAsyncWrapped(new AddMembersToGroupCommand(Group.Id,
                        SelectedItems.Select(x => x.Model.Id).ToArray()));
            } finally {
                IsSending = false;
            }
            TryClose(true);
        }

        [DoNotObfuscate]
        public void Cancel() {
            TryClose(false);
        }
    }
}