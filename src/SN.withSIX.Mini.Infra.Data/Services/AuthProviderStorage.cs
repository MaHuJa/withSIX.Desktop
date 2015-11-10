// <copyright company="SIX Networks GmbH" file="AuthProviderStorage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    // TODO: This is a dummy implementation, implement actual storage!
    public class AuthProviderSettingsStorage : IAuthProviderStorage, IInfrastructureService
    {
        readonly IDictionary<Uri, AuthInfo> _authInfos = new Dictionary<Uri, AuthInfo>();

        public void SetAuthInfo(Uri uri, AuthInfo authInfo) {
            _authInfos[uri] = authInfo;
        }

        public AuthInfo GetAuthInfoFromCache(Uri uri) {
            return _authInfos.ContainsKey(uri) ? _authInfos[uri] : null;
        }
    }
}