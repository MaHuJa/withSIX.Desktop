// <copyright company="SIX Networks GmbH" file="IConnectAccountApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SN.withSIX.Play.Core.Connect.Infrastructure.Components
{
    public interface IConnectAccountApi
    {
        Task<Account> GetAccount(Guid guid);
        Task<IReadOnlyCollection<Account>> SearchUsers(string search, int page = 1);
        Task<InviteRequest> AddFriendshipRequest(Account user);
        Task RemoveFriend(Account user);
        Task<Friend> ApproveFriend(InviteRequest request);
        Task DeclineFriend(InviteRequest request);
        Task HandleAuthentication(string code, Uri localCallback);
    }
}