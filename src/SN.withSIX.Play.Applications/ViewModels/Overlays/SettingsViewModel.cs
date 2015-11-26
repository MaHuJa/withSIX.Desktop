// <copyright company="SIX Networks GmbH" file="SettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using MahApps.Metro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.ViewModels.Popups;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Sync.Core.Transfer;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SN.withSIX.Play.Applications.ViewModels.Overlays
{
    public static class SettingsCategories
    {
        public const string Notifications = "Notifications";
        public const string Other = "Other";
        public const string Transfers = "Transfers";
        public const string ServerBrowser = "Server Browser";
        public const string Behavior = "Behavior";
        public const string DisabledDialogs = "Disabled dialogs";
        public const string DisabledDialogsAutoAnswers = "Disabled dialogs: auto-answers";
        public const string SynQ = "SynQ";
    }

    public class AccentColorMenuData : AccentColorMenuDataBase
    {
        ICommand changeAccentCommand;
        public ICommand ChangeAccentCommand
        {
            get
            {
                return changeAccentCommand ??
                       (changeAccentCommand =
                           new SimpleCommand {CanExecuteDelegate = x => true, ExecuteDelegate = x => DoChangeTheme(x)});
            }
        }

        protected virtual void DoChangeTheme(object sender) {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender) {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
    }

    [DoNotObfuscate]
    public class SettingsViewModel : OverlayViewModelBase, ISingleton
    {
        const string _name = "Game Settings";
        static string[] _locales = UserSettings.Locales;
        static int[] _maxThreadsEntries = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12};
        static int[] _minPlayersEntries = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 25, 50, 100};
        static int[] _minSlotsEntries = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 25, 50};
        readonly IDialogManager _dialogManager;
        readonly UserSettings _settings;
        SettingsViewModel _propertyGrid;
        string _steamDirectory;

        public SettingsViewModel(UserSettings settings, IDialogManager dialogManager, IRestarter restarter) {
            _dialogManager = dialogManager;
            DisplayName = "Settings";
            _settings = settings;

            // create accent color menu items for the demo
            Accents = ThemeInfo.Accents
                .Select(
                    a => new AccentColorMenuData {Name = a.Name, ColorBrush = a.ColorBrush}).ToList();

            // create metro theme color menu items for the demo
            Themes = ThemeInfo.Themes
                .Select(
                    a =>
                        new AppThemeMenuData {
                            Name = a.Name,
                            BorderColorBrush = a.BorderColorBrush,
                            ColorBrush = a.ColorBrush
                        }).ToList();

            DiagnosticsMenu = new DiagnosticsMenu(_dialogManager, restarter);

            // Task.Run because RXCommand does not throw exceptions in Subscribe.
            ReactiveCommand.CreateAsyncTask(x => Task.Run(() => Logout()))
                .SetNewCommand(this, x => x.LogoutCommand)
                .Subscribe();
            ReactiveCommand.CreateAsyncTask(
                x => Common.App.Events.PublishOnUIThreadAsync(new RequestGameSettingsOverlay(Guid.Empty)))
                .SetNewCommand(this, x => x.GameSettingsCommand)
                .Subscribe();

            _settings.WhenAnyValue(x => x.GameOptions.SteamDirectory)
                .BindTo(this, x => x.SteamDirectory);

            this.WhenAnyValue(x => x.SteamDirectory)
                .Select(x => x == DefaultSteamDirectory ? null : x)
                .BindTo(_settings.GameOptions, x => x.SteamDirectory);

            this.WhenAnyValue(x => x.DefaultSteamDirectory)
                .Where(x => string.IsNullOrWhiteSpace(SteamDirectory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { SteamDirectory = x; });

            SetupPropertyGrid();
        }

        protected SettingsViewModel() {}
        [Browsable(false)]
        public DiagnosticsMenu DiagnosticsMenu { get; private set; }
        [Browsable(false)]
        public List<AppThemeMenuData> Themes { get; private set; }
        [Browsable(false)]
        public List<AccentColorMenuData> Accents { get; private set; }
        [Category(GameSettingCategories.Directories)]
        [DisplayName("Steam Directory")]
        [Description("The path to Steam")]
        public string SteamDirectory
        {
            get { return _steamDirectory; }
            set { SetProperty(ref _steamDirectory, value); }
        }
        string DefaultSteamDirectory
        {
            get
            {
                var absoluteDirectoryPath = Common.Paths.SteamPath;
                return absoluteDirectoryPath == null ? null : absoluteDirectoryPath.ToString();
            }
        }
        [Browsable(false)]
        public SettingsViewModel PropertyGrid
        {
            get { return _propertyGrid; }
            set { SetProperty(ref _propertyGrid, value); }
        }
        [Browsable(false)]
        public string Name
        {
            get { return _name; }
        }
        [Browsable(false)]
        public GameSettingsController GameSettingsController
        {
            get { return _settings.GameOptions.GameSettingsController; }
        }
        [Browsable(false)]
        public ReactiveCommand<Unit> LogoutCommand { get; private set; }
        [Browsable(false)]
        public ReactiveCommand<Unit> GameSettingsCommand { get; private set; }
        [Browsable(false)]
        public int[] MinPlayersEntries
        {
            get { return _minPlayersEntries; }
            set { SetProperty(ref _minPlayersEntries, value); }
        }
        [Browsable(false)]
        public int[] MinSlotsEntries
        {
            get { return _minSlotsEntries; }
            set { SetProperty(ref _minSlotsEntries, value); }
        }
        [Browsable(false)]
        public int[] MaxThreadsEntries
        {
            get { return _maxThreadsEntries; }
            set { SetProperty(ref _maxThreadsEntries, value); }
        }
        [DisplayName("Participate in the Customer Experience Program")]
        [Description("By participating you help us improve the performance, reliability and functionality, anonymously")
        ]
        [Category(SettingsCategories.Other)]
        public bool ParticipateInCustomerExperienceProgram
        {
            get { return _settings.AppOptions.ParticipateInCustomerExperienceProgram; }
            set
            {
                if (_settings.AppOptions.ParticipateInCustomerExperienceProgram == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.ParticipateInCustomerExperienceProgram = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Protocol Preference")]
        [Category(SettingsCategories.Transfers)]
        [Description("Which protocol is preferred. 'Any' is recommended unless issues occur.")]
        public ProtocolPreference ProtocolPreference
        {
            get { return _settings.AppOptions.ProtocolPreference; }
            set
            {
                if (_settings.AppOptions.ProtocolPreference == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.ProtocolPreference = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Max Concurrent Transfers")]
        [Category(SettingsCategories.Transfers)]
        [Description("Maximum allowed concurrent transfers. Limited by the used network.")]
        public int MaxThreads
        {
            get { return _settings.AppOptions.MaxThreads ?? 6; }
            set
            {
                int? val = value;
                if (val == 6)
                    val = null;

                if (_settings.AppOptions.MaxThreads == val)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.MaxThreads = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Keep compressed files")]
        [Category(SettingsCategories.Transfers)]
        [Description(
            "Maximum speed, maximum repairability. But also uses more disk-space. If you want to save space but don't mind a bit slower updates, you can disable this setting"
            )]
        public bool KeepCompressedFiles
        {
            get { return _settings.AppOptions.KeepCompressedFiles; }
            set
            {
                if (_settings.AppOptions.KeepCompressedFiles == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.KeepCompressedFiles = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Behavior)]
        [DisplayName("Auto switch to grid view mode")]
        [Description("Automatically switch between List and Grid mode when enough Width available")]
        public bool AutoAdjustLibraryViewType
        {
            get { return _settings.AppOptions.AutoAdjustLibraryViewType; }
            set
            {
                if (_settings.AppOptions.AutoAdjustLibraryViewType == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.AutoAdjustLibraryViewType = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Other)]
        [DisplayName("Disable EasterEggs")]
        public bool DisableEasterEggs
        {
            get { return _settings.AppOptions.DisableEasterEggs; }
            set
            {
                if (_settings.AppOptions.DisableEasterEggs == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.DisableEasterEggs = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.ServerBrowser)]
        [DisplayName("Auto Process Server Apps")]
        public bool AutoProcessServerApps
        {
            get { return _settings.ServerOptions.AutoProcessServerApps; }
            set
            {
                if (_settings.ServerOptions.AutoProcessServerApps == value)
                    return;
                //OnPropertyChanging();
                _settings.ServerOptions.AutoProcessServerApps = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Behavior)]
        [DisplayName("Auto Process Mod Apps (e.g TS3 plugins)")]
        public bool AutoProcessModApps
        {
            get { return _settings.ModOptions.AutoProcessModApps; }
            set
            {
                if (_settings.ModOptions.AutoProcessModApps == value)
                    return;
                //OnPropertyChanging();
                _settings.ModOptions.AutoProcessModApps = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Suspend sync while Game running")]
        [Description("Suspend syncing servers or api info when Game is running")]
        [Category(SettingsCategories.Behavior)]
        public bool SuspendSyncWhileGameRunning
        {
            get { return _settings.AppOptions.SuspendSyncWhileGameRunning; }
            set
            {
                if (_settings.AppOptions.SuspendSyncWhileGameRunning == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.SuspendSyncWhileGameRunning = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Server list auto refresh time")]
        [Category(SettingsCategories.ServerBrowser)]
        public long AutoRefreshServersTime
        {
            get { return _settings.AppOptions.AutoRefreshServersTime; }
            set
            {
                if (_settings.AppOptions.AutoRefreshServersTime == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.AutoRefreshServersTime = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Other)]
        [DisplayName("Show dialog when update downloaded")]
        [Description("Enable to show dialog with options to install software update when downloaded")]
        public bool ShowDialogWhenUpdateDownloaded
        {
            get { return _settings.AppOptions.ShowDialogWhenUpdateDownloaded; }
            set
            {
                if (_settings.AppOptions.ShowDialogWhenUpdateDownloaded == value)
                    return;
                _settings.AppOptions.ShowDialogWhenUpdateDownloaded = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Other)]
        [DisplayName("Receive Beta updates")]
        [Description("Enable to receive Beta updates with latest improvements. Please report bugs")]
        public bool EnableBetaUpdates
        {
            get { return _settings.AppOptions.EnableBetaUpdates; }
            set
            {
                if (_settings.AppOptions.EnableBetaUpdates == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.EnableBetaUpdates = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Transfers)]
        [DisplayName("HTTP Proxy")]
        [Description("Used for Zsync. Only specify when required in your network topology")]
        public string HttpProxy
        {
            get { return _settings.AppOptions.HttpProxy; }
            set
            {
                if (_settings.AppOptions.HttpProxy == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.HttpProxy = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.Behavior)]
        [DisplayName("Launch with Windows")]
        public bool LaunchWithWindows
        {
            get { return _settings.AppOptions.LaunchWithWindows; }
            set
            {
                if (_settings.AppOptions.LaunchWithWindows == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.LaunchWithWindows = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Use elevated service to minimize UAC")]
        [Category(SettingsCategories.Behavior)]
        public bool UseElevatedService
        {
            get { return _settings.AppOptions.UseElevatedService; }
            set
            {
                if (_settings.AppOptions.UseElevatedService == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.UseElevatedService = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Closing minimizes to tray")]
        [Category(SettingsCategories.Behavior)]
        [ReadOnly(true)]
        public bool CloseMeansMinimize
        {
            get { return _settings.AppOptions.CloseMeansMinimize; }
            set
            {
                if (_settings.AppOptions.CloseMeansMinimize == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.CloseMeansMinimize = value;

                if (value)
                    _settings.AppOptions.FirstTimeMinimizedToTray = true;

                OnPropertyChanged();
            }
        }
        [DisplayName("Enable tray icon")]
        [Category(SettingsCategories.Behavior)]
        public bool EnableTrayIcon
        {
            get { return _settings.AppOptions.EnableTrayIcon; }
            set
            {
                if (_settings.AppOptions.EnableTrayIcon == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.EnableTrayIcon = value;

                if (!value)
                    CloseMeansMinimize = false;

                EnableDisableOption("CloseMeansMinimize", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Closing closes current overlay")]
        [Category(SettingsCategories.Behavior)]
        public bool CloseCurrentOverlay
        {
            get { return _settings.AppOptions.CloseCurrentOverlay; }
            set
            {
                if (_settings.AppOptions.CloseCurrentOverlay == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.CloseCurrentOverlay = value;

                OnPropertyChanged();
            }
        }
        [DisplayName("Close after Game launch")]
        [Category(SettingsCategories.Behavior)]
        public bool CloseAfterLaunch
        {
            get { return _settings.GameOptions.CloseOnLaunch; }
            set
            {
                if (_settings.GameOptions.CloseOnLaunch == value)
                    return;
                //OnPropertyChanging();
                _settings.GameOptions.CloseOnLaunch = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Minimize after Game launch")]
        [Category(SettingsCategories.Behavior)]
        public bool MinimizeAfterLaunch
        {
            get { return _settings.GameOptions.MinimizeOnLaunch; }
            set
            {
                if (_settings.GameOptions.MinimizeOnLaunch == value)
                    return;
                //OnPropertyChanging();
                _settings.GameOptions.MinimizeOnLaunch = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Disable overlay open warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnOverlayOpen
        {
            get { return _settings.AppOptions.RememberWarnOnOverlayOpen; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnOverlayOpen == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnOverlayOpen = value;

                if (!value)
                    WarnOnOverlayOpen = true;

                EnableDisableOption("WarnOnOverlayOpen", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Disable abort update warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnAbortUpdate
        {
            get { return _settings.AppOptions.RememberWarnOnAbortUpdate; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnAbortUpdate == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnAbortUpdate = value;

                if (!value)
                    WarnOnAbortUpdate = true;

                EnableDisableOption("WarnOnAbortUpdate", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Abort update: allow abort?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnAbortUpdate
        {
            get { return _settings.AppOptions.WarnOnAbortUpdate; }
            set
            {
                if (_settings.AppOptions.WarnOnAbortUpdate == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnAbortUpdate = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Disable busy shutdown warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnBusyShutdown
        {
            get { return _settings.AppOptions.RememberWarnOnBusyShutdown; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnBusyShutdown == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnBusyShutdown = value;

                if (!value)
                    WarnOnBusyShutdown = true;

                EnableDisableOption("WarnOnBusyShutdown", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Overlay open: allow closing application?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnOverlayOpen
        {
            get { return _settings.AppOptions.WarnOnOverlayOpen; }
            set
            {
                if (_settings.AppOptions.WarnOnOverlayOpen == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnOverlayOpen = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Busy shutdown: allow shutdown?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnBusyShutdown
        {
            get { return _settings.AppOptions.WarnOnBusyShutdown; }
            set
            {
                if (_settings.AppOptions.WarnOnBusyShutdown == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnBusyShutdown = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Disable Game already running warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnGameRunning
        {
            get { return _settings.AppOptions.RememberWarnOnGameRunning; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnGameRunning == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnGameRunning = value;

                if (!value)
                    WarnOnGameRunning = true;

                EnableDisableOption("WarnOnGameRunning", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Game already running: kill process?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnGameRunning
        {
            get { return _settings.AppOptions.WarnOnGameRunning; }
            set
            {
                if (_settings.AppOptions.WarnOnGameRunning == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnGameRunning = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Disable logout warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnLogout
        {
            get { return _settings.AppOptions.RememberWarnOnLogout; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnLogout == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnLogout = value;

                if (!value)
                    WarnOnLogout = true;

                EnableDisableOption("WarnOnLogout", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Logout: allow logout?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnLogout
        {
            get { return _settings.AppOptions.WarnOnLogout; }
            set
            {
                if (_settings.AppOptions.WarnOnLogout == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnLogout = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Disable unprotected server warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnUnprotectedServer
        {
            get { return _settings.AppOptions.RememberWarnOnUnprotectedServer; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnUnprotectedServer == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnUnprotectedServer = value;

                if (!value)
                    WarnOnUnprotectedServer = true;

                EnableDisableOption("WarnOnUnprotectedServer", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Unprotected server: allow join?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnUnprotectedServer
        {
            get { return _settings.AppOptions.WarnOnUnprotectedServer; }
            set
            {
                if (_settings.AppOptions.WarnOnUnprotectedServer == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnUnprotectedServer = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Disable outdated DirectX warning")]
        [Category(SettingsCategories.DisabledDialogs)]
        public bool RememberWarnOnOutdatedDirectX
        {
            get { return _settings.AppOptions.RememberWarnOnOutdatedDirectX; }
            set
            {
                if (_settings.AppOptions.RememberWarnOnOutdatedDirectX == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.RememberWarnOnOutdatedDirectX = value;

                if (!value)
                    WarnOnOutdatedDirectX = true;

                EnableDisableOption("WarnOnOutdatedDirectX", value);

                OnPropertyChanged();
            }
        }
        [DisplayName("Outdated DirectX: ignore?")]
        [Category(SettingsCategories.DisabledDialogsAutoAnswers)]
        [ReadOnly(true)]
        public bool WarnOnOutdatedDirectX
        {
            get { return _settings.AppOptions.WarnOnOutdatedDirectX; }
            set
            {
                if (_settings.AppOptions.WarnOnOutdatedDirectX == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.WarnOnOutdatedDirectX = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Friend comes online")]
        [Category(SettingsCategories.Notifications)]
        public bool FriendOnlineNotify
        {
            get { return _settings.AppOptions.FriendOnlineNotify; }
            set
            {
                if (_settings.AppOptions.FriendOnlineNotify == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.FriendOnlineNotify = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Friend joins game/server")]
        [Category(SettingsCategories.Notifications)]
        public bool FriendJoinNotify
        {
            get { return _settings.AppOptions.FriendJoinNotify; }
            set
            {
                if (_settings.AppOptions.FriendJoinNotify == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.FriendJoinNotify = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Received friend request")]
        [Category(SettingsCategories.Notifications)]
        public bool FriendRequestNotify
        {
            get { return _settings.AppOptions.FriendRequestNotify; }
            set
            {
                if (_settings.AppOptions.FriendRequestNotify == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.FriendRequestNotify = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Queue status")]
        [Category(SettingsCategories.Notifications)]
        public bool QueueStatusNotify
        {
            get { return _settings.AppOptions.QueueStatusNotify; }
            set
            {
                if (_settings.AppOptions.QueueStatusNotify == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.QueueStatusNotify = value;
                OnPropertyChanged();
            }
        }
        [DisplayName("Received chat message")]
        [Category(SettingsCategories.Notifications)]
        public bool ChatMessageNotify
        {
            get { return _settings.AppOptions.ChatMessageNotify; }
            set
            {
                if (_settings.AppOptions.ChatMessageNotify == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.ChatMessageNotify = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.ServerBrowser)]
        [DisplayName("Max concurrent server info connections")]
        [Description("Maximum allowed concurrent server info retrieval")]
        public int MaxConnections
        {
            get { return _settings.AppOptions.MaxConnections; }
            set
            {
                if (_settings.AppOptions.MaxConnections == value)
                    return;
                //OnPropertyChanging();
                _settings.AppOptions.MaxConnections = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.ServerBrowser)]
        [DisplayName("Minimum players")]
        [Description("Minimum number of players to use for Quick Play")]
        public int MinNumPlayers
        {
            get { return _settings.ServerOptions.MinNumPlayers; }
            set
            {
                if (_settings.ServerOptions.MinNumPlayers == value)
                    return;
                //OnPropertyChanging();
                _settings.ServerOptions.MinNumPlayers = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.ServerBrowser)]
        [DisplayName("Apply filters to QuickLaunch")]
        public bool ApplyServerFilters
        {
            get { return _settings.ServerOptions.QuickPlayApplyServerFilters; }
            set
            {
                if (_settings.ServerOptions.QuickPlayApplyServerFilters == value)
                    return;
                //OnPropertyChanging();
                _settings.ServerOptions.QuickPlayApplyServerFilters = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.ServerBrowser)]
        [DisplayName("Minimum free slots")]
        [Description("Minimum free slots to use for Quick Play")]
        public int MinFreeSlots
        {
            get { return _settings.ServerOptions.MinFreeSlots; }
            set
            {
                if (_settings.ServerOptions.MinFreeSlots == value)
                    return;
                //OnPropertyChanging();
                _settings.ServerOptions.MinFreeSlots = value;
                OnPropertyChanged();
            }
        }
        [Category(SettingsCategories.ServerBrowser)]
        [DisplayName("Show ping as number")]
        [Description("Replaces the ping bars with number in ms")]
        public bool ShowPingAsNumber
        {
            get { return _settings.ServerOptions.ShowPingAsNumber; }
            set
            {
                if (_settings.ServerOptions.ShowPingAsNumber == value)
                    return;
                //OnPropertyChanging();
                _settings.ServerOptions.ShowPingAsNumber = value;
                OnPropertyChanged();
            }
        }
        [Browsable(false)]
        public string[] Locales
        {
            get { return _locales; }
            private set { _locales = value; }
        }
        [Category(SettingsCategories.Other)]
        [DisplayName("Prefer system browser")]
        [Description("Prefer system browser when opening withSIX links (instead of built-in browser)")]
        public bool PreferSystemBrowser
        {
            get { return _settings.AppOptions.PreferSystemBrowser; }
            set
            {
                _settings.AppOptions.PreferSystemBrowser = value;
                OnPropertyChanged();
            }
        }
        [Browsable(false)]
        [Category(SettingsCategories.Other)]
        [ItemsSource(typeof (LocaleItemsSource))]
        public string Locale
        {
            get { return _settings.AppOptions.Locale; }
            set
            {
                if (_settings.AppOptions.Locale == value)
                    return;
                _settings.AppOptions.Locale = value;
                SetLocale(value);
                OnPropertyChanged();
            }
        }

        static void SetLocale(string locale) {
            /*
            CultureManager.UICulture = String.IsNullOrWhiteSpace(locale) ||
                           UserSettings.Locales.None(x => x == locale)
                        ? CultureInfo.CreateSpecificCulture("en")
                        : CultureManager.UICulture = CultureInfo.CreateSpecificCulture(locale);
             */
        }

        protected void SetupPropertyGrid() {
            // Needed later for refreshing property grid to enable/disable options at runtime
            PropertyGrid = this;

            // Need this to handle startup situation for enabled/disabled options
            EnableDisableOption("CloseMeansMinimize", EnableTrayIcon);

            EnableDisableOption("WarnOnOverlayOpen", RememberWarnOnOverlayOpen);
            EnableDisableOption("WarnOnAbortUpdate", RememberWarnOnAbortUpdate);
            EnableDisableOption("WarnOnBusyShutdown", RememberWarnOnBusyShutdown);
            EnableDisableOption("WarnOnGameRunning", RememberWarnOnGameRunning);
            EnableDisableOption("WarnOnLogout", RememberWarnOnLogout);
            EnableDisableOption("WarnOnUnprotectedServer", RememberWarnOnUnprotectedServer);
            EnableDisableOption("WarnOnOutdatedDirectX", RememberWarnOnOutdatedDirectX);
        }

        void RefreshPropertyGrid() {
            PropertyGrid = null;
            PropertyGrid = this;
        }

        void EnableDisableOption(string OptionName, bool Enabled) {
            var descriptor = TypeDescriptor.GetProperties(GetType())[OptionName];
            var attribute = (ReadOnlyAttribute) descriptor.Attributes[typeof (ReadOnlyAttribute)];
            var fieldToChange = attribute.GetType()
                .GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldToChange.SetValue(attribute, !Enabled);

            RefreshPropertyGrid();
        }

        [SmartAssembly.Attributes.ReportUsage]
        void Logout() {
            Common.App.Events.PublishOnCurrentThread(new RequestOpenBrowser(CommonUrls.AccountSettingsUrl));
        }
    }

    public class LocaleItemsSource : IItemsSource
    {
        public ItemCollection GetValues() {
            return new ItemCollection {
                {"EN", "English"},
                {"DE", "German"}
            };
        }
    }
}