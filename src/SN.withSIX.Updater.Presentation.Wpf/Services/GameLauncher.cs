// <copyright company="SIX Networks GmbH" file="GameLauncher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Legacy;
using SteamWrapper;
using Syringe;

namespace SN.withSIX.Updater.Presentation.Wpf.Services
{
    public class GameLauncher
    {
        readonly IRestarter _restarter;
        readonly IShutdownHandler _shutdownHandler;
        ProcessStartInfo _gameStartInfo;
        bool _isSteamGameAndAvailable;
        bool _isSteamRunning;
        bool _isSteamValid;
        Process _launchedGame;
        GameLaunchSpec _spec;
        bool _steamError;
        string _steamExePath;

        public GameLauncher(IShutdownHandler shutdownHandler, IRestarter restarter) {
            _shutdownHandler = shutdownHandler;
            _restarter = restarter;
        }

        public int LaunchGame(GameLaunchSpec spec) {
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
            } catch (InvalidOperationException ex) {
                MainLog.Logger.WarnException("Error while trying to set process affinity", ex);
            }
        }

        void SetPriority() {
            try {
                _launchedGame.PriorityClass = _spec.Priority;
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
                Arguments = "-applaunch " + _spec.SteamID + " " + _spec.Arguments
            };
        }

        void ModernLaunch() {
            StartSteamIfRequired();
            ValidateSteamRunning();
            PrepareModernGameLaunch();
            StartGame();
            InjectSteamOverlayIfNeeded();
        }

        void PrepareModernGameLaunch() {
            _gameStartInfo = new ProcessStartInfo {
                FileName = _spec.GamePath,
                UseShellExecute = false,
                Arguments = _spec.Arguments
            };

            if (!String.IsNullOrWhiteSpace(_spec.WorkingDirectory))
                _gameStartInfo.WorkingDirectory = _spec.WorkingDirectory;

            if (!_isSteamValid)
                return;
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
            } catch (Win32Exception) {
                if (!Tools.Processes.Uac.CheckUac())
                    throw;
                _restarter.RestartWithUacInclEnvironmentCommandLine();
                _launchedGame = null;
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

            var injector = new Injector(_launchedGame);
            TryInject(injector);
            Launch(new ProcessStartInfo(Path.Combine(_spec.SteamPath, "GameOverlayUI.exe"),
                "-pid " + _launchedGame.Id));

            if (_spec.SteamID != 0)
                TrySteamApiInit();
        }

        static Process Launch(ProcessStartInfo processStartInfo) {
            MainLog.Logger.Info("Launching {0} from {1} with {2}", processStartInfo.FileName,
                processStartInfo.WorkingDirectory, processStartInfo.Arguments);
            return Process.Start(processStartInfo);
        }

        void TryInject(Injector injector) {
            try {
                injector.InjectLibrary(Path.Combine(_spec.SteamPath, "GameOverlayRenderer.dll"));
            } catch (Win32Exception) {
                TryInject64(injector);
            }
        }

        void TryInject64(Injector injector) {
            try {
                injector.InjectLibrary(Path.Combine(_spec.SteamPath,
                    "GameOverlayRenderer64.dll"));
            } catch (Win32Exception e) {
                MainLog.Logger.FormattedWarnException(e);
            }
        }

        void TrySteamApiInit() {
            try {
                SteamAPI.Init();
            } catch (Exception e) {
                MainLog.Logger.FormattedWarnException(e);
                MainLog.Logger.Warn("Abnormal termination - SteamAPI Failed to Initialize");
                _shutdownHandler.Shutdown(1);
            }
        }

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
    }
}