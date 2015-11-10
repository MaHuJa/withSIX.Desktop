// <copyright company="SIX Networks GmbH" file="TokenRefresher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Options;
using Synercoding.Encryption.Hashing;
using Synercoding.Encryption.Symmetrical;

namespace SN.withSIX.Play.Infra.Api
{
    public interface ITokenRefresher
    {
        bool Loaded { get; }
        Task<string> RefreshTokenTask();
        Task HandleAuthentication(string code, Uri localCallback);
        Task Logout();
    }

    public class TokenRefresher : ITokenRefresher, IInfrastructureService
    {
        readonly IOauthConnect _connect;
        readonly PremiumHandler _premiumRefresher;
        readonly object _refreshLock = new object();
        readonly SecretData _secretData = DomainEvilGlobal.SecretData;
        readonly UserSettings _settings = DomainEvilGlobal.Settings;
        Task<string> _refreshTask;

        public TokenRefresher(IOauthConnect connect, IMediator mediator) {
            _connect = connect;
            _premiumRefresher = new PremiumHandler(mediator);
        }

        public bool Loaded { get; private set; }

        public Task<string> RefreshTokenTask() {
            lock (_refreshLock)
                return _refreshTask == null || _refreshTask.IsCompleted
                    ? (_refreshTask = RefreshTokenInternal())
                    : _refreshTask;
        }

        public async Task HandleAuthentication(string code, Uri localCallback) {
            var authorizationResponse =
                await
                    _connect.GetAuthorization(CommonUrls.AuthorizationEndpoints.TokenEndpoint,
                        localCallback, code, CommonUrls.AuthorizationEndpoints.SyncClientName, "secret",
                        GetAdditionalValues())
                        .ConfigureAwait(false);

            await HandleAccessToken(authorizationResponse).ConfigureAwait(false);
            await UpdateUserInfo().ConfigureAwait(false);
        }

        public Task Logout() {
            return _premiumRefresher.Logout();
        }

        static Dictionary<string, string> GetAdditionalValues() {
            return new Dictionary<string, string> {
                {Common.ClientHeader, DomainEvilGlobal.SecretData.UserInfo.ClientId.ToString()}
            };
        }

        async Task<string> RefreshTokenInternal() {
#if DEBUG
            MainLog.Logger.Debug("Refreshing token...");
#endif
            var newInfo =
                await
                    _connect.RefreshToken(CommonUrls.AuthorizationEndpoints.TokenEndpoint,
                        _secretData.UserInfo.RefreshToken, CommonUrls.AuthorizationEndpoints.SyncClientName, "secret").ConfigureAwait(false);

            _secretData.UserInfo.RefreshToken = newInfo.RefreshToken;
            _secretData.UserInfo.AccessToken = newInfo.AccessToken;
            await _secretData.Save().ConfigureAwait(false);

            await UpdateUserInfo().ConfigureAwait(false);

            Loaded = true;

            return newInfo.AccessToken;
        }

        Task HandleAccessToken(ITokenResponse authorizationResponse) {
            _secretData.UserInfo.RefreshToken = authorizationResponse.RefreshToken;
            _settings.AccountOptions.AccessToken = authorizationResponse.AccessToken;
            return _secretData.Save();
        }

        async Task UpdateUserInfo() {
            var localUserInfo = _secretData.UserInfo;
            var userInfo =
                await
                    _connect.GetUserInfo(CommonUrls.AuthorizationEndpoints.UserInfoEndpoint, localUserInfo.AccessToken)
                        .ConfigureAwait(false);
            _secretData.UserInfo.Account = BuildAccountInfo(userInfo);
            if (localUserInfo.Account.Roles.Contains("premium")) {
                await
                    _premiumRefresher.ProcessPremium(GetClaim(userInfo, CustomClaimTypes.PremiumToken))
                        .ConfigureAwait(false);
            }
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
            return claim == null ? null : claim.Item2;
        }

        class PremiumHandler
        {
            readonly IMediator _mediator;
            readonly SecretData _secretData;
            readonly UserSettings _settings;
            bool _firstCompleted;

            public PremiumHandler(IMediator mediator) {
                _mediator = mediator;
                _secretData = DomainEvilGlobal.SecretData;
                _settings = DomainEvilGlobal.Settings;
            }

            public async Task ProcessPremium(string encryptedPremiumToken) {
                // TODO
                //var apiKey = _connectionManager.ApiKey;
                //var apiKey = DomainEvilGlobal.Settings.AppOptions.Id.ToString().Sha256String();
                var apiKey = _secretData.UserInfo.Account.Id.ToString();
                var premiumToken = await GetPremiumTokenInternal(encryptedPremiumToken, apiKey).ConfigureAwait(false);
                await UpdateToken(premiumToken).ConfigureAwait(false);
                _firstCompleted = true;
            }

            public Task Logout() {
                return UpdateToken(null);
            }

            async Task UpdateToken(PremiumAccessToken newToken) {
                var userInfo = _secretData.UserInfo;
                var existingToken = userInfo.Token;
                // Always process the first time. Then on consequtive, only update on change
                if (_firstCompleted &&
                    (newToken != null && newToken.Equals(existingToken) || (existingToken == null && newToken == null)))
                    return;
                userInfo.Token = newToken;
                await _mediator.NotifyAsync(new TokenUpdatedEvent(newToken)).ConfigureAwait(false);
            }

            async Task<PremiumAccessToken> GetPremiumTokenInternal(string encryptedPremiumToken, string apiKey) {
                var premiumToken = _settings.AccountOptions.UserInfo.Token;
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