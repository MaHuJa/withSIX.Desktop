// <copyright company="SIX Networks GmbH" file="Cheat.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using AutoMapper;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Presentation.Wpf.Services;
using SN.withSIX.Core.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications
{
    /// <summary>
    ///     Globally accessible services and constants in the application layer
    ///     Cheat - Under advisement for better forms of access...
    /// </summary>
    public static class Cheat
    {
        static ICheatImpl _cheat;
        static readonly IPlatformProvider defaultPlatformProvider = new DefaultPlatformProvider();
        public static IMediator Mediator => _cheat.Mediator;
        public static IGameLockMonitor GameLockMonitor => _cheat.GameLockMonitor;
        public static IScreenOpener ScreenOpener => _cheat.ScreenOpener;
        public static IDialogManager DialogManager => _cheat.DialogManager;
        //public static IFlyoutOpener FlyoutOpener => _cheat.FlyoutOpener;

        //[Obsolete("Just for testing. Import ISettingsStorage instead")]
        //public static Settings Settings => _cheat.Settings;
        public static IPlatformProvider PlatformProvider => _cheat?.PlatformProvider ?? defaultPlatformProvider;
        public static IMessageBus MessageBus => _cheat.MessageBus;
        public static bool IsShuttingDown => Common.Flags.ShuttingDown;
        public static IConfiguration MapperConfiguration { get; set; }

        public static void SetServices(ICheatImpl cheat) {
            _cheat = cheat;
        }

        public static string WindowDisplayName(string title) {
            return title; //+ " - " + Consts.ProductTitle;
        }
    }

    public static class Consts
    {
        public const string InternalTitle = "Sync";
        public const string ProductTitle = InternalTitle;
        public const string ReleaseTitle =
#if NIGHTLY_RELEASE
                "ALPHA";
#else
#if BETA_RELEASE
                "BETA";
#else
#if MAIN_RELEASE
                null;
#else
            "DEV";
#endif
#endif
#endif
        public const string DirectorySubtitle =
#if NIGHTLY_RELEASE
                "alpha";
#else
#if BETA_RELEASE
                null;
#else
#if MAIN_RELEASE
                null;
#else
            "dev";
#endif
#endif
#endif
        public const string DirectoryTitle = ProductTitle + (DirectorySubtitle == null ? null : ("-" + DirectorySubtitle));
        public const string DisplayTitle = ProductTitle + (ReleaseTitle == null ? null : (" " + ReleaseTitle));
        public const string WindowTitle = DisplayTitle;

        public const int SrvPort = 9666;
        public const int SrvHttpPort = 9665;
        public const string SrvAddress = "127.0.0.66";

        public static string ProductVersion { get; } = CommonBase.AssemblyLoader.GetInformationalVersion();
        public static Version InternalVersion { get; } = CommonBase.AssemblyLoader.GetEntryVersion();
        public static bool IsTestVersion { get; }
#if DEBUG || NIGHTLY_RELEASE
            = true;
#else
            = false;
#endif
        public static Version NewVersionAvailable { get; set; }
        public static bool NewVersionInstalled { get; set; }
        // TODO: Consider FirstRun not just from Setup but also in terms of Settings.... so that deleting settings is a new FirstRun?
        public static bool FirstRun { get; set; }

        public static class Features
        {
            public static bool Queue => IsTestVersion;
            public static bool UnreleasedGames => IsTestVersion;
        }
    }

    public interface ICheatImpl
    {
        IMediator Mediator { get; }
        IScreenOpener ScreenOpener { get; }
        //IFlyoutOpener FlyoutOpener { get; }
        IPlatformProvider PlatformProvider { get; }
        IMessageBus MessageBus { get; }
        IGameLockMonitor GameLockMonitor { get; }
        IDialogManager DialogManager { get; }
    }

    public interface IGameLockMonitor
    {
        IObservable<bool> GetObservable(Guid id);
    }

    // NOTE: Stateful service!
    public class GameLockMonitor : IGameLockMonitor, IApplicationService
    {
        readonly IDictionary<Guid, bool> _currentValues = new Dictionary<Guid, bool>();
        readonly IDictionary<Guid, Subject<bool>> _observables = new Dictionary<Guid, Subject<bool>>();

        public GameLockMonitor(IMessageBus messageBus) {
            messageBus.Listen<GameLockChanged>()
                .Subscribe(Handle);
        }

        public IObservable<bool> GetObservable(Guid gameId) {
            var result = GetOrAdd(gameId);
            return result.Item1.StartWith(result.Item2);
        }

        Tuple<Subject<bool>, bool> GetOrAdd(Guid gameId) {
            lock (_observables) {
                if (!_observables.ContainsKey(gameId)) {
                    _observables[gameId] = new Subject<bool>();
                    _currentValues[gameId] = false;
                }
                return Tuple.Create(_observables[gameId], _currentValues[gameId]);
            }
        }

        void Handle(GameLockChanged gameLockChanged) {
            var obs = GetOrAdd(gameLockChanged.GameId);
            lock (obs) {
                _currentValues[gameLockChanged.GameId] = gameLockChanged.IsLocked;
                obs.Item1.OnNext(gameLockChanged.IsLocked);
            }
        }
    }

    public class CheatImpl : ICheatImpl, IApplicationService
    {
        public CheatImpl(IMediator mediator, IScreenOpener screenOpener, IPlatformProvider platformProvider,
            IExceptionHandler exceptionHandler, Cache.IImageFileCache imageFileCache,
            IMessageBus messageBus, IGameLockMonitor gameLockMonitor, IDialogManager dialogManager) {
            Mediator = mediator;
            ScreenOpener = screenOpener;
            PlatformProvider = platformProvider;
            MessageBus = messageBus;
            GameLockMonitor = gameLockMonitor;
            DialogManager = dialogManager;
            UiTaskHandler.SetExceptionHandler(exceptionHandler);
            Cache.ImageFiles = imageFileCache;
        }

        public IMediator Mediator { get; }
        public IScreenOpener ScreenOpener { get; }
        //public IFlyoutOpener FlyoutOpener { get;  }
        public IPlatformProvider PlatformProvider { get; }
        public IMessageBus MessageBus { get; }
        public IGameLockMonitor GameLockMonitor { get; }
        public IDialogManager DialogManager { get; }
    }
}