// <copyright company="SIX Networks GmbH" file="SteamHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NDepend.Path;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Mini.Core.Games
{
    public class SteamStuff
    {
        public KeyValues TryReadSteamConfig() {
            try {
                return ReadSteamConfig();
            } catch (ParseException e) {
                //this.Logger().FormattedWarnException(e, "during steam config parsing");
                return null;
            }
        }

        KeyValues ReadSteamConfig() {
            var steamPath = GetSteamPath();
            if (steamPath == null || !steamPath.Exists)
                return null;

            var steamConfigPath = steamPath.GetChildDirectoryWithName("config").GetChildFileWithName("config.vdf");
            if (steamConfigPath.Exists)
                return new KeyValues(Tools.FileUtil.Ops.ReadTextFileWithRetry(steamConfigPath));

            return null;
        }

        public static IAbsoluteDirectoryPath GetSteamPath() {
            //var steamDirectory = DomainEvilGlobal.Settings.GameOptions.SteamDirectory;
            //return steamDirectory == null ? Common.Paths.SteamPath : steamDirectory.ToAbsoluteDirectoryPath();
            return Common.Paths.SteamPath;
        }
    }

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

        public SteamApp TryGetSteamAppById(int appId, bool noCache = false) {
            if (!SteamFound)
                throw new NotFoundException("Unable to get Steam App, Steam was not found.");
            try {
                return GetSteamAppById(appId, noCache);
            } catch (Exception e) {
                MainLog.Logger.ErrorException("Unknown Exception Attempting to get Steam App", e);
                return SteamApp.Default;
            }
        }

        SteamApp GetSteamAppById(int appId, bool noCache) {
            SteamApp app = null;
            if (noCache || !_appCache.ContainsKey(appId)) {
                app = new SteamApp(appId, GetAppManifestLocation(appId), TryGetConfigByAppId(appId));
                if (app.InstallBase != null)
                    _appCache[appId] = app;
            } else
                app = _appCache[appId];
            return app;
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
        protected SteamApp() {}

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
        public virtual bool IsValid => true;
        public static SteamApp Default { get; } = new NullSteamApp();

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

        class NullSteamApp : SteamApp
        {
            public override bool IsValid => false;
        }
    }

    public class ParseException : Exception
    {
        public ParseException() {}
        public ParseException(string message) : base(message) {}
    }

    // Support for Valve Data Format: https://developer.valvesoftware.com/wiki/KeyValues
    public class KeyValues : Dictionary<string, object>
    {
        public KeyValues() {}
        public KeyValues(IDictionary<string, object> input) : base(input) {}

        public KeyValues(string readAllText) {
            Load(readAllText);
        }

        public string GetString(string key) {
            return (string) this[key];
        }

        public string GetString(IEnumerable<string> keysIn) {
            var keys = keysIn.ToArray();
            var last = keys.Last();
            var entry = this;
            foreach (var k in keys.Take(keys.Length - 1)) {
                var kv = entry.GetKeyValue(k);
                entry = kv;
            }
            return entry.GetString(last);
        }

        public KeyValues GetKeyValue(string key) {
            return (KeyValues) this[key];
        }

        public KeyValues GetKeyValue(IEnumerable<string> keysIn) {
            var keys = keysIn.ToArray();
            var last = keys.Last();
            var entry = this;
            foreach (var k in keys.Take(keys.Length - 1)) {
                var kv = entry.GetKeyValue(k);
                entry = kv;
            }
            return entry.GetKeyValue(last);
        }

        public string PrettyPrint(int level = 0) {
            var sb = new StringBuilder();
            var nextLevel = level + 1;
            var indent = new string('\t', level);
            foreach (var kvp in this) {
                if (kvp.Value is string)
                    sb.AppendFormat("{0}\"{1}\" \"{2}\"\n", indent, kvp.Key, kvp.Value);
                else if (kvp.Value is KeyValues) {
                    var kv = kvp.Value as KeyValues;
                    sb.AppendFormat("{0}\"{1}\"\n{0}{{\n{2}{0}}}\n", indent, kvp.Key, kv.PrettyPrint(nextLevel));
                }
            }

            return sb.ToString();
        }

        public void Load(string data) {
            var tokenizer = new KeyValuesTokenizer(data);
            var token = tokenizer.NextToken();
            if (token == null || token.Item1 != TokenType.String)
                throw new ParseException("Invalid token at " + tokenizer.Location());

            var key = token.Item2;
            token = tokenizer.NextToken();
            if (token == null || token.Item1 != TokenType.BlockBegin) {
                throw new ParseException(String.Format("Invalid token: {0}, {1} at {2}", token.Item1, token.Item2,
                    tokenizer.Location()));
            }

            var kv = new KeyValues();
            this[key] = kv;
            kv.Parse(tokenizer);

            token = tokenizer.NextToken();
            if (token != null)
                throw new ParseException("Unexpected token at file end");
        }

        void Parse(KeyValuesTokenizer tokenizer) {
            string key = null;

            while (true) {
                var token = tokenizer.NextToken();
                if (token == null)
                    throw new ParseException("Unexpected end of file");

                if (key != null) {
                    if (token.Item1 == TokenType.BlockBegin) {
                        var value = new KeyValues();
                        value.Parse(tokenizer);
                        this[key] = value;
                    } else if (token.Item1 == TokenType.String)
                        this[key] = token.Item2;
                    else
                        throw new ParseException("Invalid token at " + tokenizer.Location());
                    key = null;
                } else {
                    if (token.Item1 == TokenType.BlockEnd)
                        break;
                    if (token.Item1 != TokenType.String)
                        throw new ParseException("Invalid token at " + tokenizer.Location());
                    key = token.Item2;
                }
            }
        }
    }

    public enum TokenType
    {
        BlockBegin,
        BlockEnd,
        String
    }

    public class KeyValuesTokenizer
    {
        static readonly char[] braces = {'{', '}'};
        static readonly char[] ws = {' ', '\n', '\t'};
        readonly string _data;
        long _line = 1;
        int _position;
        protected int LastLineBreak;

        public KeyValuesTokenizer(string data) {
            _data = data;
        }

        public Tuple<TokenType, string> NextToken() {
            while (true) {
                IgnoreWhitespace();
                if (!IgnoreComment())
                    break;
            }

            var current = Current();
            if (current == default(char))
                return null;

            if (current == '{') {
                Forward();
                return Tuple.Create(TokenType.BlockBegin, (string) null);
            }
            if (current == '}') {
                Forward();
                return Tuple.Create(TokenType.BlockEnd, (string) null);
            }
            return Tuple.Create(TokenType.String, GetString());
        }

        string GetString() {
            var escape = false;
            var r = String.Empty;
            var quoted = false;
            var current = Current();
            if (current == '\"') {
                quoted = true;
                Forward();
            }
            while (true) {
                current = Current();
                if (current == default(char))
                    break;

                if (!quoted && braces.Contains(current))
                    break;

                if (!escape && quoted && current == '\"')
                    break;

                if (escape) {
                    escape = false;
                    if (current == '\"')
                        r += "\"";
                    else if (current == '\\')
                        r += "\\";
                } else if (current == '\\')
                    escape = true;
                else
                    r += current;
                Forward();
            }

            if (quoted)
                Forward();

            return r;
        }

        bool IgnoreComment() {
            var current = Current();
            var next = Next();

            // Skip // comments - TODO: What about storing them??
            if (current == '/' && next == '/') {
                while (Current() != '\n')
                    Forward();
                return true;
            }

            // Skip /* comments */ - TODO: What about storing them?
            // Actually, these aren't supported in the original format
            if (current == '/' && next == '*') {
                while (current != default(char)
                       && (current != '*' && next != '/')) {
                    Forward();
                    current = Current();
                    next = Next();
                    if (current == '\n') {
                        LastLineBreak = _position;
                        _line++;
                    }
                }
                Forward(2); // Move past the */
                return true;
            }

            return false;
        }

        void IgnoreWhitespace() {
            var current = Current();
            while (current != default(char)) {
                if (current == '\n') {
                    LastLineBreak = _position;
                    _line++;
                } else if (!ws.Contains(current))
                    return;
                Forward();
                current = Current();
            }
        }

        bool Forward(int count = 1) {
            return (_position += count) < _data.Length;
        }

        public string Location() {
            return string.Format("line {0}, column {1}", _line, _position - LastLineBreak);
        }

        char Next() {
            var pos = _position + 1;
            if (pos > _data.Length)
                return default(char);
            return _data[pos];
        }

        char Current() {
            if (_position >= _data.Length)
                return default(char);

            return _data[_position];
        }
    }
}