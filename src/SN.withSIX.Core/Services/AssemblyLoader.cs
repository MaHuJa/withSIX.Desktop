// <copyright company="SIX Networks GmbH" file="AssemblyLoader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SN.withSIX.Core.Services
{
    public class AssemblyLoader : IAssemblyLoader, IDomainService
    {
        readonly Assembly _entryAssembly;

        public AssemblyLoader(Assembly assembly) {
            if (assembly == null)
                throw new Exception("Entry Assembly is null!");
            _entryAssembly = assembly;
        }

        #region IAssemblyLoader Members

        public Version GetEntryVersion() {
            return _entryAssembly.GetName().Version;
        }

        public string GetProductVersion() {
            var attr = Attribute
                .GetCustomAttribute(
                    _entryAssembly,
                    typeof (AssemblyInformationalVersionAttribute))
                as AssemblyInformationalVersionAttribute;
            return attr.InformationalVersion;
        }


        public string GetEntryAssemblyName() {
            return _entryAssembly.GetName().Name;
        }

        public string GetEntryPath() {
            return Path.GetDirectoryName(GetEntryLocation());
        }

        public string GetEntryLocation() {
            return _entryAssembly.Location;
        }

        public string GetInformationalVersion() {
            return _entryAssembly.GetInformationalVersion();
        }

        #endregion
    }

    public static class AssemblyExtensions
    {
        public static string GetInformationalVersion(this Assembly assembly) {
            return FileVersionInfo.GetVersionInfo(assembly.Location)?.ProductVersion;
        }
    }
}