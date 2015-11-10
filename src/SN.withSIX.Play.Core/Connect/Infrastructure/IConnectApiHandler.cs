// <copyright company="SIX Networks GmbH" file="IConnectApiHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Api.Models;
using SN.withSIX.Play.Core.Connect.Infrastructure.Components;

namespace SN.withSIX.Play.Core.Connect.Infrastructure
{
    public interface ICreateGroupInfo
    {
        string Name { get; }
        string Description { get; }
        Uri Homepage { get; }
    }

    public interface IConnectApiHandler : IConnectMissionsApi, IConnectCollectionsApi, IConnectGroupApi,
        IConnectAccountApi, IConnectChatApi
    {
        MyAccount Me { get; }
        IMessageBus MessageBus { get; }
        Task SetServerAddress(ServerAddress serverAddress);
        Task Initialize(string key);
        void ConfirmLoggedIn();
        void ConfirmConnected();
        Task<List<ChatMessage>> GetChatMessages(GroupChat groupChat);
        Task<List<ChatMessage>> GetChatMessages(PublicChat publicChat);
        Task<List<PrivateMessage>> GetPrivateChatMessages(Account user);
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