// <copyright company="SIX Networks GmbH" file="PublicChatConnectApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using AutoMapper;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Infra.Api.ConnectApi
{
    class PublicChatConnectApiRepository : ConnectApiRepository<PublicChat>
    {
        public PublicChatConnectApiRepository(IConnectionManager connectionManager, MappingEngine mappingEngine)
            : base(connectionManager, mappingEngine) {}
    }
}