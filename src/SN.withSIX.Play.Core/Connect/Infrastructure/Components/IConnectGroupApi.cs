// <copyright company="SIX Networks GmbH" file="IConnectGroupApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;

namespace SN.withSIX.Play.Core.Connect.Infrastructure.Components
{
    public interface IConnectGroupApi
    {
        Task<Guid> CreateGroup(ICreateGroupInfo inputModel, IAbsoluteFilePath logoFilename,
            IAbsoluteFilePath backgroundFilename);

        Task UploadGroupLogoPicture(IAbsoluteFilePath logoFileName, Guid groupId);
        Task UploadGroupBackgroundPicture(IAbsoluteFilePath backgroundFileName, Guid groupId);
        Task<Group> GetGroup(Guid groupId);
        Task LeaveGroup(Group group);
        Task DeleteGroup(Group group);
        Task AddUserToGroup(Account user, Group group);
        Task RemoveUserFromGroup(Account user, Group group);
        Task Refresh(Group group);
    }
}