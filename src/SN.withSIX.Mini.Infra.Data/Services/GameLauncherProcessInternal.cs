// <copyright company="SIX Networks GmbH" file="GameLauncherProcessInternal.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    // TODO: Decide if we should relaunch the withSIX process, more like the withSIX-Updater, or stick to current implementation.
    // (outputting the LaunchGameInfos to console commands, and picking them up again and transforming them to LaunchSpec)
    // That kind of processing would then need to happen before the App.xaml.cs's 'SingleInstance' processing has been completed.
    public class GameLauncherProcessInternal : IEnableLogging, IGameLauncherProcess, IInfrastructureService
    {
        public async Task<Process> LaunchInternal(LaunchGameInfo info)
            => Process.GetProcessById(new GameLauncher().LaunchGame(info.ToLaunchSpec()));

        public async Task<Process> LaunchInternal(LaunchGameWithJavaInfo info)
            => Process.GetProcessById(new GameLauncher().LaunchGame(info.ToLaunchSpec()));

        public async Task<Process> LaunchInternal(LaunchGameWithSteamInfo info)
            => Process.GetProcessById(new GameLauncher().LaunchGame(info.ToLaunchSpec()));

        public async Task<Process> LaunchInternal(LaunchGameWithSteamLegacyInfo info)
            => Process.GetProcessById(new GameLauncher().LaunchGame(info.ToLaunchSpec()));

        public static class SteamInfos
        {
            public const string SteamExecutable = "Steam.exe";
            public const string SteamServiceExecutable = "steamservice.exe";
        }

        // TODO: Support for the SteamAPI/Injector
        // TODO: Re-use the external launcher option - by relaunching sync.exe with the commandline params etc.
        public class GameLauncher
        {
            //readonly IRestarter _restarter;
            //readonly IShutdownHandler _shutdownHandler;
            ProcessStartInfo _gameStartInfo;
            bool _isSteamGameAndAvailable;
            bool _isSteamRunning;
            bool _isSteamValid;
            Process _launchedGame;
            GameLaunchSpec _spec;
            bool _steamError;
            string _steamExePath;
            //public GameLauncher(IShutdownHandler shutdownHandler, IRestarter restarter)
            //{
            //  _shutdownHandler = shutdownHandler;
            //_restarter = restarter;
            //}

            public int LaunchGame(GameLaunchSpec spec) {
                Contract.Requires<ArgumentNullException>(spec != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(spec.GamePath));
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(spec.WorkingDirectory));

                _steamError = false;
                _isSteamRunning = false;
                _spec = spec;

                PrepareSteamState();
                if (_spec.LegacyLaunch && _isSteamGameAndAvailable)
                    LegacySteamLaunch();
                else
                    ModernLaunch();

                using (_launchedGame) {
                    if (_launchedGame == null)
                        return -1;
                    PostProcessLaunch();
                    return _launchedGame.Id;
                }
            }

            void PostProcessLaunch() {
                SetPriority();
                if (_spec.Affinity != null && _spec.Affinity.Any())
                    SetAffinity();
            }

            void SetAffinity() {
                try {
                    Tools.Processes.SetAffinity(_launchedGame, _spec.Affinity);
                } catch (Win32Exception ex) {
                    if (ex.NativeErrorCode != Win32ErrorCodes.ACCESS_DENIED)
                        throw;
                    MainLog.Logger.WarnException("Access denied while trying to set process affinity", ex);
                } catch (InvalidOperationException ex) {
                    MainLog.Logger.WarnException("Error while trying to set process affinity", ex);
                }
            }

            void SetPriority() {
                try {
                    _launchedGame.PriorityClass = _spec.Priority;
                } catch (Win32Exception ex) {
                    if (ex.NativeErrorCode != Win32ErrorCodes.ACCESS_DENIED)
                        throw;
                    MainLog.Logger.WarnException("Access denied while trying to set process priority", ex);
                } catch (InvalidOperationException ex) {
                    MainLog.Logger.WarnException("Error while trying to set process priority", ex);
                }
            }

            void PrepareSteamState() {
                if (!string.IsNullOrWhiteSpace(_spec.SteamPath))
                    _steamExePath = Path.Combine(_spec.SteamPath, SteamInfos.SteamExecutable);
                _isSteamGameAndAvailable = _spec.SteamID != 0 && !string.IsNullOrWhiteSpace(_steamExePath) &&
                                           File.Exists(_steamExePath);
            }

            void LegacySteamLaunch() {
                PrepareLegacySteamLaunch();
                StartGame();
            }

            void PrepareLegacySteamLaunch() {
                _gameStartInfo = new ProcessStartInfo {
                    FileName = _steamExePath,
                    Arguments = "-applaunch " + _spec.SteamID + " " + _spec.Arguments,
                    UseShellExecute = true
                };
            }

            void ModernLaunch() {
                StartSteamIfRequired();
                ValidateSteamRunning();
                PrepareModernGameLaunch();
                StartGame();
                //InjectSteamOverlayIfNeeded();
            }

            void PrepareModernGameLaunch() {
                _gameStartInfo = new ProcessStartInfo {
                    FileName = _spec.GamePath,
                    Arguments = _spec.Arguments,
                    UseShellExecute = true
                };

                if (!String.IsNullOrWhiteSpace(_spec.WorkingDirectory))
                    _gameStartInfo.WorkingDirectory = _spec.WorkingDirectory;

                if (!_isSteamValid)
                    return;
                _gameStartInfo.UseShellExecute = false; // This breaks UAC prompts if the game is set to require UAC.
                _gameStartInfo.EnvironmentVariables["SteamAppID"] = Convert.ToString(_spec.SteamID);
                _gameStartInfo.EnvironmentVariables["SteamGameID"] = Convert.ToString(_spec.SteamID);
            }

            void StartSteamIfRequired() {
                if (!_isSteamGameAndAvailable)
                    return;
                TryDetectSteamRunning();
                if (!IsSteamStartRequired())
                    return;
                StartSteam();
            }

            void TryDetectSteamRunning() {
                try {
                    DetectSteamRunningByMainModule();
                } catch (Exception ex1) {
                    MainLog.Logger.FormattedWarnException(ex1, "Steam detection by main module failed");
                    try {
                        DetectSteamRunning();
                    } catch (Exception ex2) {
                        MainLog.Logger.FormattedWarnException(ex2, "Steam detection failed");
                        _steamError = true;
                    }
                }
            }

            void DetectSteamRunningByMainModule() {
                var processes = GetSteamProcesses();
                using (new CompositeDisposable(processes))
                    _isSteamRunning = processes.Any(IsSteamMainModule);
            }

            bool IsSteamMainModule(Process p) {
                return p.MainModule.FileName.Equals(_steamExePath, StringComparison.InvariantCultureIgnoreCase);
            }

            void DetectSteamRunning() {
                var processes = GetSteamProcesses();
                using (new CompositeDisposable(processes))
                    _isSteamRunning = processes.Any();
            }

            static Process[] GetSteamProcesses() {
                return Tools.Processes.FindProcess(SteamInfos.SteamExecutable);
            }

            bool IsSteamStartRequired() {
                return !_isSteamRunning && !_steamError;
            }

            void StartSteam() {
                try {
                    TryStartSteam();
                } catch (Exception) {
                    _steamError = true;
                }
            }

            void TryStartSteam() {
                using (Launch(new ProcessStartInfo {
                    FileName = _steamExePath,
                    UseShellExecute = true,
                    Arguments = "-forceservice"
                })) {}
                WaitForSteamProcess();
            }

            void WaitForSteamProcess() {
                var tries = 0;
                while (!_isSteamRunning && tries < 10) {
                    if (Tools.Processes.FindProcess(SteamInfos.SteamServiceExecutable).Any())
                        _isSteamRunning = true;

                    Thread.Sleep(1000);
                    tries++;
                }
            }

            void ValidateSteamRunning() {
                MainLog.Logger.Info("Steam Info: {0}, {1}, {2}", _isSteamGameAndAvailable, _isSteamRunning, _steamError);
                _isSteamValid = _isSteamGameAndAvailable && _isSteamRunning && !_steamError;
            }

            void StartGame() {
                if (_spec.BypassUAC)
                    LaunchGameWithUacBypass();
                else
                    LaunchGameWithoutUacBypass();

                if (_launchedGame == null)
                    return;
                TrySetForeground(_launchedGame);
            }

            void LaunchGameWithUacBypass() {
                _launchedGame =
                    Process.GetProcessById(LaunchWithUacBypass(_gameStartInfo));
            }

            int LaunchWithUacBypass(ProcessStartInfo gameStartInfo) {
                MainLog.Logger.Info("LaunchingAsUser {0} from {1} with {2}", gameStartInfo.FileName,
                    gameStartInfo.WorkingDirectory, gameStartInfo.Arguments);
                return ServiceStartProcess.StartProcessAndBypassUAC(gameStartInfo);
            }

            void LaunchGameWithoutUacBypass() {
                try {
                    _launchedGame = Launch(_gameStartInfo);
                } catch (Win32Exception ex) {
                    if (!Tools.Processes.Uac.CheckUac())
                        throw;
                    throw new NotSupportedException("this is currently not supported", ex);
                    //_restarter.RestartWithUacInclEnvironmentCommandLine();
                    //_launchedGame = null;
                }
            }

            static void TrySetForeground(Process launchedGame) {
                try {
                    Tools.ProcessesTools.NativeMethods.SetForeground(launchedGame);
                } catch (Exception e) {
                    MainLog.Logger.FormattedWarnException(e);
                }
            }

            void InjectSteamOverlayIfNeeded() {
                if (_spec.SteamDRM || !_isSteamValid)
                    return;

                Environment.SetEnvironmentVariable("SteamAppID", Convert.ToString(_spec.SteamID));
                Environment.SetEnvironmentVariable("SteamGameID", Convert.ToString(_spec.SteamID));

                //TryInject();
                Launch(new ProcessStartInfo(Path.Combine(_spec.SteamPath, "GameOverlayUI.exe"),
                    "-pid " + _launchedGame.Id));

                //if (_spec.SteamID != 0)
                //  TrySteamApiInit();
            }

            static Process Launch(ProcessStartInfo processStartInfo) {
                MainLog.Logger.Info("Launching {0} from {1} with {2}", processStartInfo.FileName,
                    processStartInfo.WorkingDirectory, processStartInfo.Arguments);
                return Process.Start(processStartInfo);
            }

            /*
            void TryInject()
            {
                try {
                    var injector = new Injector(_launchedGame);
                    injector.InjectLibrary(Path.Combine(_spec.SteamPath, "GameOverlayRenderer.dll"));
                }
                catch (Win32Exception)
                {
                    TryInject64(injector);
                }
            }

            void TryInject64()
            {
                try
                {
                    injector.InjectLibrary(Path.Combine(_spec.SteamPath,
                        "GameOverlayRenderer64.dll"));
                }
                catch (Win32Exception e)
                {
                    MainLog.Logger.FormattedWarnException(e);
                }
            }
            */

            /*
            void TrySteamApiInit()
            {
                try
                {
                    SteamAPI.Init();
                }
                catch (Exception e)
                {
                    MainLog.Logger.FormattedWarnException(e);
                    MainLog.Logger.Warn("Abnormal termination - SteamAPI Failed to Initialize");
                    _shutdownHandler.Shutdown(1);
                }
            }
            */

            public class GameLaunchSpec
            {
                public string Arguments { get; set; }
                public bool BypassUAC { get; set; }
                public string GamePath { get; set; }
                public bool LegacyLaunch { get; set; }
                public bool SteamDRM { get; set; }
                public int SteamID { get; set; }
                public string SteamPath { get; set; }
                public string WorkingDirectory { get; set; }
                public ProcessPriorityClass Priority { get; set; }
                public int[] Affinity { get; set; }
            }

            public static class ServiceStartProcess
            {
                public static int StartProcessAndBypassUAC(ProcessStartInfo startInfo) {
                    uint winlogonPid = 0;
                    IntPtr hUserTokenDup = IntPtr.Zero, hPToken = IntPtr.Zero, hProcess = IntPtr.Zero;
                    var procInfo = new PROCESS_INFORMATION();

                    // obtain the currently active session id; every logged on user in the system has a unique session id
                    var dwSessionId = WTSGetActiveConsoleSessionId();

                    // obtain the process id of the winlogon process that is running within the currently active session
                    var processes = Process.GetProcessesByName("winlogon");
                    foreach (var p in processes) {
                        if ((uint) p.SessionId == dwSessionId)
                            winlogonPid = (uint) p.Id;
                    }

                    // obtain a handle to the winlogon process
                    hProcess = OpenProcess(MAXIMUM_ALLOWED, false, winlogonPid);

                    // obtain a handle to the access token of the winlogon process
                    if (!OpenProcessToken(hProcess, TOKEN_DUPLICATE, ref hPToken)) {
                        CloseHandle(hProcess);
                        return 0;
                    }

                    // Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser
                    // I would prefer to not have to use a security attribute variable and to just 
                    // simply pass null and inherit (by default) the security attributes
                    // of the existing token. However, in C# structures are value types and therefore
                    // cannot be assigned the null value.
                    var sa = new SECURITY_ATTRIBUTES();
                    sa.Length = Marshal.SizeOf(sa);

                    // copy the access token of the winlogon process; the newly created token will be a primary token
                    if (
                        !DuplicateTokenEx(hPToken, MAXIMUM_ALLOWED, ref sa,
                            (int) SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, (int) TOKEN_TYPE.TokenPrimary,
                            ref hUserTokenDup)) {
                        CloseHandle(hProcess);
                        CloseHandle(hPToken);
                        return 0;
                    }

                    // By default CreateProcessAsUser creates a process on a non-interactive window station, meaning
                    // the window station has a desktop that is invisible and the process is incapable of receiving
                    // user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
                    // interaction with the new process.
                    var si = new STARTUPINFO();
                    si.cb = Marshal.SizeOf(si);
                    si.lpDesktop = @"winsta0\default";
                    // interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop

                    // flags that specify the priority and creation method of the process
                    const int dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;

                    // create a new process in the current user's logon session
                    var result = CreateProcessAsUser(hUserTokenDup, // client's access token
                        startInfo.FileName, // file to execute
                        startInfo.Arguments, // command line
                        ref sa, // pointer to process SECURITY_ATTRIBUTES
                        ref sa, // pointer to thread SECURITY_ATTRIBUTES
                        false, // handles are not inheritable
                        dwCreationFlags, // creation flags
                        IntPtr.Zero, // pointer to new environment block 
                        startInfo.WorkingDirectory, // name of current directory 
                        ref si, // pointer to STARTUPINFO structure
                        out procInfo // receives information about new process
                        );

                    // invalidate the handles
                    CloseHandle(hProcess);
                    CloseHandle(hPToken);
                    CloseHandle(hUserTokenDup);

                    return (int) (result ? procInfo.dwProcessId : 0); // return the result
                }

                #region Structures

                [StructLayout(LayoutKind.Sequential)]
                public struct PROCESS_INFORMATION
                {
                    public IntPtr hProcess;
                    public IntPtr hThread;
                    public uint dwProcessId;
                    public uint dwThreadId;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct SECURITY_ATTRIBUTES
                {
                    public int Length;
                    public IntPtr lpSecurityDescriptor;
                    public bool bInheritHandle;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct STARTUPINFO
                {
                    public int cb;
                    public String lpReserved;
                    public String lpDesktop;
                    public String lpTitle;
                    public uint dwX;
                    public uint dwY;
                    public uint dwXSize;
                    public uint dwYSize;
                    public uint dwXCountChars;
                    public uint dwYCountChars;
                    public uint dwFillAttribute;
                    public uint dwFlags;
                    public short wShowWindow;
                    public short cbReserved2;
                    public IntPtr lpReserved2;
                    public IntPtr hStdInput;
                    public IntPtr hStdOutput;
                    public IntPtr hStdError;
                }

                #endregion

                #region Enumerations

                enum SECURITY_IMPERSONATION_LEVEL
                {
                    SecurityAnonymous = 0,
                    SecurityIdentification = 1,
                    SecurityImpersonation = 2,
                    SecurityDelegation = 3
                }

                enum TOKEN_TYPE
                {
                    TokenPrimary = 1,
                    TokenImpersonation = 2
                }

                #endregion

                #region Constants

                public const int TOKEN_DUPLICATE = 0x0002;
                public const uint MAXIMUM_ALLOWED = 0x2000000;
                public const int CREATE_NEW_CONSOLE = 0x00000010;

                public const int IDLE_PRIORITY_CLASS = 0x40;
                public const int NORMAL_PRIORITY_CLASS = 0x20;
                public const int HIGH_PRIORITY_CLASS = 0x80;
                public const int REALTIME_PRIORITY_CLASS = 0x100;

                #endregion

                #region Win32 API Imports

                [DllImport("kernel32.dll", SetLastError = true)]
                static extern bool CloseHandle(IntPtr hSnapshot);

                [DllImport("kernel32.dll")]
                static extern uint WTSGetActiveConsoleSessionId();

                [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true,
                    CharSet = CharSet.Ansi,
                    CallingConvention = CallingConvention.StdCall)]
                public static extern bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName,
                    String lpCommandLine,
                    ref SECURITY_ATTRIBUTES lpProcessAttributes,
                    ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags,
                    IntPtr lpEnvironment,
                    String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo,
                    out PROCESS_INFORMATION lpProcessInformation);

                [DllImport("kernel32.dll")]
                static extern bool ProcessIdToSessionId(uint dwProcessId, ref uint pSessionId);

                [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
                public static extern bool DuplicateTokenEx(IntPtr ExistingTokenHandle, uint dwDesiredAccess,
                    ref SECURITY_ATTRIBUTES lpThreadAttributes, int TokenType,
                    int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

                [DllImport("kernel32.dll")]
                static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

                [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurity]
                static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

                #endregion
            }
        }
    }

    public static class LaunchGameInfoExtensions
    {
        public static GameLauncherProcessInternal.GameLauncher.GameLaunchSpec ToLaunchSpec(this LaunchGameInfoBase info) {
            return new GameLauncherProcessInternal.GameLauncher.GameLaunchSpec {
                GamePath = info.LaunchExecutable.ToString(),
                WorkingDirectory = info.WorkingDirectory.ToString(),
                SteamPath = Common.Paths.SteamPath.ToString(),
                Arguments = info.StartupParameters.CombineParameters(),
                Priority = info.Priority,
                Affinity = info.Affinity
            };
        }

        public static GameLauncherProcessInternal.GameLauncher.GameLaunchSpec ToLaunchSpec(
            this LaunchGameWithSteamInfo info) {
            var spec = ((LaunchGameInfoBase) info).ToLaunchSpec();
            spec.SteamDRM = info.SteamDRM;
            spec.SteamID = info.SteamAppId;

            return spec;
        }

        public static GameLauncherProcessInternal.GameLauncher.GameLaunchSpec ToLaunchSpec(
            this LaunchGameWithSteamLegacyInfo info) {
            var spec = ((LaunchGameWithSteamInfo) info).ToLaunchSpec();
            spec.LegacyLaunch = true;

            return spec;
        }
    }
}