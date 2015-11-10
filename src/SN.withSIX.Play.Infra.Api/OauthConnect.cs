// <copyright company="SIX Networks GmbH" file="OauthConnect.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Infra.Services;
using Thinktecture.IdentityModel.Client;

namespace SN.withSIX.Play.Infra.Api
{
    public class OauthConnect : IInfrastructureService, IOauthConnect
    {
        public Uri GetLoginUri(Uri authorizationEndpoint, Uri callbackUri, string scope, string responseType, string clientId, string clientSecret) {
            var client = GetOAuthClient(authorizationEndpoint, clientId, clientSecret);
            //            client.RequestRefreshTokenAsync()
            return new Uri(client.CreateAuthorizeUrl(
                clientId, responseType, scope, callbackUri.ToString(), null, "random_nonce" /**,
                loginHint: "alice",
                acrValues: "idp:Google b c" **/));
        }

        public async Task<ITokenResponse> GetAuthorization(Uri tokenEndpoint, Uri callbackUrl, string code, string clientId, string clientSecret,
            Dictionary<string, string> additionalValues = null) {
            var client = GetOAuthClient(tokenEndpoint, clientId, clientSecret);
            var response =
                await
                    client.RequestAuthorizationCodeAsync(code, callbackUrl.ToString(), additionalValues)
                        .ConfigureAwait(false);
            if (response.IsError) {
                throw new Exception(string.Format("Error while retrieving authorization: {0}. Code: {1}. Reason: {2}",
                    response.Error, response.HttpErrorStatusCode, response.HttpErrorReason));
            }
            return new TokenResponse(response.Raw);
        }

        public async Task<ITokenResponse> RefreshToken(Uri tokenEndpoint, string refreshToken, string clientId, string clientSecret, Dictionary<string, string> additionalValues = null) {
            var client = GetOAuthClient(tokenEndpoint, clientId, clientSecret);
            var response = await client.RequestRefreshTokenAsync(refreshToken, additionalValues).ConfigureAwait(false);
            if (response.IsError) {
                if (response.Error == "invalid_grant")
                    throw new RefreshTokenInvalidException(response.Error);
                throw new Exception(string.Format("Error while refreshing token: {0}. Code: {1}. Reason: {2}",
                    response.Error, response.HttpErrorStatusCode, response.HttpErrorReason));
            }
            return new TokenResponse(response.Raw);
        }

        public async Task<IUserInfoResponse> GetUserInfo(Uri userInfoEndpoint, string accessToken) {
            var userInfoClient = new UserInfoClient(userInfoEndpoint, accessToken);
            var response = await userInfoClient.GetAsync().ConfigureAwait(false);
            if (response.IsError)
                throw new Exception("Error while retrieving userinfo: " + response.ErrorMessage);
            return new UserInfoResponse(response.Raw);
        }

        public IAuthorizeResponse GetResponse(Uri callbackUri, Uri currentUri) {
            if (!currentUri.ToString().StartsWith(callbackUri.AbsoluteUri))
                throw new Exception("Not valid callback uri");
            return new AuthorizeResponse(currentUri.AbsoluteUri);
        }

        static OAuth2Client GetOAuthClient(Uri endpoint, string clientId, string clientSecret) {
            return new OAuth2Client(endpoint, clientId, clientSecret);
        }
    }

    public class RefreshTokenInvalidException : Exception
    {
        public RefreshTokenInvalidException(string error) : base(error) {}
    }

    public class UserInfoResponse : Thinktecture.IdentityModel.Client.UserInfoResponse, IUserInfoResponse
    {
        public UserInfoResponse(string raw) : base(raw) {}
        public UserInfoResponse(HttpStatusCode statusCode, string httpErrorReason) : base(statusCode, httpErrorReason) {}
    }

    public class AuthorizeResponse : Thinktecture.IdentityModel.Client.AuthorizeResponse, IAuthorizeResponse
    {
        public AuthorizeResponse(string raw) : base(raw) {}
    }

    public class TokenResponse : Thinktecture.IdentityModel.Client.TokenResponse, ITokenResponse
    {
        public TokenResponse(string raw) : base(raw) {}
        public TokenResponse(HttpStatusCode statusCode, string reason) : base(statusCode, reason) {}
    }
}