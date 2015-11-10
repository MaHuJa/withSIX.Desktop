// <copyright company="SIX Networks GmbH" file="SettingsStorage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    // Singleton for now..
    public class SettingsStorage : IInfrastructureService, ISettingsStorage
    {
        readonly ILocalCache _localCache;
        readonly IUserCache _roamingCache;
        readonly ISecureCache _roamingSecureCache;
        readonly Lazy<Settings> _settings;

        public SettingsStorage(ILocalCache localCache, ISecureCache roamingSecureCache, IUserCache roamingCache) {
            _localCache = localCache;
            _roamingSecureCache = roamingSecureCache;
            _roamingCache = roamingCache;
            _settings = new Lazy<Settings>(() => Task.Run(() => LoadSettings()).Result);
        }

        public Settings Settings => _settings.Value;

        public async Task SaveSettings() {
            await _localCache.InsertObject("localSettings", Settings.Local);
            await _roamingCache.InsertObject("roamingSettings", Settings.Roaming);
            await _roamingSecureCache.InsertObject("secureSettings", Settings.Secure);
        }

        async Task<Settings> LoadSettings() {
            return new Settings {
                Local = await _localCache.GetOrCreateObject("localSettings", () => new LocalSettings()),
                Roaming = await _roamingCache.GetOrCreateObject("roamingSettings", () => new RoamingSettings()),
                Secure = await _roamingSecureCache.GetOrCreateObject("secureSettings", () => new SecureSettings())
            };
        }
    }
}