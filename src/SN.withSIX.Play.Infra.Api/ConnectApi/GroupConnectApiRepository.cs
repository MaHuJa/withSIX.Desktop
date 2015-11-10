// <copyright company="SIX Networks GmbH" file="GroupConnectApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Infra.Api.ConnectApi
{
    class GroupConnectApiRepository : ConnectApiRepository<Group>
    {
        public GroupConnectApiRepository(IConnectionManager connectionManager, MappingEngine mappingEngine)
            : base(connectionManager, mappingEngine) {}

        public async Task<List<Account>> GetMembers(Guid uuid) {
            var groupMembers = await ConnectionManager.GroupHub.ListGroupMembers(uuid, 1).ConfigureAwait(false);
            return MappingEngine.Map<List<Account>>(groupMembers.Items);
        }
    }
}