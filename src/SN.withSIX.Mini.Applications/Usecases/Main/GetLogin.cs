// <copyright company="SIX Networks GmbH" file="GetLogin.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Premium;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Login;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Mini.Applications.Usecases.Main
{
    public class GetLogin : IAsyncQuery<ILoginViewModel> {}

    public class GetLoginHandler : IAsyncRequestHandler<GetLogin, ILoginViewModel>
    {
        readonly IOauthConnect _oauthConnect;

        public GetLoginHandler(IOauthConnect oauthConnect) {
            _oauthConnect = oauthConnect;
        }

        public Task<ILoginViewModel> HandleAsync(GetLogin request) {
            var loginUri =
                _oauthConnect.GetLoginUri(CommonUrls.AuthorizationEndpoints.AuthorizeEndpoint,
                    CommonUrls.AuthorizationEndpoints.LocalCallbackMini,
                    "openid profile extended_profile premium api roles offline_access",
                    "code",
                    CommonUrls.AuthorizationEndpoints.SyncClientName, "secret");
            return
                Task.FromResult<ILoginViewModel>(new LoginViewModel(loginUri,
                    CommonUrls.AuthorizationEndpoints.LocalCallbackMini));
        }
    }

    public class ProcessLoginCommand : IRequest<IAuthorizeResponse>
    {
        public ProcessLoginCommand(Uri uri, Uri callbackUri) {
            Uri = uri;
            CallbackUri = callbackUri;
        }

        public Uri Uri { get; }
        public Uri CallbackUri { get; }
    }

    public class ProcessLoginCommandHandler : IRequestHandler<ProcessLoginCommand, IAuthorizeResponse>
    {
        readonly IOauthConnect _connect;

        public ProcessLoginCommandHandler(IOauthConnect connect) {
            _connect = connect;
        }

        public IAuthorizeResponse Handle(ProcessLoginCommand request) {
            return _connect.GetResponse(request.CallbackUri, request.Uri);
        }
    }


    public class PerformAuthentication : IAsyncVoidCommand
    {
        public PerformAuthentication(string code, Uri callbackUri) {
            Code = code;
            CallbackUri = callbackUri;
        }

        public string Code { get; }
        public Uri CallbackUri { get; }
    }

    public class PerformAuthenticationHandler : DbCommandBase, IAsyncRequestHandler<PerformAuthentication, UnitType>
    {
        readonly ITokenRefresher _tokenRefresher;

        public PerformAuthenticationHandler(ITokenRefresher tokenRefresher, IDbContextLocator dbContextLocator)
            : base(dbContextLocator) {
            _tokenRefresher = tokenRefresher;
        }

        public async Task<UnitType> HandleAsync(PerformAuthentication request) {
            await _tokenRefresher.HandleAuthentication(request.Code, request.CallbackUri).ConfigureAwait(false);

            await SettingsContext.SaveSettings().ConfigureAwait(false);
            return UnitType.Default;
        }
    }


    public class LoginChanged : IDomainEvent
    {
        public LoginChanged(LoginInfo loginInfo) {
            LoginInfo = loginInfo;
        }

        public LoginInfo LoginInfo { get; }
    }

    public interface ITokenRefresher
    {
        bool Loaded { get; }
        Task<string> RefreshTokenTask();
        Task HandleAuthentication(string code, Uri callbackUri);
        Task Logout();
    }

    public class TokenUpdatedEvent
    {
        public TokenUpdatedEvent(PremiumAccessTokenV1 premiumToken) {
            PremiumToken = premiumToken;
        }

        public PremiumAccessTokenV1 PremiumToken { get; }
    }

    public class RefreshTokenFailed {}

    public class PremiumEventHandler : IAsyncNotificationHandler<TokenUpdatedEvent>,
        IAsyncNotificationHandler<RefreshTokenFailed>
    {
        readonly IAuthProvider _authProvider;

        public PremiumEventHandler(IAuthProvider authProvider) {
            _authProvider = authProvider;
        }

        public async Task HandleAsync(RefreshTokenFailed notification) {
            await new GetLogin().OpenScreenCached();
        }

        public Task HandleAsync(TokenUpdatedEvent notification) {
#if DEBUG
            if (notification.PremiumToken != null) {
                MainLog.Logger.Debug("Premium UN: " + notification.PremiumToken.AccessToken);
                MainLog.Logger.Debug("Premium Token: " + notification.PremiumToken.PremiumKey);
            } else
                MainLog.Logger.Debug("Not Premium");
#endif
            return notification.PremiumToken == null ? RemovePremium() : AddPremium(notification.PremiumToken);
        }

        async Task RemovePremium() {
            foreach (var endpoint in Common.PremiumHosts)
                _authProvider.SetNonPersistentAuthInfo(("http://" + endpoint).ToUri(), null);
        }

        async Task AddPremium(PremiumAccessTokenV1 premiumToken) {
            //_storage.AccountOptions.SetP(true);
            foreach (var endpoint in Common.PremiumHosts) {
                _authProvider.SetNonPersistentAuthInfo(("http://" + endpoint).ToUri(),
                    new AuthInfo(premiumToken.AccessToken, premiumToken.PremiumKey));
            }
        }
    }
}