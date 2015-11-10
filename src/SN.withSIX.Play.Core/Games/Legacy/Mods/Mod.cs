// <copyright company="SIX Networks GmbH" file="Mod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NDepend.Path;
using Newtonsoft.Json;
using SmartAssembly.Attributes;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Validators;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Sync.Core.Legacy.SixSync;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DoNotObfuscate]
    public class Mod : HostedContent, IMod, IComparePK<Mod>, IEnableLogging
    {
        static readonly Uri modUrl2 = Tools.Transfer.JoinUri(CommonUrls.SuUrl, "api/v1/mods");
        static readonly ArmaModPathValidator modPathValidator = new ArmaModPathValidator();
        static readonly string[] acreMods = {"@ACRE2", "@ACRE", "@ACRE_A3"};
        string[] _aliases = new string[0];
        string _cppName;
        string[] _dependencies = new string[0];
        string _displayName;
        string _fullName;
        string _guid;
        bool _hasLicense;
        bool? _isFavorite;
        bool _isInCurrentCollection;
        Uri[] _mirrors;
        string _modVersion;
        [DataMember] string _Revision;
        long _size;
        long _sizeWd;
        GameModType _type;
        DateTime _updatedVersion;
        Userconfig _userConfig;

        public Mod(Guid id)
            : base(id) {
            Controller = new ModController(this);
            Categories = new string[0];
            Mirrors = new Uri[0];
            Networks = new List<Network>();
        }

        public bool ComparePK(Mod other) {
            return other != null && ComparePK((IMod) other);
        }

        public override sealed string Name
        {
            get { return base.Name; }
            set
            {
                if (IsDeprecatedOaBeta(value))
                    value = "beta_oa";
                FileNameValidator.ValidateName(value);
                base.Name = value;
            }
        }
        public bool IsInCurrentCollection
        {
            get { return _isInCurrentCollection; }
            set { SetProperty(ref _isInCurrentCollection, value); }
        }

        public void LoadSettings(ISupportModding game) {
            if (Name.Equals("@ace", StringComparison.InvariantCultureIgnoreCase)) {
                var aceUserconfig = new AceUserconfig(game);
                aceUserconfig.ReadAceClientSideConfig();
                UserConfig = aceUserconfig;
            }
        }

        public Userconfig UserConfig
        {
            get { return _userConfig; }
            private set { SetProperty(ref _userConfig, value); }
        }

        public bool RequiresAdminRights() {
            return acreMods.Contains(Name, StringComparer.InvariantCultureIgnoreCase);
        }

        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(FullName) ? Name : (FullName + " - " + Name); }
        }
        public string Revision
        {
            get { return _Revision; }
            set { SetProperty(ref _Revision, value); }
        }

        public Guid[] GetGameRequirements() {
            var name = Name;
            if (string.IsNullOrWhiteSpace(name))
                return new Guid[0];

            if (name.Equals("@allinarma", StringComparison.InvariantCultureIgnoreCase)) {
                return new[] {
                    GameUuids.Arma1,
                    GameUuids.Arma2,
                    GameUuids.Arma2Oa,
                    GameUuids.TKOH
                };
            }
            if (name.Equals("Rearmed", StringComparison.InvariantCultureIgnoreCase)) {
                return new[] {
                    GameUuids.Arma2,
                    GameUuids.Arma2Oa
                };
            }

            return new Guid[0];
        }

        public override bool IsFavorite
        {
            get
            {
                return _isFavorite == null
                    ? (bool) (_isFavorite = DomainEvilGlobal.Settings.ModOptions.IsFavorite(this))
                    : (bool) _isFavorite;
            }
            set
            {
                if (_isFavorite == value)
                    return;
                if (value)
                    DomainEvilGlobal.Settings.ModOptions.AddFavorite(this);
                else
                    DomainEvilGlobal.Settings.ModOptions.RemoveFavorite(this);
                _isFavorite = value;
                OnPropertyChanged();
            }
        }
        public string UserConfigChecksum
        {
            get
            {
                return DomainEvilGlobal.Settings.ModOptions.UserConfigChecksums.ContainsKey(Name)
                    ? DomainEvilGlobal.Settings.ModOptions.UserConfigChecksums[Name]
                    : null;
            }
            set
            {
                if (DomainEvilGlobal.Settings.ModOptions.UserConfigChecksums.ContainsKey(Name))
                    DomainEvilGlobal.Settings.ModOptions.UserConfigChecksums[Name] = value;
                else
                    DomainEvilGlobal.Settings.ModOptions.UserConfigChecksums.Add(Name, value);
                OnPropertyChanged();
            }
        }
        public ModController Controller { get; }

        public int GetMaxThreads() {
            var network = Networks.FirstOrDefault();
            return network == null ? 0 : network.MaxThreads.GetValueOrDefault(0);
        }

        public DateTime UpdatedVersion
        {
            get { return _updatedVersion; }
            set { SetProperty(ref _updatedVersion, value); }
        }
        public override string Notes { get; set; }
        public override bool HasNotes
        {
            get { throw new NotImplementedException(); }
        }

        public bool ComparePK(IMod other) {
            if (other == null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            var lm = other as LocalMod;
            var lm2 = this as LocalMod;
            if (lm != null || lm2 != null) {
                if (other.Name == default(string) || Name == default(string))
                    return false;
                return other.Name == Name;
            }
            if (other.Id == System.Guid.Empty || Id == System.Guid.Empty)
                return false;

            return other.Id == Id;
        }

        public IAbsoluteDirectoryPath CustomPath { get; set; }

        public IEnumerable<IAbsolutePath> GetPaths() {
            return ModFolders(Controller.Path);
        }

        public bool Match(string name) {
            return name.Equals(Name, StringComparison.OrdinalIgnoreCase)
                   || name.Equals(CppName, StringComparison.OrdinalIgnoreCase)
                   || Aliases.Any(x => name.Equals(x, StringComparison.OrdinalIgnoreCase));
        }

        public override bool ComparePK(object obj) {
            var emp = obj.ToMod();
            return emp != null && ComparePK(emp);
        }

        public bool GameMatch(ISupportModding game) {
            Contract.Requires<ArgumentNullException>(game != null);
            return this is LocalMod
                   || game.SupportsContent(this);
        }

        public virtual string GetRemotePath() {
            return Tools.Transfer.JoinPaths("rel", GetRepoName(Name), Repository.PackFolderName);
        }

        bool IContentEngineContent.IsInstalled => Controller.IsInstalled;
        [JsonIgnore]
        public IAbsoluteDirectoryPath PathInternal => Controller.Path;
        public string Path => PathInternal?.ToString();
        Guid IContentEngineContent.GameId
        {
            get { return GameId; }
        }

        static bool IsDeprecatedOaBeta(string value) {
            return !string.IsNullOrWhiteSpace(value) && value.ToLower() == "expansion/beta";
        }

        protected override string GetSlug() {
            return FullName.Sluggify();
        }

        IEnumerable<IAbsolutePath> ModFolders(IAbsoluteDirectoryPath modPath) {
            try {
                return TryGetModFolders(modPath);
            } catch (UnauthorizedAccessException e) {
                this.Logger().FormattedWarnException(e, "while trying to enumerate submodfolders");
                return new[] {modPath};
            }
        }

        IEnumerable<IAbsolutePath> TryGetModFolders(IAbsoluteDirectoryPath modPath) {
            var di = new DirectoryInfo(modPath.ToString());
            if (!di.Exists)
                return Enumerable.Empty<IAbsolutePath>();

            return Enumerable.Repeat(di, 1).Concat(di.RecurseFilterDottedDirectories())
                .Select(x => x.FullName)
                .Where(modPathValidator.Validate)
                .Select(x => x.ToAbsoluteDirectoryPath());
        }

        protected override string GetGameSlug() {
            var t = Type.ToString();
            if (t == "Rv3Mod" || t.StartsWith("Arma2"))
                return "arma-2";
            if (t.StartsWith("Takeonh"))
                return "take-on-helicopters";
            return "arma-3";
        }

        public static IEnumerable<LocalModInfo> GetLocalMods(string path, Game game) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(path));
            Contract.Requires<ArgumentNullException>(game != null);
            if (game.Id == GameUuids.Homeworld2) {
                return GetModFiles(path)
                    .ToArray();
            }
            return GetModFolders(path)
                .ToArray();
        }

        static IEnumerable<LocalModInfo> GetModFolders(string path) {
            var validator = new ArmaModPathValidator();
            return Directory.EnumerateDirectories(path)
                .Where(directory => TryValidate(validator, directory))
                .Select(x => new LocalModFolderInfo(x.ToAbsoluteDirectoryPath()));
        }

        static IEnumerable<LocalModInfo> GetModFiles(string path) {
            var validator = new Homeworld2ModFileValidator();
            return Directory.EnumerateFiles(path)
                .Where(file => TryValidate(validator, file))
                .Select(x => new LocalModFileInfo(x.ToAbsoluteFilePath()));
        }

        static bool TryValidate(IModPathValidator validator, string path) {
            try {
                return validator.Validate(path);
            } catch (Exception) {
                return false;
            }
        }

        public static string GetRepoName(string name) {
            Contract.Requires<ArgumentNullException>(name != null);

            return name.StartsWith("@") ? name.Substring(1).ToLower() : name.ToLower();
        }

        // TODO: Have the game report the subgame that supports the mod, first match
        public Game FirstGameMatch(Game game) {
            Contract.Requires<ArgumentNullException>(game != null);
            if (this is LocalMod)
                return game;

            return GameMatch(game.Modding()) ? game : null;
        }

        #region Mod Members

        public GameModType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public string CppName
        {
            get { return _cppName; }
            set { SetProperty(ref _cppName, value); }
        }

        public string FullName
        {
            get { return _fullName; }
            set { SetProperty(ref _fullName, value); }
        }

        public string MinBuild { get; set; }

        public string MaxBuild { get; set; }
        public Guid GameId { get; set; }
        public string Guid
        {
            get { return _guid; }
            set { SetProperty(ref _guid, value); }
        }

        public bool HasLicense
        {
            get { return _hasLicense; }
            set { SetProperty(ref _hasLicense, value); }
        }

        public string ModVersion
        {
            get { return _modVersion; }
            set { SetProperty(ref _modVersion, value); }
        }

        public virtual string GetSerializationString() {
            return Name;
        }

        public long Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public long SizeWd
        {
            get { return _sizeWd; }
            set { SetProperty(ref _sizeWd, value); }
        }

        public string[] Aliases
        {
            get { return _aliases; }
            set { SetProperty(ref _aliases, value); }
        }

        public string[] Dependencies
        {
            get { return _dependencies; }
            set { SetProperty(ref _dependencies, value); }
        }

        public Uri[] Mirrors
        {
            get { return _mirrors; }
            set { SetProperty(ref _mirrors, value); }
        }

        [SmartAssembly.ReportUsage.ReportUsage]
        public virtual void OpenChangelog() {
            BrowserHelper.TryOpenUrlIntegrated(GetUrl());
        }

        public bool CompatibleWith(ISupportModding game) {
            Contract.Requires<ArgumentNullException>(game != null);
            return GameMatch(game) && BuildMatch(game);
        }

        public string GetUrl(string type = "changelog") {
            return Tools.Transfer.JoinPaths(modUrl2, Id, type);
        }

        public void OpenReadme() {
            BrowserHelper.TryOpenUrlIntegrated(GetUrl("readme"));
        }

        public void OpenLicense() {
            BrowserHelper.TryOpenUrlIntegrated(GetUrl("license"));
        }

        public void OpenHomepage() {
            Tools.Generic.TryOpenUrl(HomepageUrl);
        }

        bool BuildMatch(IHaveInstalledState game) {
            return BuildMatch(game.InstalledState.Version);
        }

        bool BuildMatch(Version gameVersion) {
            return ConfirmMinBuild(gameVersion) && ConfirmMaxBuild(gameVersion);
        }

        bool ConfirmMaxBuild(Version gameVersion) {
            if (String.IsNullOrWhiteSpace(MaxBuild))
                return true;

            if (gameVersion == null)
                return false;
            if (MaxBuild.Contains(".")) {
                if (gameVersion > MaxBuild.TryVersion())
                    return false;
            } else {
                if (gameVersion.Revision > MaxBuild.TryInt())
                    return false;
            }
            return true;
        }

        bool ConfirmMinBuild(Version gameVersion) {
            if (String.IsNullOrWhiteSpace(MinBuild))
                return true;

            if (gameVersion == null)
                return false;
            if (MinBuild.Contains(".")) {
                if (gameVersion < MinBuild.TryVersion())
                    return false;
            } else {
                if (gameVersion.Revision < MinBuild.TryInt())
                    return false;
            }
            return true;
        }

        #endregion
    }

    public static class RepoExtensions
    {
        static readonly string[] protocols = {"http", "https", "ftp", "rsync", "zsync", "zsyncs"};

        public static Uri GetCleanuri(this Uri uri) {
            return new Uri(uri.GetLeftPart(UriPartial.Path)
                .Replace("pws://", "http://")
                .Replace("pwsftp://", "ftp://")
                .Replace("pwshttp://", "http://")
                .Replace("pwshttps://", "https://")
                .Replace("pwsrsync://", "rsync://")
                .Replace("pwszsync://", "zsync://")
                .Replace("pwszsyncs://", "zsyncs://"));
        }

        public static Uri ProcessRepoUrl(this Uri uri) {
            if (!uri.IsRepoUrl())
                throw new NotSupportedException(uri + " does not appear to be a repository URL");

            if (uri.AbsolutePath.EndsWith("/" + Repository.ConfigFileName) || uri.AbsolutePath.EndsWith(".yml"))
                return uri.GetParentUri().GetCleanedAuthlessUrl();

            return uri.GetCleanedAuthlessUrl();
        }

        public static bool IsRepoUrl(this Uri uri) {
            return !IsInvalidUri(uri) && !IsNotSupportedProtoocl(uri);
            // TODO: Only return true if either a directory path is specified, or if it is .yml file
        }

        static bool IsNotSupportedProtoocl(Uri uri) {
            return !uri.Scheme.StartsWith("pws") && !protocols.Contains(uri.Scheme);
        }

        static bool IsInvalidUri(Uri uri) {
            return !uri.IsAbsoluteUri || String.IsNullOrEmpty(uri.DnsSafeHost);
        }
    }
}