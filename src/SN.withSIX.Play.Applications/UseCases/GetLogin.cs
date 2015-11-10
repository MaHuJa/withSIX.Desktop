// <copyright company="SIX Networks GmbH" file="GetLogin.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Play.Applications.Views.Dialogs;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class GetLogin : IRequest<LoginViewModel> {}

    public class GetLoginHandler : IRequestHandler<GetLogin, LoginViewModel>
    {
        readonly IOauthConnect _oauthConnect;

        public GetLoginHandler(IOauthConnect oauthConnect) {
            _oauthConnect = oauthConnect;
        }

        public LoginViewModel Handle(GetLogin request) {
            var loginUri =
                _oauthConnect.GetLoginUri(CommonUrls.AuthorizationEndpoints.AuthorizeEndpoint,
                    CommonUrls.AuthorizationEndpoints.LocalCallbackMini,
                    "openid profile extended_profile premium api roles offline_access",
                    "code",
                    CommonUrls.AuthorizationEndpoints.SyncClientName, "secret");
            return new LoginViewModel(loginUri, CommonUrls.AuthorizationEndpoints.LocalCallbackMini);
        }
    }
}