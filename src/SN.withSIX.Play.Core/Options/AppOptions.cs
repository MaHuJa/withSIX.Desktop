// <copyright company="SIX Networks GmbH" file="AppOptions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.Win32;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "AppOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class AppOptions : OptionBase, IEnableLogging
    {
        const int DefaultMaxConnections = 50;
        const string SmartAssemblyReportUsage = "SmartAssemblyReportUsage";
        const string CepRegKey = @"Software\SIX Networks\Play withSIX";
        public const int DefaultPMax = 8;
        public const int DefaultMax = 3;
        static readonly IDictionary<string, string> protocolMappings = new Dictionary<string, string> {
            {"zsync", "http"},
            {"zsyncs", "https"}
        };
        [Obsolete, DataMember] ConcurrentDictionary<string, AuthInfo> _AuthCache =
            new ConcurrentDictionary<string, AuthInfo>();
        [DataMember] bool _autoAdjustLibraryViewType;
        [DataMember] long? _AutoRefreshServersTime = 10;
        [DataMember] bool? _ChatMessageNotify = true;
        [DataMember] bool _CloseCurrentOverlay;
        [DataMember] bool? _CloseMeansMinimize = true;
        [DataMember] bool? _denyAutoInstall2 = false;
        [DataMember] bool _DisableEasterEggs;
        [DataMember] bool _EnableBetaUpdates;
        [DataMember] bool? _EnableTrayIcon = true;
        [DataMember] ReactiveList<ExternalApp> _ExternalApps = new ReactiveList<ExternalApp>();
        [DataMember] bool _FirstTimeLicenceAccepted;
        [DataMember] bool? _FirstTimeMinimizedToTray = true;
        [DataMember] bool? _FriendJoinNotify = true;
        [DataMember] bool? _FriendOnlineNotify = true;
        [DataMember] bool? _FriendRequestNotify = true;
        [DataMember] string _HttpProxy;
        [Obsolete] [DataMember] Guid _id = Guid.NewGuid();
        [DataMember] int _Initialized;
        [DataMember] bool? _keepCompressedFiles = true;
        [DataMember] string _LastModule = "GameBrowser";
        [DataMember] bool _LaunchWithWindows;
        [DataMember] string _Locale;
        [DataMember] int _MaxConnections;
        [DataMember] int? _MaxThreads;
        bool? _participateInCustomerExperienceProgram;
        [DataMember] bool _preferSystemBrowser;
        [DataMember] ProtocolPreference _ProtocolPreference2 = ProtocolPreference.PreferZsync;
        [DataMember] bool? _QueueStatusNotify = true;
        [DataMember] bool _RememberWarnOnAbortUpdate;
        [DataMember] bool _RememberWarnOnBusyShutdown;
        [DataMember] bool _RememberWarnOnGameRunning;
        [DataMember] bool _RememberWarnOnLogout;
        [DataMember] bool _RememberWarnOnOutdatedDirectX;
        [DataMember] bool _RememberWarnOnOverlayOpen;
        [DataMember] bool _RememberWarnOnUnprotectedServer;
        [DataMember] bool? _selectedModule = true;
        [DataMember] bool _showDialogWhenUpdateDownloaded;
        [DataMember] bool? _SuspendSyncWhileGameRunning = true;
        [DataMember] bool _UseElevatedService;
        [DataMember] bool? _WarnOnAbortUpdate = true;
        [DataMember] bool? _WarnOnBusyShutdown = true;
        [DataMember] bool? _WarnOnGameRunning = true;
        [DataMember] bool? _WarnOnLogout = true;
        [DataMember] bool? _WarnOnOutdatedDirectX = true;
        [DataMember] bool? _WarnOnOverlayOpen = true;
        [DataMember] bool? _WarnOnUnprotectedServer = true;
        public Guid Id
        {
            get { return DomainEvilGlobal.SecretData.UserInfo.ClientId; }
        }
        [DataMember]
        int _LastSelectedContactListTab { get; set; }
        public bool ShowDialogWhenUpdateDownloaded
        {
            get { return _showDialogWhenUpdateDownloaded; }
            set
            {
                if (SetProperty(ref _showDialogWhenUpdateDownloaded, value))
                    SaveSettings();
            }
        }
        public bool FirstTimeMinimizedToTray
        {
            get { return _FirstTimeMinimizedToTray.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _FirstTimeMinimizedToTray, value))
                    SaveSettings();
            }
        }
        public bool AutoAdjustLibraryViewType
        {
            get { return _autoAdjustLibraryViewType; }
            set
            {
                if (SetProperty(ref _autoAdjustLibraryViewType, value))
                    SaveSettings();
            }
        }
        ConcurrentDictionary<string, AuthInfo> AuthCache
        {
            get { return DomainEvilGlobal.SecretData.Authentication.AuthCache; }
        }
        public int MaxConnections
        {
            get
            {
                if (_MaxConnections == 0)
                    _MaxConnections = DefaultMaxConnections;
                return _MaxConnections;
            }
            set
            {
                var theval = value;
                if (value == 0)
                    theval = DefaultMaxConnections;

                if (SetProperty(ref _MaxConnections, theval))
                    SaveSettings();
            }
        }
        public bool LaunchWithWindows
        {
            get { return _LaunchWithWindows; }
            set
            {
                if (SetProperty(ref _LaunchWithWindows, value))
                    SaveSettings();
            }
        }
        public string LastModule
        {
            get { return _LastModule; }
            set { SetProperty(ref _LastModule, value); }
        }
        public bool SelectedModule
        {
            get { return _selectedModule.HasValue ? _selectedModule.Value : (bool) (_selectedModule = true); }
            set { SetProperty(ref _selectedModule, value); }
        }
        public bool PreferSystemBrowser
        {
            get { return _preferSystemBrowser; }
            set { SetProperty(ref _preferSystemBrowser, value); }
        }
        public string HttpProxy
        {
            get { return _HttpProxy; }
            set
            {
                value = value == null ? null : value.Trim();
                if (SetProperty(ref _HttpProxy, value)) {
                    SaveSettings();
                    TrySetProxy(value);
                }
            }
        }
        public bool UseElevatedService
        {
            get { return _UseElevatedService; }
            set
            {
                if (SetProperty(ref _UseElevatedService, value))
                    SaveSettings();
            }
        }
        public bool FirstTimeLicenceAccepted
        {
            get { return _FirstTimeLicenceAccepted; }
            set
            {
                if (SetProperty(ref _FirstTimeLicenceAccepted, value))
                    SaveSettings();
            }
        }
        public bool CloseMeansMinimize
        {
            get { return _CloseMeansMinimize.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _CloseMeansMinimize, value))
                    SaveSettings();
            }
        }
        public bool EnableTrayIcon
        {
            get { return _EnableTrayIcon.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _EnableTrayIcon, value))
                    SaveSettings();
            }
        }
        public bool CloseCurrentOverlay
        {
            get { return _CloseCurrentOverlay; }
            set
            {
                if (SetProperty(ref _CloseCurrentOverlay, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnOverlayOpen
        {
            get { return _RememberWarnOnOverlayOpen; }
            set
            {
                if (SetProperty(ref _RememberWarnOnOverlayOpen, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnAbortUpdate
        {
            get { return _RememberWarnOnAbortUpdate; }
            set
            {
                if (SetProperty(ref _RememberWarnOnAbortUpdate, value))
                    SaveSettings();
            }
        }
        public bool WarnOnAbortUpdate
        {
            get { return _WarnOnAbortUpdate.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnAbortUpdate, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnBusyShutdown
        {
            get { return _RememberWarnOnBusyShutdown; }
            set
            {
                if (SetProperty(ref _RememberWarnOnBusyShutdown, value))
                    SaveSettings();
            }
        }
        public bool WarnOnOverlayOpen
        {
            get { return _WarnOnOverlayOpen.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnOverlayOpen, value))
                    SaveSettings();
            }
        }
        public bool WarnOnBusyShutdown
        {
            get { return _WarnOnBusyShutdown.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnBusyShutdown, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnGameRunning
        {
            get { return _RememberWarnOnGameRunning; }
            set
            {
                if (SetProperty(ref _RememberWarnOnGameRunning, value))
                    SaveSettings();
            }
        }
        public bool WarnOnGameRunning
        {
            get { return _WarnOnGameRunning.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnGameRunning, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnLogout
        {
            get { return _RememberWarnOnLogout; }
            set
            {
                if (SetProperty(ref _RememberWarnOnLogout, value))
                    SaveSettings();
            }
        }
        public bool WarnOnLogout
        {
            get { return _WarnOnLogout.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnLogout, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnUnprotectedServer
        {
            get { return _RememberWarnOnUnprotectedServer; }
            set
            {
                if (SetProperty(ref _RememberWarnOnUnprotectedServer, value))
                    SaveSettings();
            }
        }
        public bool WarnOnUnprotectedServer
        {
            get { return _WarnOnUnprotectedServer.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnUnprotectedServer, value))
                    SaveSettings();
            }
        }
        public bool RememberWarnOnOutdatedDirectX
        {
            get { return _RememberWarnOnOutdatedDirectX; }
            set
            {
                if (SetProperty(ref _RememberWarnOnOutdatedDirectX, value))
                    SaveSettings();
            }
        }
        public bool WarnOnOutdatedDirectX
        {
            get { return _WarnOnOutdatedDirectX.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _WarnOnOutdatedDirectX, value))
                    SaveSettings();
            }
        }
        public bool FriendOnlineNotify
        {
            get { return _FriendOnlineNotify.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _FriendOnlineNotify, value))
                    SaveSettings();
            }
        }
        public bool FriendJoinNotify
        {
            get { return _FriendJoinNotify.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _FriendJoinNotify, value))
                    SaveSettings();
            }
        }
        public bool FriendRequestNotify
        {
            get { return _FriendRequestNotify.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _FriendRequestNotify, value))
                    SaveSettings();
            }
        }
        public bool QueueStatusNotify
        {
            get { return _QueueStatusNotify.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _QueueStatusNotify, value))
                    SaveSettings();
            }
        }
        public bool ChatMessageNotify
        {
            get { return _ChatMessageNotify.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _ChatMessageNotify, value))
                    SaveSettings();
            }
        }
        public int Initialized
        {
            get { return _Initialized; }
            set { SetProperty(ref _Initialized, value); }
        }
        public int? MaxThreads
        {
            get { return _MaxThreads; }
            set
            {
                if (SetProperty(ref _MaxThreads, value))
                    SaveSettings();
            }
        }
        public ReactiveList<ExternalApp> ExternalApps
        {
            get { return _ExternalApps ?? (_ExternalApps = new ReactiveList<ExternalApp>()); }
            set
            {
                if (SetProperty(ref _ExternalApps, value))
                    SaveSettings();
            }
        }
        public ProtocolPreference ProtocolPreference
        {
            get { return _ProtocolPreference2; }
            set
            {
                if (SetProperty(ref _ProtocolPreference2, value))
                    SaveSettings();
            }
        }
        public bool SuspendSyncWhileGameRunning
        {
            get { return _SuspendSyncWhileGameRunning.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _SuspendSyncWhileGameRunning, value))
                    SaveSettings();
            }
        }
        public bool DisableEasterEggs
        {
            get { return _DisableEasterEggs; }
            set
            {
                if (SetProperty(ref _DisableEasterEggs, value))
                    SaveSettings();
            }
        }
        public long AutoRefreshServersTime
        {
            get { return _AutoRefreshServersTime.GetValueOrDefault(10); }
            set
            {
                if (SetProperty(ref _AutoRefreshServersTime, value))
                    SaveSettings();
            }
        }
        public string Locale
        {
            get { return _Locale; }
            set
            {
                if (SetProperty(ref _Locale, value))
                    SaveSettings();
            }
        }
        public bool ParticipateInCustomerExperienceProgram
        {
            get { return _participateInCustomerExperienceProgram.GetValueOrDefault(GetCEP()); }
            set
            {
                TryUpdateCEPRegistry(value);
                SetProperty(ref _participateInCustomerExperienceProgram, value);
            }
        }
        public bool EnableBetaUpdates
        {
            get { return _EnableBetaUpdates; }
            set
            {
                if (SetProperty(ref _EnableBetaUpdates, value))
                    SaveSettings();
            }
        }
        public bool ServerListEnabled { get; set; }
        public bool DenyAutoInstall
        {
            get { return _denyAutoInstall2.GetValueOrDefault(false); }
            set { _denyAutoInstall2 = value; }
        }
        public bool KeepCompressedFiles
        {
            get { return _keepCompressedFiles.GetValueOrDefault(true); }
            set
            {
                if (SetProperty(ref _keepCompressedFiles, value))
                    SaveSettings();
            }
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (_AuthCache != null) {
                if (!DomainEvilGlobal.SecretData.Authentication.AuthCache.Any())
                    DomainEvilGlobal.SecretData.Authentication.AuthCache = _AuthCache;
                _AuthCache = null;
            }
            if (_id != Guid.Empty)
                DomainEvilGlobal.SecretData.UserInfo.ClientId = _id;
            _id = Guid.Empty;
            if (Id == Guid.Empty)
                DomainEvilGlobal.SecretData.UserInfo.ClientId = Guid.NewGuid();
        }

        int GetMaxThreadsInternal() {
            return DomainEvilGlobal.SecretData.UserInfo.Token.IsPremium() ? DefaultPMax : DefaultMax;
        }

        public int GetMaxThreads() {
            return GetMaxThreads(GetMaxThreadsInternal());
        }

        int GetMaxThreads(int defaultMax) {
            var mt = MaxThreads;
            return mt == null || mt > defaultMax ? defaultMax : mt.Value;
        }

        void TryUpdateCEPRegistry(bool value) {
            try {
                var reg = Tools.Generic.OpenRegistry(RegistryView.Registry32, RegistryHive.CurrentUser);
                var key = reg.CreateSubKey(CepRegKey);
                key.SetValue(SmartAssemblyReportUsage, value ? "True" : "False");
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        public void TrySetProxy(string value) {
            try {
                Environment.SetEnvironmentVariable("http_proxy", value);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        public AuthInfo GetAuthInfoFromCache(Uri uri) {
            AuthInfo val;

            if (AuthCache.TryGetValue(GetAuthInfoKey(uri), out val))
                return val;

            return null;
        }

        static string GetAuthInfoKey(Uri uri) {
            var scheme = protocolMappings.ContainsKey(uri.Scheme) ? protocolMappings[uri.Scheme] : uri.Scheme;
            var port = scheme == "http" && uri.Port == -1 ? 80 : (scheme == "https" && uri.Port == -1 ? 443 : uri.Port);
            return String.Format("{0}://{1}:{2}", scheme, uri.Host, port);
        }

        public void SetAuthInfo(Uri uri, string username, string password, string domain = null) {
            SetAuthInfo(uri, new AuthInfo(username, password, domain));
        }

        public void SetAuthInfo(Uri uri, AuthInfo authInfo) {
            var key = String.Format("{0}://{1}:{2}", uri.Scheme, uri.Host, uri.Port);

            if (authInfo == null
                || authInfo.Username == null && authInfo.Password == null && authInfo.Domain == null) {
                AuthInfo val;
                AuthCache.TryRemove(key, out val);
            } else
                AuthCache.AddOrUpdate(key, authInfo, (s, info) => authInfo);
        }

        bool GetCEP() {
            try {
                var reg = Tools.Generic.OpenRegistry(RegistryView.Registry32, RegistryHive.CurrentUser);
                var key = reg.OpenSubKey(CepRegKey);
                if (key == null)
                    return false;
                var val = key.GetValue(SmartAssemblyReportUsage) as string;
                return val.TryBool();
            } catch (SecurityException e) {
                this.Logger().FormattedWarnException(e, "exception while checking CEP");
                return true;
            }
        }
    }
}