// <copyright company="SIX Networks GmbH" file="IOauthConnect.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SN.withSIX.Core.Applications.Infrastructure
{
    public interface IOauthConnect
    {
        Uri GetLoginUri(Uri authorizationEndpoint, Uri callbackUri, string scope, string responseType, string clientName,
            string clientSecret);

        IAuthorizeResponse GetResponse(Uri callbackUri, Uri currentUri);

        Task<ITokenResponse> GetAuthorization(Uri tokenEndpoint, Uri callBackUri, string code, string clientId,
            string clientSecret,
            Dictionary<string, string> additionalValues = null);

        Task<ITokenResponse> RefreshToken(Uri tokenEndpoint, string refreshToken, string clientId, string clientSecret,
            Dictionary<string, string> additionalValues = null);

        Task<IUserInfoResponse> GetUserInfo(Uri userInfoEndpoint, string accessToken);
    }

    public enum ResponseTypes
    {
        AuthorizationCode,
        Token,
        FormPost,
        Error
    };

    public interface IAuthorizeResponse
    {
        //        ResponseTypes ResponseType { get; }
        string Raw { get; }
        Dictionary<string, string> Values { get; }
        string Code { get; }
        string AccessToken { get; }
        string IdentityToken { get; }
        string Error { get; }
        long ExpiresIn { get; }
        string Scope { get; }
        string TokenType { get; }
        string State { get; }
    }

    public interface IUserInfoResponse
    {
        string Raw { get; }
        JObject JsonObject { get; }
        IEnumerable<Tuple<string, string>> Claims { get; set; }
        bool IsHttpError { get; }
        HttpStatusCode HttpErrorStatusCode { get; }
        string HttpErrorReason { get; }
        bool IsError { get; }
        string ErrorMessage { get; set; }
    }

    public interface ITokenResponse
    {
        string Raw { get; }
        JObject Json { get; }
        bool IsHttpError { get; }
        HttpStatusCode HttpErrorStatusCode { get; }
        string HttpErrorReason { get; }
        string AccessToken { get; }
        string IdentityToken { get; }
        string Error { get; }
        bool IsError { get; }
        long ExpiresIn { get; }
        string TokenType { get; }
        string RefreshToken { get; }
    }
}