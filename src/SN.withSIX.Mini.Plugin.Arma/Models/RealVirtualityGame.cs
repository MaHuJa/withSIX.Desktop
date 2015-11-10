// <copyright company="SIX Networks GmbH" file="RealVirtualityGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using SN.withSIX.Api.Models;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Core.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;
using SN.withSIX.Mini.Plugin.Arma.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Services;
using SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public abstract class RealVirtualityGame : BasicGame, ILaunchWith<IRealVirtualityLauncher>
    {
        public const string BohemiaStudioRegistry = @"SOFTWARE\Bohemia Interactive Studio";
        public const string BohemiaRegistry = @"SOFTWARE\Bohemia Interactive";
        readonly RvContentScanner _rvContentScanner;
        readonly RealVirtualityGameSettings _settings;

        protected RealVirtualityGame(Guid id, RealVirtualityGameSettings settings) : base(id, settings) {
            _settings = settings;
            ProfileInfo = this.GetMetaData<RvProfileInfoAttribute>();

            SetupDefaultDirectories();
            _rvContentScanner = new RvContentScanner(this);
        }

        RvProfileInfoAttribute ProfileInfo { get; }
        protected override bool DefaultDirectoriesOverriden => true;

        void SetupDefaultDirectories() {
            if (Settings.GameDirectory == null)
                Settings.GameDirectory = GetDefaultDirectory();
            if (_settings.PackageDirectory == null)
                _settings.PackageDirectory = GetDefaultPackageDirectory();
            if (_settings.RepoDirectory == null && _settings.PackageDirectory != null)
                _settings.RepoDirectory = _settings.PackageDirectory;
        }

        protected override Task ScanForLocalContentImpl() => Task.Run(() => ScanForLocalContentInternal());

        void ScanForLocalContentInternal() {
            var newContent = _rvContentScanner.ScanForNewContent(Metadata.Dlcs, GetModFolders()).ToArray();
            if (newContent.Any())
                AddLocalContent(newContent);
        }

        IEnumerable<IAbsoluteDirectoryPath> GetModFolders() {
            if (ContentPaths.IsValid)
                yield return ContentPaths.Path;
            yield return InstalledState.Directory;
        }

        // TODO: Refactor out the MyDocuments Environment grabbing
        IAbsoluteDirectoryPath GetDefaultPackageDirectory()
            => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                .ToAbsoluteDirectoryPath()
                .GetChildDirectoryWithName(ProfileInfo.DocumentsMainName);

        protected override InstallContentAction GetInstallAction(
            IContentAction<IInstallableContent> action)
            => new InstallContentAction(action.Content, action.CancelToken) {
                RemoteInfo = RemoteInfo,
                Paths = ContentPaths,
                Game = this,
                Cleaning = ContentCleaning
            };

        protected override IAbsoluteDirectoryPath GetContentDirectory() => _settings.PackageDirectory;

        protected override async Task<Process> LaunchImpl(IGameLauncherFactory factory,
            ILaunchContentAction<IContent> action) {
            var launcher = factory.Create<IRealVirtualityLauncher>(this);
            var startupParameters = await GetStartupParameters(launcher, action).ConfigureAwait(false);

            return await (IsLaunchingSteamApp()
                ? LaunchWithSteam(launcher, startupParameters)
                : LaunchNormal(launcher, startupParameters)).ConfigureAwait(false);
        }

        Tuple<string[], string[]> RvStartupParameters(IRealVirtualityLauncher launcher,
            ILaunchContentAction<IContent> action)
            => GetStartupBuilder().GetStartupParameters(GetStartupSpec(action));

        protected virtual StartupBuilder GetStartupBuilder() => new StartupBuilder(this, new ModListBuilder());

        protected virtual async Task<IEnumerable<string>> GetStartupParameters(IRealVirtualityLauncher launcher,
            ILaunchContentAction<IContent> action) {
            var startupInfo = RvStartupParameters(launcher, action);
            var startupParameters = startupInfo.Item1.ToList();
            var parData = startupInfo.Item2;
            if (!parData.Any())
                return startupParameters;

            try {
                var parPath =
                    await
                        launcher.WriteParFile(new WriteParFileInfo(Id, string.Join("\n", parData)
                            //, new ShortGuid(Settings.CurrentProfile).ToString()
                            ))
                            .ConfigureAwait(false);
                startupParameters.Insert(0, "-par=" + parPath);
            } catch (Exception e) {
                throw new ParFileException(e.Message, e);
            }

            return startupParameters;
        }

        protected override async Task InstallImpl(IContentInstallationService installationService,
            IContentAction<IInstallableContent> content) {
            var installAction = GetInstallAction(content);
            PrepareUserconfigPostInstallActions(installAction);
            await installationService.Install(installAction).ConfigureAwait(false);
        }

        void PrepareUserconfigPostInstallActions(InstallContentAction installAction) {
            // TODO: userconfig processing should happen as part of the Mod Installation
            // However we do not have Mod implementation classes per Game atm. Should we?
            // Or should we rather make it part of the ContentEngine?
            // In that case we could determine actions like UserConfig processing based on the Guid or type of the game,
            // then the flow of control is at least at the Mod, instead of here..

            // TODO: Cache the existing userconfig checksums etc like in PwS?
            var ucp = new UserconfigProcessor();
            foreach (
                var c in installAction.Content.SelectMany(x => x.Content.GetRelatedContent(constraint: x.Constraint))
                    .Select(x => x.Content).Distinct().OfType<IHavePackageName>()) {
                var c2 = (IContent) c;
                c2.RegisterAdditionalPostInstallTask(
                    async () => ucp.ProcessUserconfig(installAction.Paths.Path.GetChildDirectoryWithName(c.PackageName),
                        InstalledState.Directory, null));
            }
        }

        StartupBuilderSpec GetStartupSpec(ILaunchContentAction<IContent> action) {
            var content = action.GetLaunchables().ToArray();

            return new StartupBuilderSpec {
                GamePath = InstalledState.Directory,
                LaunchType = action.LaunchType,
                ModPath = ContentPaths.Path,
                GameVersion = InstalledState.Version,
                InputMods = content.OfType<IModContent>(), //.Where(x => x.Controller.Exists),
                AdditionalLaunchMods = GetAdditionalLaunchMods(),
                //Mission = CalculatedSettings.Mission,
                Server = GetServer(action, content),
                // TODO: Or configurable by user?
                StartupParameters = Settings.StartupParameters.Get(),
                UseParFile = true
            };
        }

        static CollectionServer GetServer(ILaunchContentAction<IContent> action, ILaunchableContent[] content) {
            if (action.Action == LaunchAction.Default)
                return content.OfType<IHaveServers>().FirstOrDefault()?.Servers.FirstOrDefault();
            return action.Action == LaunchAction.Join
                ? content.OfType<IHaveServers>().First().Servers.First()
                : null;
        }

        protected virtual IEnumerable<IAbsoluteDirectoryPath> GetAdditionalLaunchMods()
            => Enumerable.Empty<IAbsoluteDirectoryPath>();

        // TODO: Local mods in separate folders (Custom path) support
        protected class ModListBuilder
        {
            static readonly ArmaModPathValidator modPathValidator = new ArmaModPathValidator();
            protected List<IModContent> InputMods;
            protected List<IAbsoluteDirectoryPath> OutputMods;
            protected ModListBuilderSpec Spec;

            public IEnumerable<string> ProcessModList(ModListBuilderSpec spec) {
                Spec = spec;
                OutputMods = new List<IAbsoluteDirectoryPath>();
                InputMods = Spec.InputMods.ToList();

                ProcessMods();

                return CleanModList(OutputMods).DistinctBy(x => x.ToLower());
            }

            protected IEnumerable<IAbsoluteDirectoryPath> GetModPaths(IModContent arg) {
                return Enumerable.Repeat(CombineModPathIfNeeded(arg.PackageName), 1);
                // TODO
                //return arg.GetPaths().OfType<IAbsoluteDirectoryPath>();
            }

            IAbsoluteDirectoryPath CombineModPathIfNeeded(string mod) {
                return Spec.ModPath == null
                    ? Spec.GamePath.GetChildDirectoryWithName(mod)
                    : Spec.ModPath.GetChildDirectoryWithName(mod);
            }

            static string SubMod(IModContent mod, string name) {
                return mod.PackageName + "/" + name;
            }

            protected virtual void ProcessMods() {
                AddPrimaryGameFolders();
                AddNormalMods();
            }

            protected void AddPrimaryGameFolders() {
                OutputMods.AddRange(Spec.AdditionalLaunchMods);
            }

            void AddNormalMods() {
                OutputMods.AddRange(InputMods.SelectMany(GetModPaths));
            }

            IEnumerable<string> CleanModList(IEnumerable<IAbsoluteDirectoryPath> modList) {
                return modList.Distinct()
                    .Where(x => modPathValidator.Validate(x))
                    .Select(CleanModPath).DistinctBy(x => x.ToLower());
            }

            string CleanModPath(IAbsoluteDirectoryPath fullModPath) {
                return fullModPath.GetRelativeDirectory(Spec.GamePath);
            }
        }

        protected class ModListBuilderSpec
        {
            public IEnumerable<IAbsoluteDirectoryPath> AdditionalLaunchMods = new IAbsoluteDirectoryPath[0];
            public IAbsoluteDirectoryPath GamePath;
            public IEnumerable<IModContent> InputMods = new IModContent[0];
            public IAbsoluteDirectoryPath ModPath;
        }

        protected class ParFileException : Exception
        {
            public ParFileException(string message, Exception exception) : base(message, exception) {}
        }

        protected class RvProfileInfo
        {
            public RvProfileInfo(string mainName, string otherProfilesName, string profileExtension) {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(mainName));
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(otherProfilesName));
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(profileExtension));

                DocumentsMainName = mainName;
                DocumentsOtherProfilesName = otherProfilesName;
                ProfileExtension = profileExtension;
            }

            public string DocumentsMainName { get; private set; }
            public string DocumentsOtherProfilesName { get; private set; }
            public string ProfileExtension { get; private set; }
        }

        protected class StartupBuilder
        {
            readonly string[] _doesNotSupportPar = {
                "-malloc=", "-cpuCount=", "-exThreads=", "-maxMem=", "-maxVram=",
                "-enableHT", "-profiles=", "-cfg="
            };
            readonly ModListBuilder _modListBuilder;
            readonly bool _supportsMissions;
            readonly bool _supportsMods;
            readonly bool _supportsServers;
            List<string> _arguments;
            List<string> _par;
            StartupBuilderSpec _spec;

            public StartupBuilder(RealVirtualityGame game) {
                Contract.Requires<ArgumentNullException>(game != null);
                //_supportsMods = game.SupportsMods();
                //_supportsMissions = game.SupportsMissions();
                //_supportsServers = game.SupportsServers();

                if (_supportsMods)
                    _modListBuilder = new ModListBuilder();
            }

            public StartupBuilder(RealVirtualityGame game, ModListBuilder builder) {
                Contract.Requires<ArgumentNullException>(game != null);
                Contract.Requires<ArgumentNullException>(builder != null);
                _supportsMods = true;
                //_supportsMissions = game.SupportsMissions();
                //_supportsServers = game.SupportsServers();
                _modListBuilder = builder;
            }

            public Tuple<string[], string[]> GetStartupParameters(StartupBuilderSpec spec) {
                _spec = spec;
                _arguments = new List<string>();
                _par = new List<string>();

                DealWithLaunchType();

                //if (_supportsServers)
                AddServerArgs();
                if (_supportsMods)
                    AddModArgs();
                //if (_supportsMissions)
                //AddMissionArgs();

                AddCommandArgs();
                AddGameArgs();

                return Tuple.Create(_arguments.Distinct().ToArray(), _par.Distinct().ToArray());
            }

            void DealWithLaunchType() {
                switch (_spec.LaunchType) {
                case LaunchType.Multiplayer: {
                    AddArgOrPar("-host");
                    break;
                }
                /*
                case LaunchType.Editor: {
                    AddArgOrPar("-editor");
                    break;
                }*/
                }
            }

            void AddArgOrPar(string arg) {
                if (_spec.UseParFile)
                    _par.Add(arg);
                else
                    _arguments.Add(arg);
            }

            /*
        void AddMissionArgs()
        {
            var missionFolder = _spec.Mission as MissionFolder;
            if (missionFolder == null)
                AddMissionFileArgs();
            else
                AddMissionFolderArgs(missionFolder);
        }

    void AddMissionFolderArgs(MissionFolder missionFolder)
    {
        if (missionFolder == null)
            return;

        var folder = missionFolder.CustomPath.GetChildDirectoryWithName(missionFolder.FolderName);
        if (_spec.UseParFile)
            _par.Add(GetMissionFolderParameter(folder));
        else
            _arguments.Add(GetMissionFolderParameter(folder));
    }

        static string GetMissionFolderParameter(IAbsoluteDirectoryPath missionFolder)
            {
                return missionFolder + "\\mission.sqm";
            }

            void AddMissionFileArgs()
            {
                if (_spec.Mission == null)
                    return;

                var param = GetMissionParameter(_spec.Mission);
                if (param == null)
                    return;

                if (_spec.UseParFile)
                    _par.Add(param);
                else
                    _arguments.Add(param);
            }

            static string GetMissionParameter(MissionBase mission)
            {
                var missionFile = mission.Controller.GetMissionFile();
                if (missionFile == null)
                    return null;

                var folder = missionFile as IAbsoluteDirectoryPath;
                return folder != null
                    ? GetMissionFolderParameter(folder)
                    : GetMissionParameterBasedOnType(mission, (IAbsoluteFilePath)missionFile);
            }

            static string GetMissionParameterBasedOnType(MissionBase mission, IAbsoluteFilePath missionFile)
            {
                return mission.Type == MissionTypes.MpMission
                    ? "-host"
                    : string.Format("-init=playMission['', '{0}']", missionFile.FileNameWithoutExtension);
            }
                        */

            void AddGameArgs() {
                if (_spec.UseParFile)
                    AddGameArgsToParFileAndArguments();
                else
                    AddGameArgsToArguments();
            }

            void AddGameArgsToParFileAndArguments() {
                _arguments.AddRange(_spec.StartupParameters.Where(ShouldBeOnStartupLine));
                _par.AddRange(_spec.StartupParameters.Where(ShouldBeOnParLine));
            }

            void AddGameArgsToArguments() {
                _arguments.AddRange(_spec.StartupParameters);
            }

            bool ShouldBeOnStartupLine(string arg) {
                return _doesNotSupportPar.Any(x => arg.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
            }

            bool ShouldBeOnParLine(string arg) {
                return !ShouldBeOnStartupLine(arg);
            }

            void AddCommandArgs() {
                var gv = _spec.GameVersion;
                if (gv == null || gv.Revision < 100399)
                    return;
                var arg = String.Format("-command={0}", CommandAPI.DefaultPipeTag);
                if (_spec.UseParFile)
                    _par.Add(arg);
                else
                    _arguments.Add(arg);
            }

            void AddModArgs() {
                var modStr = String.Join(";", _modListBuilder.ProcessModList(new ModListBuilderSpec {
                    InputMods = _spec.InputMods,
                    GamePath = _spec.GamePath,
                    ModPath = _spec.ModPath,
                    AdditionalLaunchMods = _spec.AdditionalLaunchMods
                }));

                var arg = String.Format("-mod={0}", modStr);
                if (_spec.UseParFile)
                    _par.Add(arg);
                else
                    _arguments.Add(arg);
            }

            void AddServerArgs() {
                if (_spec.Server == null)
                    return;
                var args = GetServerArgs().ToArray();
                if (_spec.UseParFile) {
                    _par.AddRange(args);
                    _arguments.AddRange(args.Take(2));
                    // To pick up running games with server, but leave password off because it breaks password usage in the par file :S
                } else
                    _arguments.AddRange(args);
            }

            static IEnumerable<IPAddress> GetSystemIpAddresses() {
                return (from netInterface in NetworkInterface.GetAllNetworkInterfaces()
                    select netInterface.GetIPProperties()
                    into ipProps
                    from addr in ipProps.UnicastAddresses
                    select addr.Address);
            }

            IPAddress[] TryGetSystemIpAddresses() {
                try {
                    return GetSystemIpAddresses().ToArray();
                } catch (Exception ex) {
                    MainLog.Logger.FormattedWarnException(ex, "Failure to determine ip addresses");
                    return new IPAddress[0];
                }
            }

            IEnumerable<string> GetServerArgs() {
                var sa = new ServerAddress(_spec.Server.Address);
                var args = new List<string> {
                    "-connect=" + GetServerIP(sa.IP),
                    "-port=" + sa.Port
                };

                if (!String.IsNullOrWhiteSpace(_spec.Server.Password))
                    args.Add("-password=" + _spec.Server.Password);
                return args;
            }

            IPAddress GetServerIP(IPAddress ip) {
                return TryGetSystemIpAddresses().Contains(ip) ? GetLoopbackIP(ip) : ip;
            }

            static IPAddress GetLoopbackIP(IPAddress ip) {
                return ip.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback;
            }
        }

        public interface IModPathValidator
        {
            bool Validate(string path);
        }

        public abstract class PathValidator
        {
            static bool IsValidPath(string path) {
                return Tools.FileUtil.IsValidRootedPath(path);
            }

            protected static bool HasSubFolder(string path, string subFolder) {
                return Directory.Exists(Path.Combine(path, subFolder));
            }

            protected static bool ValidateBasics(string path) {
                return IsValidPath(path) && Directory.Exists(path);
            }
        }

        public class ArmaModPathValidator : PathValidator, IModPathValidator
        {
            static readonly string[] gameDataDirectories = {"addons", "dta", "common", "dll"};

            public bool Validate(string path) {
                return ValidateBasics(path) && ValidateSpecials(path);
            }

            public bool Validate(IAbsoluteDirectoryPath path) {
                return Validate(path.ToString());
            }

            static bool ValidateSpecials(string path) {
                return gameDataDirectories.Any(dir => HasSubFolder(path, dir));
            }
        }

        protected class StartupBuilderSpec
        {
            public IEnumerable<IAbsoluteDirectoryPath> AdditionalLaunchMods { get; set; } =
                new IAbsoluteDirectoryPath[0];
            public IAbsoluteDirectoryPath GamePath { get; set; }
            public Version GameVersion { get; set; }
            public IEnumerable<IModContent> InputMods { get; set; } = new IModContent[0];
            //public MissionBase Mission { get; set; }
            public IAbsoluteDirectoryPath ModPath { get; set; }
            public CollectionServer Server { get; set; }
            public IEnumerable<string> StartupParameters { get; set; } = new string[0];
            public bool UseParFile { get; set; }
            public LaunchType LaunchType { get; set; }
        }
    }
}