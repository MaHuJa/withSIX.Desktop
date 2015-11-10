// <copyright company="SIX Networks GmbH" file="IAccountHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SignalRNetClientProxyMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Api.Models.Social;
using SN.withSIX.Api.Models.Statistics.PlayedServers;

namespace SN.withSIX.Play.Infra.Api.Hubs
{
    [DoNotObfuscateType]
    interface IAccountHub : IClientHubProxyBase
    {
        Task<ContextModel> GetContext();
        Task<AccountModel> GetAccount(Guid accountGuid);
        Task<ReadOnlyCollection<FriendshipModel>> GetFriends(Guid accountUuid);
        Task DeleteFriend(Guid friendGuid);
        Task<ReadOnlyCollection<FriendshipRequestModel>> GetFriendshipRequests();
        Task<FriendshipRequestModel> RequestFriendship(Guid friendGuid);
        Task<FriendshipModel> AcceptFriendship(Guid friendGuid);
        Task DeclineFriendship(Guid friendGuid);
        Task<PageModel<AccountModel>> SearchUsers(AccountSearchInputModel searchInput);
        Task<Guid> CreateServerAddressSession(StartSessionInput session);
        Task UpdateServerAddressSession(Guid guid);
        Task Heartbeat();
        IDisposable OnlineStatusChanged(Action<AccountStatusChanged> action);
        IDisposable FriendshipAccepted(Action<FriendshipRequestAccepted> action);
        IDisposable FriendshipRequestReceived(Action<FriendshipRequestReceived> action);
        IDisposable FriendshipDeclined(Action<FriendshipRequestDeclined> action);
        IDisposable FriendshipRequestCancelled(Action<FriendshipRequestCancelled> action);
        IDisposable FriendshipDeleted(Action<FriendshipDeleted> action);
    }
}