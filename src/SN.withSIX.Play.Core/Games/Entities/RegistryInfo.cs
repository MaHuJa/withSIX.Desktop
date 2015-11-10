// <copyright company="SIX Networks GmbH" file="RegistryInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public class RegistryInfo
    {
        protected RegistryInfo() {}

        public RegistryInfo(string path, string key) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
            Contract.Requires<ArgumentNullException>(key != null);
            Path = path;
            Key = key;
        }

        public string Path { get; private set; }
        public string Key { get; private set; }
    }

    public class NullRegistryInfo : RegistryInfo {}
}