// <copyright company="SIX Networks GmbH" file="Common.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

#if MAIN_RELEASE
#elif BETA_RELEASE
#elif NIGHTLY_RELEASE
#define STAGING
#else
#define STAGING
#define DEV_BUILD
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using NDepend.Path;
using ReactiveUI;
using SevenZip;
using ShortBus;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using Action = System.Action;

namespace SN.withSIX.Core
{
    public enum ReleaseType
    {
        Dev,
        Alpha,
        Beta,
        Stable
    }

    public static class Common
    {
        public const string DefaultCategory = "Unknown";
        public const long MagicPingValue = 9999;
        public const string ClientHeader = "X-Six-Client";
        public const string ClientHeaderV = ClientHeader + "-V";
        public static AppCommon App;
        public static readonly StartupFlags Flags = new StartupFlags();
        public static readonly DateTime StartTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
        public static readonly string[] DefaultHosts = {
            "c1-de.sixmirror.com",
            "c1-us.sixmirror.com"
        };
        // TODO: Premium host authentication needs to be addressed more dynamically
        public static readonly string[] PremiumHosts = {
            "c1-de-p.sixmirror.com",
            "c1-us-p.sixmirror.com"
            //"c1-sg-p.sixmirror.com"
        };
        public static readonly string[] PremiumMirrors = PremiumHosts.Select(x => "zsync://" + x).ToArray();
        public static readonly string[] DefaultMirrors = DefaultHosts.Select(x => "zsync://" + x).ToArray();

        static Common() {
            App = new AppCommon();
        }

        public static PathConfiguration Paths { get; set; } = new PathConfiguration();
        public static Action OnExit { get; set; }
        public static bool IsMini { get; set; }

        public static Task GenerateDiagnosticZip(IAbsoluteFilePath path) {
            var archiveManager = new SevenZipCompressor();
            return
                Task.Run(
                    () =>
                        archiveManager.CompressFileDictionary(
                            GetLogFilesDictionary(Paths.LocalDataRootPath),
                            path.ToString()));
        }

        static Dictionary<string, string> GetLogFilesDictionary(IAbsoluteDirectoryPath path) {
            return
                path.DirectoryInfo.GetFiles("*.log", SearchOption.AllDirectories)
                    .ToDictionary(x => x.FullName.Replace(path + "\\", ""), x => x.FullName);
        }

        public class AppCommon : IEnableLogging
        {
            public const string ApplicationState = ""; //"Preview";
            public const string Classes = "Software\\Classes";
            public const int InitializationLvl = 1;
            public const string DefaultModResourceFile = "ModsPlaceholder-small40x40.png";
            public const string DefaultModResourceFileLarge = "ModsPlaceholder-full232x112.png";
            public const string DefaultModResourceFileHuge = "ModsPlaceholder-huge300x144.png";
            const string OnlineResourcePath = "https://d2l9k1uqfxdpqe.cloudfront.net/assets/mods/";
            public static string ApplicationName = "Play withSIX";
            public static string ApplicationRegKey = "Software\\SIX Networks\\" + ApplicationName; // TODO
            static readonly bool debug;
            static readonly bool trace;
            public static readonly TimeSpan DefaultFilterDelay = TimeSpan.FromMilliseconds(250);
            public static bool IsBusy;
            public static readonly ReleaseType Type =
#if BETA_RELEASE
                ReleaseType.Beta;
#else
#if MAIN_RELEASE
                ReleaseType.Stable;
#else
#if NIGHTLY_RELEASE
                ReleaseType.Alpha;
#else
                ReleaseType.Dev;
#endif
#endif
#endif
            public static readonly string TitleType = Type == ReleaseType.Stable ? "" : " " + Type.ToString().ToUpper();

            static AppCommon() {
#if DEBUG
                debug = true;
#endif
#if TRACE
                trace = true;
#endif
            }

            [Obsolete("Take a dependency on IEventAggregator instead")]
            public IEventAggregator Events { get; set; }
            public Version ApplicationVersion { get; private set; }
            public string ProductVersion { get; private set; }
            public string AppTitle { get; private set; }
            [Obsolete("Workaround for need to check for Wpf Application availability in Core")]
            public bool IsWpfApp { get; set; }
            public IMediator Mediator { get; set; }

            public void PublishEvent<T>(T message) {
                Events.PublishOnCurrentThread(message);
            }

