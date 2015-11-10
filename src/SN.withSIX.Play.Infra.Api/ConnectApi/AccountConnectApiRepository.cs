// <copyright company="SIX Networks GmbH" file="AccountConnectApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using AutoMapper;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Infra.Api.ConnectApi
{
    class AccountConnectApiRepository : ConnectApiRepository<Account>
    {
        public AccountConnectApiRepository(IConnectionManager connectionManager, MappingEngine mappingEngine)
            : base(connectionManager, mappingEngine) {}
    }
}