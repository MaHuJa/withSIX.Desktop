// <copyright company="SIX Networks GmbH" file="AuthProvider.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Core.Infra.Services
{
    // TODO: Convert async??
    public interface IAuthProviderStorage
    {
        void SetAuthInfo(Uri uri, AuthInfo authInfo);
        AuthInfo GetAuthInfoFromCache(Uri uri);
    }

    public class AuthProvider : IAuthProvider, IInfrastructureService
    {
        static readonly IDictionary<string, string> protocolMappings = new Dictionary<string, string> {
            {"zsync", "http"},
            {"zsyncs", "https"}
        };
        readonly ConcurrentDictionary<string, AuthInfo> _nonPersistentAuthCache =
            new ConcurrentDictionary<string, AuthInfo>();
        readonly IAuthProviderStorage _storage;

        public AuthProvider(IAuthProviderStorage storage) {
            _storage = storage;
        }

        public AuthInfo GetAuthInfoFromUriWithCache(Uri uri) {
            var authInfo = GetAuthInfoFromUri(uri);
            if (authInfo.Username == null && authInfo.Password == null)
                authInfo = GetAuthInfo(uri);

            return authInfo;
        }

        public void SetNonPersistentAuthInfo(Uri uri, AuthInfo authInfo) {
            var key = String.Format("{0}://{1}:{2}", uri.Scheme, uri.Host, uri.Port);

            if (authInfo == null
                || authInfo.Username == null && authInfo.Password == null && authInfo.Domain == null) {
                AuthInfo val;
                _nonPersistentAuthCache.TryRemove(key, out val);
            } else
                _nonPersistentAuthCache.AddOrUpdate(key, authInfo, (s, info) => authInfo);
        }

        public Uri HandleUriAuth(Uri uri, string username = null, string password = null) {
            if (username == null && password == null)
                return uri.AuthlessUri();
            var ub = BuildUri(uri);
            if (username != null)
                ub.UserName = username;
            if (password != null)
                ub.Password = password;
            return
                ub.Uri;
        }

        public Uri HandleUri(Uri uri) {
            var authInfo = GetAuthInfoFromUriWithCache(uri);
            return HandleUriAuth(uri, authInfo.Username, authInfo.Password);
        }

        public AuthInfo GetAuthInfoFromUri(Uri uri) {
            var uriHasAuthInfo = !string.IsNullOrWhiteSpace(uri.UserInfo);

            if (uriHasAuthInfo) {
                var userInfo = uri.UserInfo.Split(':');
                return new AuthInfo(userInfo.Length > 0 && !string.IsNullOrWhiteSpace(userInfo[0]) ? userInfo[0] : null,
                    userInfo.Length > 1 && !string.IsNullOrWhiteSpace(userInfo[1]) ? userInfo[1] : null);
            }

            return new AuthInfo(null, null);
        }

        public void HandleAuthInfo(Uri uri, IWebClient client) {
            var authInfo = GetAuthInfoFromUriWithCache(uri);

            if (authInfo.Username == null && authInfo.Password == null) {
                client.Credentials = null;
                return;
            }

            _storage.SetAuthInfo(uri, authInfo);

            client.Credentials = new NetworkCredential(authInfo.Username, authInfo.Password);
        }

        static string GetAuthInfoKey(Uri uri) {
            var scheme = protocolMappings.ContainsKey(uri.Scheme) ? protocolMappings[uri.Scheme] : uri.Scheme;
            var port = scheme == "http" && uri.Port == -1 ? 80 : (scheme == "https" && uri.Port == -1 ? 443 : uri.Port);
            return String.Format("{0}://{1}:{2}", scheme, uri.Host, port);
        }

        AuthInfo GetAuthInfo(Uri uri) {
            AuthInfo val;
            if (_nonPersistentAuthCache.TryGetValue(GetAuthInfoKey(uri), out val))
                return val;

            return _storage.GetAuthInfoFromCache(uri) ?? new AuthInfo(null, null);
        }

        static UriBuilder BuildUri(Uri uri) {
            return new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, uri.Query);
        }
    }
}