            [Obsolete("This is only here because of lack of better approach to raising domain events currently")]
            public void PublishDomainEvent<TEvent>(TEvent evt) where TEvent : IDomainEvent {
                //Events.PublishOnCurrentThread(evt);
                Mediator.Notify(evt);
            }

            [Obsolete("This is only here because of legacy code")]
            public TResult Request<TResult>(IRequest<TResult> evt) {
                //Events.PublishOnCurrentThread(evt);
                return Mediator.Request(evt);
            }

            public void Init(string appName) {
                RxApp.SupportsRangeNotifications = false; // WPF doesnt :/
                if (Flags.Staging || Flags.Portable)
                    InitLocal(appName);
                else
                    InitInternal(appName);
            }

            void InitLocal(string appName) {
                var tup = GetLocalPathData(CommonBase.AssemblyLoader.GetEntryPath().ToAbsoluteDirectoryPath());
                InitInternal(appName, tup.Item1, tup.Item2, tup.Item3, tup.Item4, null, null, tup.Item5);
            }

            public void InitLocalWithCleanup(string appName) {
                var tup = GetLocalPathData(CommonBase.AssemblyLoader.GetEntryPath().ToAbsoluteDirectoryPath());
                CleanPaths(tup.Item2, tup.Item3, tup.Item4, tup.Item5);
                InitInternal(appName, tup.Item1, tup.Item2, tup.Item3, tup.Item4, null, null, tup.Item5);
            }

            static
                Tuple
                    <IAbsoluteDirectoryPath, IAbsoluteDirectoryPath, IAbsoluteDirectoryPath, IAbsoluteDirectoryPath,
                        IAbsoluteDirectoryPath> GetLocalPathData(IAbsoluteDirectoryPath localBasePath) {
                var dataDir = localBasePath.GetChildDirectoryWithName("Data");
                return Tuple.Create(localBasePath, dataDir.GetChildDirectoryWithName("RoamingData"),
                    dataDir.GetChildDirectoryWithName("LocalData"),
                    dataDir.GetChildDirectoryWithName("Temp"),
                    dataDir.GetChildDirectoryWithName("LocalData SIX Networks").GetChildDirectoryWithName("Shared"));
            }

            static void CleanPaths(params IAbsoluteDirectoryPath[] paths) {
                foreach (var path in paths) {
                    if (path.Exists)
                        Tools.FileUtil.Ops.DeleteWithRetry(path.ToString());
                    path.MakeSurePathExists();
                }
            }

            public static void ClearAwesomiumCache() {
                var cachePath = Paths.AwesomiumPath.GetChildDirectoryWithName("Cache");
                if (cachePath.Exists)
                    Tools.FileUtil.Ops.DeleteDirectory(cachePath);
                Paths.AwesomiumPath.MakeSurePathExists();
            }

            public static void ClearAwesomium() {
                var path = Paths.AwesomiumPath;
                if (path.Exists)
                    Tools.FileUtil.Ops.DeleteDirectory(path);
                Paths.AwesomiumPath.MakeSurePathExists();
            }

            void InitInternal(string appName = null,
                IAbsoluteDirectoryPath appPath = null, IAbsoluteDirectoryPath dataPath = null,
                IAbsoluteDirectoryPath localDataPath = null, IAbsoluteDirectoryPath tempPath = null,
                IAbsoluteDirectoryPath configPath = null,
                IAbsoluteDirectoryPath toolPath = null, IAbsoluteDirectoryPath sharedDataPath = null) {
                HandleAppInfo(appName);
                HandlePaths(appPath, dataPath, localDataPath, tempPath, configPath, toolPath, sharedDataPath);
                ShowInfo();
            }

            void HandleAppInfo(string appName) {
                if (appName == null)
                    appName = ApplicationName;
                AppTitle = appName;
                ApplicationVersion = CommonBase.AssemblyLoader.GetEntryVersion();
                ProductVersion = CommonBase.AssemblyLoader.GetProductVersion();
            }

            static void HandlePaths(IAbsoluteDirectoryPath appPath, IAbsoluteDirectoryPath dataPath,
                IAbsoluteDirectoryPath localDataPath, IAbsoluteDirectoryPath tempPath,
                IAbsoluteDirectoryPath configPath,
                IAbsoluteDirectoryPath toolPath,
                IAbsoluteDirectoryPath sharedDataPath) {
                if (!Paths.PathsSet)
                    Paths.SetPaths(appPath, dataPath, localDataPath, tempPath, configPath, toolPath, sharedDataPath);

                if (!Paths.PathsSet)
                    throw new Exception("Paths not set yet!");

                Paths.DataPath.MakeSurePathExists();
                Paths.LocalDataPath.MakeSurePathExists();
                Paths.LogPath.MakeSurePathExists();
                Paths.TempPath.MakeSurePathExists();
            }

