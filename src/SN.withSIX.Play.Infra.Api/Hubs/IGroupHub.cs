// <copyright company="SIX Networks GmbH" file="IGroupHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SignalRNetClientProxyMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Social;

namespace SN.withSIX.Play.Infra.Api.Hubs
{
    [DoNotObfuscateType]
    interface IGroupHub : IClientHubProxyBase
    {
        Task<ReadOnlyCollection<GroupModel>> GetUserGroups();
        Task<PageModel<GroupModel>> ListGroups(int page);
        Task<GroupModel> GetGroup(Guid groupUuid);
        Task<Guid> CreateGroup(CreateGroupInputModel model);
        Task DeleteGroup(Guid groupUuid);
        Task LeaveGroup(Guid groupUuid);
        Task<PageModel<AccountModel>> ListGroupMembers(Guid groupUuid, int page);
        Task AddUserToGroup(Guid groupUuid, Guid userUuid);
        Task RemoveUserFromGroup(Guid groupUuid, Guid userUuid);

        /// <summary>
        ///     Upload a group background picture
        /// </summary>
        /// <param name="groupGuid">The ID of the group</param>
        /// <param name="part">A Base64 String for the picture of max size 16,000 per part</param>
        /// <param name="partNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task UploadGroupBackgroundPicture(Guid token, Guid groupGuid, string part, Tuple<int, int> partNumber);

        /// <summary>
        ///     Upload a group logo picture
        /// </summary>
        /// <param name="groupGuid">The ID of the group</param>
        /// <param name="part">A Base64 String for the picture of max size 16,000 per part</param>
        /// <param name="partNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task UploadGroupLogoPicture(Guid token, Guid groupGuid, string part, Tuple<int, int> partNumber);

        IDisposable AddedToGroup(Action<AddedToGroup> action);
        IDisposable UserAddedToGroup(Action<UserAddedToGroup> action);
        IDisposable UserRemovedFromGroup(Action<UserRemovedFromGroup> action);
        IDisposable GroupUpdated(Action<GroupUpdated> action);
    }
}