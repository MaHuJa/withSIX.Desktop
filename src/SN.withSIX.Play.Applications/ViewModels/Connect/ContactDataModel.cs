// <copyright company="SIX Networks GmbH" file="ContactDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    // TODO: Stop wrapping the Domain Models, and instead use a DTO approach.
    public abstract class ContactDataModel : ViewModelBase, IDisposable, IHaveModel<IContact>
    {
        const string ModelProperty = "Model";
        protected readonly ConnectViewModel Connect;
        protected readonly ContactList ContactList;
        [DoNotObfuscate] protected ObservableAsPropertyHelper<int> _sortKey;

        protected ContactDataModel(IContact domainModel, ContactList contactList, ConnectViewModel connect) {
            Contract.Requires<ArgumentNullException>(domainModel != null);

            Model = domainModel;
            ContactList = contactList;
            Connect = connect;

            _sortKey = this.WhenAnyValue(x => x.Model)
                .Select(x => CalculateSortKey(1))
                .ToProperty(this, x => x.SortKey, 0, Scheduler.Immediate);

            Model.PropertyChanged += OnEntityOnPropertyChanged;
        }

        public int SortKey
        {
            get { return _sortKey.Value; }
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public IContact Model { get; protected set; }

        protected virtual void OnEntityOnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            OnPropertyChanged((String.IsNullOrEmpty(args.PropertyName)
                ? ModelProperty
                : ModelProperty + "." + args.PropertyName));
        }

        protected int CalculateSortKey(int state) {
            var invite = Model as InviteRequest;
            if (invite != null)
                return (int) (invite.IsMine ? OnlineStatusSortKey.MyInviteRequest : OnlineStatusSortKey.InviteRequest);

            if (Model is Group || Model is IChat)
                return (int) OnlineStatusSortKey.Group;
            if (state == 999)
                return (int) OnlineStatusSortKey.Playing;
            if (state == 1)
                return (int) OnlineStatusSortKey.Online;
            if (state == 2)
                return (int) OnlineStatusSortKey.Busy;
            if (state == 9)
                return (int) OnlineStatusSortKey.Away;
            return (int) OnlineStatusSortKey.Offline;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                Model.PropertyChanged -= OnEntityOnPropertyChanged;
            }
            // free native resources
        }

        public abstract void Selected();
    }
}