            void ShowInfo() {
                var elevated = TryGetElevatedStatus();

                this.Logger().Info("{0}, {1}, DEBUG: {2}, TRACE: {3}\n"
                                   + "OS: {4} 64-bit: {5}, .NET: {6}, CULT: {7}, Elevated: {8}, DefaultEncoding: {9}\n"
                                   + "Current dir: {10}\n"
                                   + "App: {11}\n"
                                   + "Data: {12}, LocalData: {13}\n"
                                   + "Arguments: {14}",
                    CommonBase.AssemblyLoader.GetEntryVersion(), TitleType,
                    debug, trace, Environment.OSVersion.Version, Environment.Is64BitOperatingSystem,
                    Environment.Version, CultureInfo.CurrentCulture, elevated, Encoding.Default,
                    Tools.FileUtil.FilterPath(Directory.GetCurrentDirectory()),
                    Tools.FileUtil.FilterPath(Paths.AppPath), Tools.FileUtil.FilterPath(Paths.DataPath),
                    Tools.FileUtil.FilterPath(Paths.LocalDataPath),
                    Tools.FileUtil.FilterPath(Flags.FullStartupParameters.CombineParameters()));
            }

            bool TryGetElevatedStatus() {
                var elevated = false;
                try {
                    elevated = Tools.Processes.Uac.IsProcessElevated();
                } catch (Exception e) {
                    this.Logger().FormattedDebugException(e);
                }
                return elevated;
            }

            public string GetResourcePath(string resource) {
                Contract.Requires<ArgumentNullException>(resource != null);

                if (resource.StartsWith("//"))
                    resource = "http:" + resource;

                if (Uri.IsWellFormedUriString(resource, UriKind.Absolute))
                    return resource;
                return OnlineResourcePath + resource;
            }
        }

        public class StartupFlags
        {
            public StartupFlags() {
                Merged = CommonBase.IsMerged();

                var staging = false;
                var verbose = false;
                var autoUpdateEnabled = Merged;
                var lockDown = false;
                var pars = Tools.Generic.GetStartupParameters().ToArray();
                FullStartupParameters = pars;
                if (pars.Contains("--staging")) {
                    staging = true;
                    pars = pars.Where(x => !x.Equals("--staging")).ToArray();
                }

                if (pars.Contains("--portable")) {
                    Portable = true;
                    pars = pars.Where(x => !x.Equals("--portable")).ToArray();
                }

                if (pars.Contains("--verbose")) {
                    verbose = true;
                    pars = pars.Where(x => !x.Equals("--verbose")).ToArray();
                }

                if (pars.Contains("--public"))
                    Public = true;

                if (pars.Contains("--production"))
                    UseProduction = true;

                if (pars.Contains("--skip-autoupdate")) {
                    autoUpdateEnabled = false;
                    pars = pars.Where(x => !x.Equals("--skip-autoupdate")).ToArray();
                }

                if (pars.Contains("--ignore-error-dialogs")) {
                    IgnoreErrorDialogs = true;
                    pars = pars.Where(x => !x.Equals("--ignore-error-dialogs")).ToArray();
                }

                if (pars.Contains("--skip-execution-confirmation")) {
                    SkipExecutionConfirmation = true;
                    pars = pars.Where(x => !x.Equals("--skip-execution-confirmation")).ToArray();
                }

                string lockDownModSet = null;
                var ld = pars.FirstOrDefault(x => x.Contains("lockdown=true"));
                if (ld != null) {
                    pars = pars.Where(x => !x.Equals(ld)).ToArray();
                    var dic = Tools.Transfer.GetDictionaryFromQueryString(ld);
                    lockDown = true;
                    lockDownModSet = dic["mod_set"];
                }

                var selfUpdateSupported = Merged;

                StartupParameters = pars;

                /*
#if DEBUG
                                UseLocalServer = true;
#endif
*/

#if DEV_BUILD
                autoUpdateEnabled = false;
                selfUpdateSupported = false;
#endif

#if MAIN_RELEASE
                IsStable = true;
#endif

#if DEBUG || (!MAIN_RELEASE && !BETA_RELEASE)
                // This is used also to determine if the local withSIX-Updater/SelfUpdater should be used...
                IsInternal = true;
#endif

#if STAGING
                staging = true;
                verbose = true;
#endif
                Staging = UseProduction ? false : staging;
                Verbose = verbose;
                AutoUpdateEnabled = autoUpdateEnabled;
                LockDown = lockDown;
                LockDownModSet = lockDownModSet;
                SelfUpdateSupported = selfUpdateSupported;
            }

