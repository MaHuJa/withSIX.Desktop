// <copyright company="SIX Networks GmbH" file="UserSettingsCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Infra.Data.Services
{
    public interface IUserSettingsCacheManager
    {
        IObservable<UserSettings> Get(Version version);
        IObservable<SettingsIndex> GetIndex();
        Task Set(UserSettings settings);
        Task Save();
    }

    public class UserSettingsCacheManager : IInfrastructureService, IUserSettingsCacheManager
    {
        const string IndexKey = SettingsVersion.SettingsPrefix + "_index";
        readonly IUserCache _cache;

        public UserSettingsCacheManager(IUserCache cache) {
            Contract.Requires<ArgumentNullException>(cache != null);
            _cache = cache;
        }

        public IObservable<UserSettings> Get(Version version) {
            Contract.Requires<ArgumentNullException>(version != null);

            return _cache.GetOrFetchObject(GetKey(version), () => TryToFindOlderVersion(version));
        }

        public IObservable<SettingsIndex> GetIndex() {
            return _cache.GetObject<SettingsIndex>(IndexKey);
        }

        public async Task Save() {
            await _cache.Flush();
        }

        public async Task Set(UserSettings settings) {
            Contract.Requires<ArgumentNullException>(settings != null);

            var versions = await GetOrCreateVersionsIndex();
            versions.Register(settings.Version);
            await _cache.InsertObject(GetKey(settings.Version), settings);
            await SetVersions(versions);
        }

        static string GetKey(Version version) {
            return new SettingsVersion(version).SettingsKey();
        }

        async Task<UserSettings> TryToFindOlderVersion(Version version) {
            var userSettings = await FindOlderVersionOrCreateNew().ConfigureAwait(false);
            userSettings.Version = version;
            return userSettings;
        }

        async Task SetVersions(SettingsIndex versions) {
            await _cache.InsertObject(IndexKey, versions);
        }

        async Task<UserSettings> FindOlderVersionOrCreateNew() {
            var index = await GetOrCreateVersionsIndex();
            foreach (var v in index.GetOrdered()) {
                try {
                    return await _cache.GetObject<UserSettings>(v.SettingsKey());
                } catch (KeyNotFoundException) {}
            }

            return new UserSettings();
        }

        IObservable<SettingsIndex> GetOrCreateVersionsIndex() {
            return _cache.GetOrCreateObject(IndexKey, () => new SettingsIndex());
        }
    }

    public class SettingsIndex
    {
        public SettingsIndex() {
            Versions = new List<SettingsVersion>();
        }

        public List<SettingsVersion> Versions { get; set; }

        public IOrderedEnumerable<SettingsVersion> GetOrdered() {
            return Versions.OrderByDescending(x => x.ToVersion());
        }

        public void Register(Version version) {
            if (!HasVersion(version))
                Versions.Add(new SettingsVersion(version));
        }

        bool HasVersion(Version version) {
            return Versions.Any(x => x.Major == version.Major && x.Minor == version.Minor);
        }
    }

    public class SettingsVersion
    {
        public const string SettingsPrefix = "settings";
        public SettingsVersion() {}

        public SettingsVersion(Version version) : this(version.Major, version.Minor) {
            Contract.Requires<ArgumentNullException>(version != null);
        }

        public SettingsVersion(int major, int minor) {
            Contract.Requires<ArgumentNullException>(major >= 0);
            Contract.Requires<ArgumentNullException>(minor >= 0);

            Major = major;
            Minor = minor;
        }

        public int Major { get; set; }
        public int Minor { get; set; }

        public string SettingsKey() {
            return SettingsPrefix + "_" + Major + "." + Minor;
        }

        public Version ToVersion() {
            return new Version(Major, Minor);
        }
    }
}