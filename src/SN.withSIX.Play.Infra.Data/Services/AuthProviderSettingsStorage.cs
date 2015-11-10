// <copyright company="SIX Networks GmbH" file="AuthProviderSettingsStorage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Infra.Data.Services
{
    public class AuthProviderSettingsStorage : IAuthProviderStorage, IInfrastructureService
    {
        readonly UserSettings _settings;

        public AuthProviderSettingsStorage(UserSettings settings) {
            _settings = settings;
        }

        public void SetAuthInfo(Uri uri, AuthInfo authInfo) {
            _settings.AppOptions.SetAuthInfo(uri, authInfo);
        }

        public AuthInfo GetAuthInfoFromCache(Uri uri) {
            return _settings.AppOptions.GetAuthInfoFromCache(uri);
        }
    }
}