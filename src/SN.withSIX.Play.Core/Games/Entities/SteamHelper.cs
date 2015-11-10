// <copyright company="SIX Networks GmbH" file="SteamHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Legacy.Steam;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public class SteamHelper
    {
        readonly Dictionary<int, SteamApp> _appCache;
        readonly IEnumerable<IAbsoluteDirectoryPath> _baseInstallPaths;
        readonly bool _cache;
        readonly IAbsoluteDirectoryPath _steamPath;

        public SteamHelper(KeyValues steamConfig, IAbsoluteDirectoryPath steamPath, bool cache = true) {
            _cache = cache;
            _appCache = new Dictionary<int, SteamApp>();
            KeyValues = steamConfig;
            _steamPath = steamPath;
            SteamFound = KeyValues != null || (_steamPath != null && _steamPath.Exists);
            _baseInstallPaths = GetBaseInstallFolderPaths();
        }

        public KeyValues KeyValues { get; }
        public bool SteamFound { get; }

        KeyValues TryGetConfigByAppId(int appId) {
            KeyValues apps = null;
            try {
                apps = KeyValues.GetKeyValue(new[] {"InstallConfigStore", "Software", "Valve", "Steam", "apps"});
            } catch (KeyNotFoundException ex) {
                MainLog.Logger.ErrorException("Config Store Invalid", ex);
                return null;
            }
            try {
                return apps.GetKeyValue(appId.ToString());
            } catch (Exception) {
                return null;
            }
        }

        public SteamApp GetSteamAppById(int appId, bool noCache = false) {
            if (!SteamFound)
                throw new NotFoundException("Unable to get Steam App, Steam was not found.");
            try {
                SteamApp app = null;
                if (noCache || !_appCache.ContainsKey(appId)) {
                    app = new SteamApp(appId, GetAppManifestLocation(appId), TryGetConfigByAppId(appId));
                    if (app.InstallBase != null)
                        _appCache[appId] = app;
                } else
                    app = _appCache[appId];
                return app;
            } catch (Exception e) {
                MainLog.Logger.ErrorException("Unknown Exception Attempting to get Steam App", e);
                return new SteamApp(appId, null, null);
            }
        }

        IAbsoluteDirectoryPath GetAppManifestLocation(int appId) {
            return _baseInstallPaths.FirstOrDefault(installPath => CheckForAppManifest(appId, installPath));
        }

        bool CheckForAppManifest(int appId, IAbsoluteDirectoryPath installPath) {
            return
                installPath.GetChildDirectoryWithName("SteamApps")
                    .GetChildFileWithName("appmanifest_" + appId + ".acf")
                    .Exists;
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        IReadOnlyList<IAbsoluteDirectoryPath> GetBaseInstallFolderPaths() {
            var list = new List<IAbsoluteDirectoryPath>();
            list.Add(_steamPath);
            if (KeyValues == null)
                return list.AsReadOnly();
            try {
                var kv = KeyValues.GetKeyValue(new[] {"InstallConfigStore", "Software", "Valve", "Steam"});
                var iFolder = 1;
                while (kv.ContainsKey("BaseInstallFolder_" + iFolder)) {
                    list.Add(kv.GetString("BaseInstallFolder_" + iFolder).ToAbsoluteDirectoryPath());
                    iFolder++;
                }
            } catch (KeyNotFoundException ex) {
                MainLog.Logger.ErrorException("Config Store Invalid", ex);
            }
            return list.AsReadOnly();
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SteamApp
    {
        public SteamApp(int appId, IAbsoluteDirectoryPath installBase, KeyValues appConfig) {
            AppId = appId;
            InstallBase = installBase;
            AppConfig = appConfig;
            if (installBase != null)
                LoadManifest();
            SetAppPath();
        }

        public int AppId { get; }
        public IAbsoluteDirectoryPath InstallBase { get; }
        public IAbsoluteDirectoryPath AppPath { get; private set; }
        public KeyValues AppManifest { get; private set; }
        public KeyValues AppConfig { get; }

        void SetAppPath() {
            if (AppConfig != null && AppConfig.ContainsKey("installdir")) {
                AppPath =
                    InstallBase.GetChildDirectoryWithName(@"SteamApps\Common\" +
                                                          AppConfig.GetString("installdir"));
            } else if (AppManifest != null) {
                try {
                    AppPath =
                        InstallBase.GetChildDirectoryWithName(@"SteamApps\Common\" +
                                                              AppManifest.GetString(new[] {"AppState", "installdir"}));
                } catch (KeyNotFoundException ex) {
                    MainLog.Logger.ErrorException("AppManifest Invalid ({0})".FormatWith(AppId), ex);
                }
            }
        }

        void LoadManifest() {
            var configPath =
                InstallBase.GetChildDirectoryWithName("SteamApps").GetChildFileWithName("appmanifest_" + AppId + ".acf");
            AppManifest = new KeyValues(Tools.FileUtil.Ops.ReadTextFileWithRetry(configPath));
        }
    }
}