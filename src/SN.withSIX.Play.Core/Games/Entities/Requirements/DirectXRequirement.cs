// <copyright company="SIX Networks GmbH" file="DirectXRequirement.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Win32;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Core.Games.Entities.Requirements
{
    public class DirectXRequirement : Requirement
    {
        readonly IList<int> _directXSupportedVersions = new List<int>();
        int _directXVersion;

        public DirectXRequirement(Version version) {
            Version = version;
        }

        public Version Version { get; }

        public override void ThrowWhenMissing() {
            try {
                DetectDirectXVersion();
                if (_directXVersion < Version.Major)
                    throw new RequirementMissingException(GetType() + ": " + Version + " not found");
            } catch (Exception e) {
                throw new RequirementProcessException(GetType() + ": " + e.Message, e);
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
                    _directXVersion = 11;
                } else {
                    _directXSupportedVersions.Add(10);
                    _directXVersion = 10;
                }
            } else
                _directXVersion = directXMajorVersion;
        }

        static int TryGetDirectXVersionFromRegistry() {
            try {
                using (var Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX")) {
                    var version = Key.GetValue("Version") as string;
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

        enum WindowsVersions
        {
            Vista = 6
        }
    }
}