            bool UseProduction { get; }
            public bool Public { get; set; }
            public bool SkipExecutionConfirmation { get; private set; }
            public bool IsInternal { get; private set; }
            public bool IsStable { get; private set; }
            public bool IgnoreErrorDialogs { get; private set; }
            public bool SelfUpdateSupported { get; private set; }
            /// <summary>
            ///     Startup parameters left over after processing low level startup parameters. Use FullStartupParameters if you need
            ///     all of them instead
            /// </summary>
            public string[] StartupParameters { get; private set; }
            /// <summary>
            ///     All startup parameters, unfiltered.
            /// </summary>
            public string[] FullStartupParameters { get; }
            public bool Merged { get; }
            public string LockDownModSet { get; private set; }
            public bool AutoUpdateEnabled { get; private set; }
            public bool LockDown { get; private set; }
            public bool Verbose { get; private set; }
            public bool Staging { get; }
            public bool UseLocalServer { get; set; }
            public bool ShuttingDown { get; set; }
            public bool UseElevatedService { get; set; }
            public bool Portable { get; }
        }
    }

    public interface IDomainEvent {}

    public static class GameUUids
    {
        public const string Arma1 = "1781B5A8-7C05-4B5D-8EC3-62C14DA42B5B";
        public const string Arma2Oa = "B4EC1290-CECD-45A3-977F-136C5929E51C";
        public const string Arma2 = "FA5A2E52-D760-4B12-ACBD-07FD35FF12E8";
        public const string Arma2Co = "1947DE55-44ED-4D92-A62F-26CFBE48258B";
        public const string Arma2Free = "FA5A2B52-D770-4B12-ACBD-07FD35CB12E8";
        public const string Arma3 = "9DE199E3-7342-4495-AD18-195CF264BA5B";
        public const string IronFront = "3EBAA36D-CA5C-4C08-B4A8-3B1CAE423F65";
        public const string TKOH = "C24098DC-4EFC-443D-B1A4-3FA69512780A";
        public const string TKOM = "AE3D9D2D-F09B-4E2F-BF1A-89D9C6BA4FA9";
        public const string DayZSA = "E3EB8AC2-AE32-41D0-8B34-E936A20B31A6";
        public const string Homeworld2 = "E65A1906-F0CB-4B78-835E-E367E6DB6962";
        public const string CarrierCommand = "6A1D6219-F47C-4EEB-BE3E-D8E39BF89FE0";
        public const string KerbalSP = "A980BC60-74E1-46D9-9F3D-8BB695A21B69";
        public const string GTAV = "BE87E190-6FA4-4C96-B604-0D9B08165CC5";
        public const string GTAIV = "8BA4D622-2A91-4149-9E06-EF40DF4E2DCB";
        public const string Witcher3 = "5137A2FB-1A8D-4DA8-97F6-65F88042E4D6";
    }

    public static class GameUuids
    {
        public static readonly Guid Arma1 = new Guid(GameUUids.Arma1);
        public static readonly Guid Arma2 = new Guid(GameUUids.Arma2);
        public static readonly Guid Arma2Free = new Guid(GameUUids.Arma2Free);
        public static readonly Guid Arma2Oa = new Guid(GameUUids.Arma2Oa);
        public static readonly Guid Arma2Co = new Guid(GameUUids.Arma2Co);
        public static readonly Guid Arma3 = new Guid(GameUUids.Arma3);
        public static readonly Guid IronFront = new Guid(GameUUids.IronFront);
        public static readonly Guid TKOH = new Guid(GameUUids.TKOH);
        public static readonly Guid TKOM = new Guid(GameUUids.TKOM);
        public static readonly Guid DayZSA = new Guid(GameUUids.DayZSA);
        public static readonly Guid Homeworld2 = new Guid(GameUUids.Homeworld2);
        public static readonly Guid CarrierCommand = new Guid(GameUUids.CarrierCommand);
        public static readonly Guid KerbalSP = new Guid(GameUUids.KerbalSP);
        public static readonly Guid GTAV = new Guid(GameUUids.GTAV);
        public static readonly Guid GTAIV = new Guid(GameUUids.GTAIV);
    }
}