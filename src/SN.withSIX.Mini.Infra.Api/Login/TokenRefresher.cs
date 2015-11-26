// <copyright company="SIX Networks GmbH" file="TokenRefresher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Services.Infra;
using Synercoding.Encryption.Hashing;
using Synercoding.Encryption.Symmetrical;

namespace SN.withSIX.Mini.Infra.Api.Login
{
    // TODO: Don't work with storage directly?
    public class TokenRefresher : IInfrastructureService, ITokenRefresher
    {
        readonly IOauthConnect _connect;
        readonly PremiumHandler _premiumRefresher;
        readonly ISettingsStorage _storage;

        public TokenRefresher(IOauthConnect connect, IMediator mediator, ISettingsStorage storage) {
            _connect = connect;
            _storage = storage;
            _premiumRefresher = new PremiumHandler(mediator, storage);
        }

        public async Task HandleLogin(AccessInfo info) {
            var authenticationInfo = _storage.Settings.Secure.Login.Authentication;
            authenticationInfo.AccessToken = info.AccessToken;
            var localUserInfo = _storage.Settings.Secure.Login;
            if (localUserInfo.Authentication.AccessToken != null)
                await TryHandleLoggedIn(localUserInfo).ConfigureAwait(false);
            else
                await HandleLoggedOut(localUserInfo).ConfigureAwait(false);
            await _storage.SaveSettings().ConfigureAwait(false);
        }

        private async Task TryHandleLoggedIn(LoginInfo localUserInfo) {
            try {
                await HandleLoggedIn(localUserInfo).ConfigureAwait(false);
                // try fetch userinfo. if failed, consider logged out, perhaps ask the website for login again
            } catch (Exception ex) {
                MainLog.Logger.FormattedWarnException(ex, "Failure while processing login info");
                await HandleLoggedOut(localUserInfo).ConfigureAwait(false);
            }
        }

        private async Task HandleLoggedIn(LoginInfo localUserInfo) {
            var userInfo =
                await
                    _connect.GetUserInfo(CommonUrls.AuthorizationEndpoints.UserInfoEndpoint,
                        localUserInfo.Authentication.AccessToken)
                        .ConfigureAwait(false);
            localUserInfo.Account = BuildAccountInfo(userInfo);
            if (localUserInfo.Account.Roles.Contains("premium")) {
                await
                    _premiumRefresher.ProcessPremium(GetClaim(userInfo, CustomClaimTypes.PremiumToken))
                        .ConfigureAwait(false);
            }
        }

        private Task HandleLoggedOut(LoginInfo localUserInfo) {
            localUserInfo.Account = new AccountInfo();
            return _premiumRefresher.Logout();
        }

        static AccountInfo BuildAccountInfo(IUserInfoResponse userInfo) {
            var avatarUrl = GetClaim(userInfo, CustomClaimTypes.AvatarUrl);
            var updatedAt = GetClaim(userInfo, CustomClaimTypes.AvatarUpdatedAt);
            return new AccountInfo {
                Id = Guid.Parse(GetClaim(userInfo, "sub")),
                Roles = userInfo.Claims.Where(x => x.Item1 == "role").Select(x => x.Item2).ToList(),
                DisplayName = GetClaim(userInfo, "nickname"),
                UserName = GetClaim(userInfo, "preferred_username"),
                AvatarURL = avatarUrl,
                HasAvatar = GetClaim(userInfo, CustomClaimTypes.HasAvatar) == "true",
                AvatarUpdatedAt = updatedAt == null ? 0 : long.Parse(updatedAt),
                EmailMd5 = GetClaim(userInfo, CustomClaimTypes.EmailMd5)
            };
        }

        static string GetClaim(IUserInfoResponse userInfo, string claimType) {
            var claim = userInfo.Claims.FirstOrDefault(x => x.Item1 == claimType);
            return claim?.Item2;
        }

        class PremiumHandler
        {
            readonly IMediator _mediator;
            readonly ISettingsStorage _storage;
            bool _firstCompleted;

            public PremiumHandler(IMediator mediator, ISettingsStorage storage) {
                _mediator = mediator;
                _storage = storage;
            }

            public async Task ProcessPremium(string encryptedPremiumToken) {
                // TODO
                //var apiKey = _connectionManager.ApiKey;
                //var apiKey = DomainEvilGlobal.Settings.AppOptions.Id.ToString().Sha256String();
                var apiKey = _storage.Settings.Secure.Login.Account.Id.ToString();
                var premiumToken = await GetPremiumTokenInternal(encryptedPremiumToken, apiKey).ConfigureAwait(false);
                await UpdateToken(premiumToken).ConfigureAwait(false);
                _firstCompleted = true;
            }

            public Task Logout() {
                return UpdateToken(null);
            }

            async Task UpdateToken(PremiumAccessToken newToken) {
                var userInfo = _storage.Settings.Secure.Login;
                var existingToken = userInfo.Authentication.PremiumToken;
                // Always process the first time. Then on consequtive, only update on change
                if (_firstCompleted &&
                    (newToken != null && newToken.Equals(existingToken) || (existingToken == null && newToken == null)))
                    return;
                userInfo.Authentication.PremiumToken = newToken;
                await _mediator.NotifyAsync(new PremiumTokenUpdatedEvent(newToken)).ConfigureAwait(false);
            }

            async Task<PremiumAccessToken> GetPremiumTokenInternal(string encryptedPremiumToken, string apiKey) {
                var premiumToken = _storage.Settings.Secure.Login.Authentication.PremiumToken;
                try {
                    var premiumCached = _firstCompleted && premiumToken.IsPremium() &&
                                        premiumToken.IsValidInNearFuture();
                    if (!premiumCached) {
                        var aes = new Aes();
                        var sha1 = new SHA1Hash();

                        var keyHash = await sha1.GetHashAsync(apiKey).ConfigureAwait(false);
                        var unencryptedPremiumToken =
                            await aes.DecryptAsync(encryptedPremiumToken, keyHash).ConfigureAwait(false);
                        premiumToken = JsonConvert.DeserializeObject<PremiumAccessToken>(unencryptedPremiumToken);
                    }
                } catch (NotPremiumUserException) {
                    premiumToken = null;
                }
                return premiumToken;
            }
        }

        // TODO: Use the one included with Api.Models
        static class CustomClaimTypes
        {
            public const string PremiumToken = "withsix:premium_token";
            public const string EmailMd5 = "withsix:email_md5";
            public const string AvatarUrl = "withsix:avatar_url";
            public const string HasAvatar = "withsix:has_avatar";
            public const string AvatarUpdatedAt = "withsix:avatar_updated_at";
        }
    }
}