// <copyright company="SIX Networks GmbH" file="GetAuthorization.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class GetAuthorization : IAsyncRequest<UnitType>
    {
        public GetAuthorization(string code, Uri callbackUri) {
            Code = code;
            CallbackUri = callbackUri;
        }

        public string Code { get; }
        public Uri CallbackUri { get; set; }
    }

    public class GetAuthorizationHandler : IAsyncRequestHandler<GetAuthorization, UnitType>
    {
        readonly IConnectApiHandler _apiHandler;

        public GetAuthorizationHandler(IConnectApiHandler apiHandler) {
            _apiHandler = apiHandler;
        }

        public Task<UnitType> HandleAsync(GetAuthorization request) {
            return _apiHandler.HandleAuthentication(request.Code, request.CallbackUri).Void();
        }
    }
}