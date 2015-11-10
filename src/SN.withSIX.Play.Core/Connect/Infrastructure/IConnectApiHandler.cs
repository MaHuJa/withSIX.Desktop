// <copyright company="SIX Networks GmbH" file="IConnectApiHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Play.Core.Connect.Infrastructure.Components;

namespace SN.withSIX.Play.Core.Connect.Infrastructure
{
    public interface IConnectApiHandler : IConnectMissionsApi, IConnectCollectionsApi
    {
        MyAccount Me { get; }
        IMessageBus MessageBus { get; }
        Task Initialize(string key);
        void ConfirmLoggedIn();
        Task HandleAuthentication(string code, Uri callbackUri);
    }


    public class CollectionPublishInfo
    {
        public CollectionPublishInfo(Guid id, Guid accountId) {
            Contract.Requires<ArgumentNullException>(id != Guid.Empty);
            Contract.Requires<ArgumentNullException>(accountId != Guid.Empty);
            AccountId = accountId;
            Id = id;
        }

        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
    }
}