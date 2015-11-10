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
using System.Threading.Tasks;
using Caliburn.Micro;
using MoreLinq;
using NDepend.Path;
using SN.withSIX.Api.Models;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities.Requirements;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Arma.Commands;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public abstract class RealVirtualityGame : Game, ILaunchWith<IRealVirtualityLauncher>
    {
        protected const string BohemiaStudioRegistry = @"SOFTWARE\Bohemia Interactive Studio";
        protected const string BohemiaRegistry = @"SOFTWARE\Bohemia Interactive";
        protected new static readonly string[] DefaultStartupParameters = {"-nosplash", "-nofilepatching"};
        static readonly IEnumerable<Requirement> requirements = new[] {new DirectXRequirement("9.0".ToVersion())};

        protected RealVirtualityGame(Guid id, RealVirtualitySettings settings) : base(id, settings) {
            Settings = settings;
        }

        public new RealVirtualitySettings Settings { get; }
        protected virtual string MissionExtension
        {
            get { return ".pbo"; }
        }
        protected abstract RvProfileInfo ProfileInfo { get; }
        protected virtual IEnumerable<Requirement> Requirements
        {
            get { return requirements; }
        }

        public override async Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory,
            string identifier) {
            var launchHandler = factory.Create(this);
            var parametersForShortcut =
                await BuildStartupParametersForShortcut(launchHandler, identifier).ConfigureAwait(false);
            return parametersForShortcut.ToArray();
        }

        public override async Task<int> Launch(IGameLauncherFactory factory) {
            var launchHandler = factory.Create(this);
            await PreLaunch(launchHandler).ConfigureAwait(false);
            return await PerformLaunch(launchHandler).ConfigureAwait(false);
        }

        protected static IAbsoluteFilePath GetExe(IAbsoluteDirectoryPath gameExePath,
            SeparateClientAndServerExecutable executables, bool forceServer) {
            var executable = gameExePath.GetChildFileWithName(executables.Client);
            var serverExecutable = gameExePath.GetChildFileWithName(executables.Server);
            if (forceServer)
                return serverExecutable;
            if (!executable.Exists && serverExecutable.Exists)
                return serverExecutable;
            return executable;
        }

        protected void InstallBiKeys(IEnumerable<ModController> procced) {
            var keysPath = InstalledState.Directory.GetChildDirectoryWithName("keys");
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(keysPath);
            foreach (var key in GetKeys(procced)) {
                Tools.FileUtil.Ops.Copy(key,
                    keysPath.GetChildFileWithName(key.FileName), true, true);
            }
        }

        static IEnumerable<IAbsoluteFilePath> GetKeys(IEnumerable<ModController> procced) {
            return procced.SelectMany(mod => mod.GetBiKeys());
        }

        protected override string GetStartupLine() {
            var installedState = InstalledState;
            return installedState.IsInstalled
                ? new[] {installedState.LaunchExecutable.ToString()}.Concat(GetFakeParameterLine()).CombineParameters()
                : string.Empty;
        }

        IEnumerable<string> GetFakeParameterLine() {
            var startupParameters = StartupParameters();
            var combinedParameters = startupParameters.Item1.Concat(startupParameters.Item2).Distinct();
            return combinedParameters;
        }

        void ConfirmRequirements() {
            foreach (var r in Requirements)
                r.ThrowWhenMissing();
        }

        async Task<bool> CommandJoin(Server server) {
            var rg = Running;
            if (rg == null || !rg.CommandAPI.IsConnected || !rg.CommandAPI.IsReady ||
                rg.Collection != CalculatedSettings.Collection)
                return false;

            var cmd = new ConnectCommand(server.ServerAddress) {
                Password = server.SavedPassword
            };
            await rg.CommandAPI.QueueSend(
                new MessageCommand(
                    String.Format(
                        "Received connect command to {0}:{1}. (If nothing happens, please first return to main menu, and retry)",
                        cmd.Ip, cmd.Port))).ConfigureAwait(false);
            await rg.CommandAPI.QueueSend(cmd).ConfigureAwait(false);
            rg.SwitchTo();
            return true;
        }

        protected virtual Tuple<string[], string[]> StartupParameters() {
            var startupBuilder = new StartupBuilder(this);
            return startupBuilder.GetStartupParameters(new StartupBuilderSpec {
                GamePath = InstalledState.Directory,
                GameVersion = GetVersion(),
                Mission = CalculatedSettings.Mission,
                Server = ShouldConnectToServer() ? CalculatedSettings.Server : null,
                StartupParameters = Settings.StartupParameters.Get(),
                UseParFile = true
            });
        }

        protected virtual bool ShouldConnectToServer() {
            return InstalledState.IsClient;
        }

        protected IEnumerable<string> GetProfilesInternal() {
            var profiles = new List<string>();
            GetProfiles(GetDocumentsGamePath(), profiles,
                ProfileInfo.ProfileExtension);
            GetProfiles(GetDocumentsGameProfilesPath(),
                profiles, ProfileInfo.ProfileExtension);

            return profiles;
        }

        IAbsoluteDirectoryPath GetDocumentsGameProfilesPath() {
            return
                DomainEvilGlobal.LocalMachineInfo.DocumentsPath.GetChildDirectoryWithName(
                    ProfileInfo.DocumentsOtherProfilesName);
        }

        protected IAbsoluteDirectoryPath GetDocumentsGamePath() {
            return
                DomainEvilGlobal.LocalMachineInfo.DocumentsPath.GetChildDirectoryWithName(ProfileInfo.DocumentsMainName);
        }

        protected ContentPaths[] GetMissionPaths(IAbsoluteDirectoryPath repositoryDirectory) {
            return InstalledState.IsInstalled
                ? new[] {
                    new ContentPaths(GetMissionsDirectory(), GetMissionRepositoryDirectory(repositoryDirectory)),
                    new ContentPaths(GetMpMissionsDirectory(), GetMpMissionRepositoryDirectory(repositoryDirectory))
                }
                : new ContentPaths[0];
        }

        IAbsoluteDirectoryPath GetMissionRepositoryDirectory(IAbsoluteDirectoryPath repositoryDirectory) {
            return repositoryDirectory ?? GetMissionsDirectory();
        }

        IAbsoluteDirectoryPath GetMissionsDirectory() {
            return InstalledState.Directory.GetChildDirectoryWithName("missions");
        }

        IAbsoluteDirectoryPath GetMpMissionRepositoryDirectory(IAbsoluteDirectoryPath repositoryDirectory) {
            return repositoryDirectory ?? GetMpMissionsDirectory();
        }

        IAbsoluteDirectoryPath GetMpMissionsDirectory() {
            return InstalledState.Directory.GetChildDirectoryWithName("mpmissions");
        }

        protected void PublishMissionInternal(string fn) {
            var doc1 = GetDocumentsGamePath();
            var doc2 = GetDocumentsGameProfilesPath();

            var path = Path.Combine(doc1.ToString(), MissionFolders.SpMissions, fn).ToAbsoluteDirectoryPath();
            if (!path.Exists) {
                path = Path.Combine(doc1.ToString(), MissionFolders.MpMissions, fn).ToAbsoluteDirectoryPath();
                if (!path.Exists && doc2.Exists) {
                    var di = doc2.DirectoryInfo;
                    foreach (var d in di.EnumerateDirectories()) {
                        path = Path.Combine(d.FullName, MissionFolders.SpMissions, fn).ToAbsoluteDirectoryPath();
                        if (path.Exists)
                            break;
                        path = Path.Combine(d.FullName, MissionFolders.MpMissions, fn).ToAbsoluteDirectoryPath();
                        if (path.Exists)
                            break;
                    }
                }
            }

            if (!path.Exists)
                throw new DirectoryNotFoundException();

            Common.App.Events.PublishOnUIThread(new RequestPublishMission(new MissionFolder(Guid.Empty) {
                CustomPath = path.ParentDirectoryPath,
                Name = path.DirectoryName,
                FolderName = path.DirectoryName
            }));
        }

        protected IEnumerable<LocalMissionsContainer> GetLocalMissionsContainers() {
            var installedState = InstalledState;
            if (!installedState.IsInstalled)
                return Enumerable.Empty<LocalMissionsContainer>();
            var gamePath = installedState.Directory;
            var list = new List<LocalMissionsContainer>();
            if (gamePath != null)
                list.Add(new LocalMissionsContainer("Game folder", gamePath.ToString(), this));

            var userPath = GetDocumentsGamePath();
            if (userPath != null)
                list.Add(new LocalMissionsContainer("User folder", userPath.ToString(), this));

            return list;
        }

        static void GetProfiles(IDirectoryPath path, List<string> profiles, string ext) {
            if (Directory.Exists(path.ToString()))
                profiles.AddRange(GetProfilesInternal(path, ext));
        }

        static IEnumerable<string> GetProfilesInternal(IDirectoryPath path, string ext) {
            var filenames = Directory.EnumerateFiles(path.ToString(), "*." + ext, SearchOption.AllDirectories);
            return from filename in filenames
                where !filename.Contains(".vars.")
                select Uri.UnescapeDataString(Path.GetFileNameWithoutExtension(filename));
        }

        async Task<int> PerformLaunch(IRealVirtualityLauncher launcher) {
            if (CalculatedSettings.Server != null && await CommandJoin(CalculatedSettings.Server).ConfigureAwait(false))
                return Running.Process.Id;

            var p = await PerformNewLaunch(launcher).ConfigureAwait(false);
            return await RegisterLaunchIf(p, launcher).ConfigureAwait(false);
        }

        protected virtual Task<Process> PerformNewLaunch(IRealVirtualityLauncher launcher) {
            if ((SteamInfo.DRM || Settings.InjectSteam) && !Settings.LaunchUsingSteam)
                return LaunchSteamModern(launcher);

            return Settings.LaunchUsingSteam ? LaunchSteamLegacy(launcher) : LaunchNormal(launcher);
        }

        protected async Task<Process> LaunchNormal(IRealVirtualityLauncher launcher) {
            if (Settings.ResetGameKeyEachLaunch)
                using (await launcher.Launch(GetDoNothingGameLaunchCommand()).ConfigureAwait(false)) {}

            return
                await
                    launcher.Launch(
                        LaunchParameters(await BuildStartupParameters(launcher).ConfigureAwait(false)))
                        .ConfigureAwait(false);
        }

        protected virtual async Task<Process> LaunchSteamLegacy(IRealVirtualityLauncher launcher) {
            // TODO: Detect if the game actually is available on Steam, and is the same directory as configured in pws.
            return
                await
                    launcher.Launch(
                        SteamLegacyLaunchParameters(await BuildStartupParameters(launcher).ConfigureAwait(false)))
                        .ConfigureAwait(false);
        }

        protected async Task<Process> LaunchSteamModern(IRealVirtualityLauncher launcher) {
            if (Settings.ResetGameKeyEachLaunch)
                using (await launcher.Launch(GetDoNothingGameLaunchCommand()).ConfigureAwait(false)) {}
            return
                await
                    launcher.Launch(
                        SteamLaunchParameters(await BuildStartupParameters(launcher).ConfigureAwait(false)))
                        .ConfigureAwait(false);
        }

        protected virtual async Task<IEnumerable<string>> BuildStartupParameters(
            IRealVirtualityLauncher launcher) {
            var startupInfo = StartupParameters();
            var startupParameters = startupInfo.Item1.ToList();
            var parData = startupInfo.Item2;
            if (!parData.Any())
                return startupParameters;

            try {
                var parPath =
                    await
                        launcher.WriteParFile(new WriteParFileInfo(Id, string.Join("\n", parData),
                            new ShortGuid(Settings.CurrentProfile).ToString()))
                            .ConfigureAwait(false);
                startupParameters.Insert(0, "-par=" + parPath);
            } catch (Exception e) {
                throw new ParFileException(e.Message, e);
            }

            return startupParameters;
        }

        protected virtual async Task<IReadOnlyCollection<string>> BuildStartupParametersForShortcut(IRealVirtualityLauncher mediator,
            string identifier) {
            var startupInfo = StartupParameters();
            var startupParameters = startupInfo.Item1.ToList();
            var parData = startupInfo.Item2;
            if (!parData.Any())
                return startupParameters;

            try {
                var parPath =
                    await
                        mediator.WriteParFile(new WriteParFileInfo(Id, string.Join("\n", parData), identifier))
                            .ConfigureAwait(false);
                startupParameters.Insert(0, "-par=" + parPath);
            } catch (Exception e) {
                throw new ParFileException(e.Message, e);
            }

            return startupParameters;
        }

        protected override void LaunchChecks() {
            base.LaunchChecks();
            ConfirmRequirements();
        }

        protected LaunchGameInfo LaunchParameters(IEnumerable<string> startupParameters) {
            return new LaunchGameInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory, startupParameters) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    InjectSteam = Settings.InjectSteam,
                    Priority = Settings.Priority
                };
        }

        protected LaunchGameWithSteamInfo SteamLaunchParameters(IEnumerable<string> startupParameters) {
            return new LaunchGameWithSteamInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory, startupParameters) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    SteamAppId = SteamInfo.AppId,
                    SteamDRM = SteamInfo.DRM,
                    Priority = Settings.Priority
                };
        }

        protected LaunchGameWithSteamLegacyInfo SteamLegacyLaunchParameters(IEnumerable<string> startupParameters) {
            return new LaunchGameWithSteamLegacyInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory, startupParameters) {
                    LaunchAsAdministrator = GetLaunchAsAdministrator(),
                    SteamAppId = SteamInfo.AppId,
                    SteamDRM = SteamInfo.DRM,
                    Priority = Settings.Priority
                };
        }

        protected LaunchGameWithSteamLegacyInfo GetDoNothingGameLaunchCommand() {
            return new LaunchGameWithSteamLegacyInfo(InstalledState.LaunchExecutable, InstalledState.Executable,
                InstalledState.WorkingDirectory, new[] {"-doNothing"}) {
                    SteamAppId = SteamInfo.AppId,
                    WaitForExit = true,
                    SteamDRM = SteamInfo.DRM,
                    Priority = Settings.Priority
                };
        }

        protected class ModListBuilder
        {
            static readonly ArmaModPathValidator modPathValidator = new ArmaModPathValidator();
            protected List<IMod> InputMods;
            protected List<IAbsoluteDirectoryPath> OutputMods;
            protected ModListBuilderSpec Spec;

            public IEnumerable<string> ProcessModList(ModListBuilderSpec spec) {
                Spec = spec;
                OutputMods = new List<IAbsoluteDirectoryPath>();
                InputMods = Spec.InputMods.ToList();

                ProcessMods();

                return CleanModList(OutputMods).DistinctBy(x => x.ToLower());
            }

            protected virtual void ProcessMods() {
                AddPrimaryGameFolders();
                AddNormalMods();
            }

            protected void AddPrimaryGameFolders() {
                OutputMods.AddRange(Spec.AdditionalLaunchMods);
            }

            void AddNormalMods() {
                OutputMods.AddRange(InputMods.SelectMany(x => x.GetPaths().OfType<IAbsoluteDirectoryPath>()));
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
            public IEnumerable<IMod> InputMods = new IMod[0];
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

            public string DocumentsMainName { get; }
            public string DocumentsOtherProfilesName { get; }
            public string ProfileExtension { get; }
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
                _supportsMods = game.SupportsMods();
                _supportsMissions = game.SupportsMissions();
                _supportsServers = game.SupportsServers();

                if (_supportsMods)
                    _modListBuilder = new ModListBuilder();
            }

            public StartupBuilder(RealVirtualityGame game, ModListBuilder builder) {
                Contract.Requires<ArgumentNullException>(game != null);
                Contract.Requires<ArgumentNullException>(builder != null);
                _supportsMods = true;
                _supportsMissions = game.SupportsMissions();
                _supportsServers = game.SupportsServers();
                _modListBuilder = builder;
            }

            public Tuple<string[], string[]> GetStartupParameters(StartupBuilderSpec spec) {
                _spec = spec;
                _arguments = new List<string>();
                _par = new List<string>();

                if (_supportsServers)
                    AddServerArgs();
                if (_supportsMods)
                    AddModArgs();
                if (_supportsMissions)
                    AddMissionArgs();

                AddCommandArgs();
                AddGameArgs();

                return Tuple.Create(_arguments.Distinct().ToArray(), _par.Distinct().ToArray());
            }

            void AddMissionArgs() {
                var missionFolder = _spec.Mission as MissionFolder;
                if (missionFolder == null)
                    AddMissionFileArgs();
                else
                    AddMissionFolderArgs(missionFolder);
            }

            void AddMissionFolderArgs(MissionFolder missionFolder) {
                if (missionFolder == null)
                    return;

                var folder = missionFolder.CustomPath.GetChildDirectoryWithName(missionFolder.FolderName);
                if (_spec.UseParFile)
                    _par.Add(GetMissionFolderParameter(folder));
                else
                    _arguments.Add(GetMissionFolderParameter(folder));
            }

            static string GetMissionFolderParameter(IAbsoluteDirectoryPath missionFolder) {
                return missionFolder + "\\mission.sqm";
            }

            void AddMissionFileArgs() {
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

            static string GetMissionParameter(MissionBase mission) {
                var missionFile = mission.Controller.GetMissionFile();
                if (missionFile == null)
                    return null;

                var folder = missionFile as IAbsoluteDirectoryPath;
                return folder != null
                    ? GetMissionFolderParameter(folder)
                    : GetMissionParameterBasedOnType(mission, (IAbsoluteFilePath) missionFile);
            }

            static string GetMissionParameterBasedOnType(MissionBase mission, IAbsoluteFilePath missionFile) {
                return mission.Type == MissionTypes.MpMission
                    ? "-host"
                    : string.Format("-init=playMission['', '{0}']", missionFile.FileNameWithoutExtension);
            }

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
                var args = new List<string> {
                    "-connect=" + GetServerIP(_spec.Server.ServerAddress.IP),
                    "-port=" + _spec.Server.ServerAddress.Port
                };

                if (!String.IsNullOrWhiteSpace(_spec.Server.SavedPassword))
                    args.Add("-password=" + _spec.Server.SavedPassword);
                return args;
            }

            IPAddress GetServerIP(IPAddress ip) {
                return TryGetSystemIpAddresses().Contains(ip) ? GetLoopbackIP(ip) : ip;
            }

            static IPAddress GetLoopbackIP(IPAddress ip) {
                return ip.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback;
            }
        }

        protected class StartupBuilderSpec
        {
            public IEnumerable<IAbsoluteDirectoryPath> AdditionalLaunchMods = new IAbsoluteDirectoryPath[0];
            public IAbsoluteDirectoryPath GamePath;
            public Version GameVersion;
            public IEnumerable<IMod> InputMods = new IMod[0];
            public MissionBase Mission;
            public IAbsoluteDirectoryPath ModPath;
            public Server Server;
            public IEnumerable<string> StartupParameters = new string[0];
            public bool UseParFile;
        }
    }

    public interface IRealVirtualityLauncher : IGameLauncher, ILaunch, ILaunchWithSteam,
        ILaunchWithSteamLegacy
    {
        Task<IAbsoluteFilePath> WriteParFile(WriteParFileInfo info);
    }
}