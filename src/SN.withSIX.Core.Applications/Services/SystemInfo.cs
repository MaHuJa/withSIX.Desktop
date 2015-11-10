// <copyright company="SIX Networks GmbH" file="SystemInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Timers;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Net;
using NetFwTypeLib;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core.Applications.Services
{
    public class SystemInfo : PropertyChangedBase, IApplicationService, IDisposable, ISystemInfo, IEnableLogging
    {
        static readonly TimeSpan networkPollIntervalMs = TimeSpan.FromSeconds(5);
        readonly IList<int> _directXSupportedVersions = new List<int>();
        readonly Timer _networkPollTimer;
        bool _isInternetAvailable;

        public SystemInfo() {
            IsAVDetected = TryDetectAV();
            DetectDirectXVersion();

            PollNetwork();
            _networkPollTimer = new TimerWithoutOverlap(networkPollIntervalMs, PollNetwork);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsAVDetected { get; }
        public bool IsInternetAvailable
        {
            get { return _isInternetAvailable; }
            set { SetProperty(ref _isInternetAvailable, value); }
        }
        public bool DidDetectAVRun { get; set; }
        public IList<string> InstalledAV { get; } = new List<string>();
        public IList<string> InstalledFW { get; } = new List<string>();
        public int DirectXVersion { get; set; }

        public bool DirectXVersionSupported(int versionToCheck) {
            return _directXSupportedVersions.Contains(versionToCheck);
        }

        void PollNetwork() {
            try {
                IsInternetAvailable = NetworkListManager.IsConnectedToInternet;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e, "error during check for internet connectivity");
                IsInternetAvailable = true;
            }
        }

        bool TryDetectAV() {
            try {
                var result = DetectAV();
                DidDetectAVRun = true;
                return result;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                DidDetectAVRun = false;
                return false;
            }
        }

        void DetectDirectXVersion() {
            var directXMajorVersion = TryGetDirectXVersionFromRegistry();

            if (directXMajorVersion == 9)
                _directXSupportedVersions.Add(9);

            var osVersion = Environment.OSVersion;
            const int vista = (int) WindowsVersions.Vista;
            if (osVersion.Version.Major >= vista) {
                if (osVersion.Version.Major > vista || osVersion.Version.Minor >= 1) {
                    _directXSupportedVersions.AddRange(new[] {10, 11});
                    DirectXVersion = 11;
                } else {
                    _directXSupportedVersions.Add(10);
                    DirectXVersion = 10;
                }
            } else
                DirectXVersion = directXMajorVersion;
        }

        static int TryGetDirectXVersionFromRegistry() {
            try {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX")) {
                    var version = key.GetValue("Version") as string;
                    if (string.IsNullOrEmpty(version))
                        return 0;
                    var versionComponents = version.Split('.');
                    if (versionComponents.Length <= 1)
                        return 0;
                    int directXLevel;
                    if (int.TryParse(versionComponents[1], out directXLevel))
                        return directXLevel;
                }
                return 0;
            } catch (Exception) {
                return 0;
            }
        }

        static bool WMINamespaceExists(string namespaceName) {
            var nsClass = new ManagementClass(new ManagementScope("root"), new ManagementPath("__namespace"), null);
            var namespacesFound =
                (from ManagementObject ns in nsClass.GetInstances() select ns["Name"].ToString()).ToList();

            return namespacesFound.Exists(name => name == namespaceName);
        }

        bool DetectAV() {
            var searcher = new ManagementObjectSearcher();

            var queries = new[] {
                new SelectQuery("select * from AntiVirusProduct"),
                new SelectQuery("select * from FirewallProduct")
            };

            // SecurityCenter = Windows XP, SecurityCenter2 = Windows Vista, 7 and 8
            searcher.Scope = WMINamespaceExists("SecurityCenter2")
                ? new ManagementScope(@"\root\SecurityCenter2")
                : new ManagementScope(@"\root\SecurityCenter");

            var foundAV = false;

            for (var i = 0; i < 2; i++) {
                searcher.Query = queries[i];

                var searchResults = searcher.Get();

                if (searchResults.Count > 0)
                    foundAV = true;

                var objEnum = searchResults.GetEnumerator();

                while (objEnum.MoveNext()) {
                    var obj = (ManagementObject) objEnum.Current;

                    if (i == 0)
                        InstalledAV.Add(obj["DisplayName"].ToString());
                    else
                        InstalledFW.Add(obj["DisplayName"].ToString());
                }
            }

            // Special check for Windows Firewall (which does not register itself in FirewallProduct)
            const string clsIdFwMgr = "{304CE942-6E39-40D8-943A-B913C40C9CD4}";

            var fwMgr = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(clsIdFwMgr))) as INetFwMgr;

            if (fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled) {
                foundAV = true;

                InstalledFW.Add("Windows Firewall");
            }

            return foundAV;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                _networkPollTimer.Dispose();
        }

        enum WindowsVersions
        {
            Vista = 6
        }
    }
}