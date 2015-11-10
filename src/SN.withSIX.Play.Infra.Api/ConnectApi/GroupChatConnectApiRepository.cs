﻿// <copyright company="SIX Networks GmbH" file="GroupChatConnectApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using AutoMapper;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Infra.Api.ConnectApi
{
    class GroupChatConnectApiRepository : ConnectApiRepository<GroupChat>
    {
        public GroupChatConnectApiRepository(IConnectionManager connectionManager, MappingEngine mappingEngine)
            : base(connectionManager, mappingEngine) {}
    }
}