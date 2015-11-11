// <copyright company="SIX Networks GmbH" file="IConnectApiHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Extensions;
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
        Task<ConnectionScoper> StartSession();
    }


    public interface IConnectionScoper
    {
        Task Stop();
        Task Start(string key = null);

    }


    public class ConnectionScoper : IDisposable
    {
        readonly IConnectionScoper _connManager;
        volatile bool _closed;

        static volatile bool _inUse;

        public ConnectionScoper(IConnectionScoper connManager)
        {
            _connManager = connManager;
            if (_inUse)
                throw new Exception("Connection already in use!");
            _inUse = true;
        }

        public async Task Close()
        {
            await _connManager.Stop().ConfigureAwait(false);
            _closed = true;
        }
        public void Dispose()
        {
            //if (!_closed)
                //throw new Exception("The connection was not closed before disposal!");
            try {
                if (!_closed)
                    Close().WaitAndUnwrapException();
            } finally {
            _inUse = false;
            }
        }
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