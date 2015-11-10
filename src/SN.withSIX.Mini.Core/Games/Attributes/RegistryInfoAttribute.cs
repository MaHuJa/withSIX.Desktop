// <copyright company="SIX Networks GmbH" file="RegistryInfoAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Mini.Core.Games.Attributes
{
    public class RegistryInfoAttribute : Attribute
    {
        public static readonly RegistryInfoAttribute Default = new NullRegistryInfo();
        protected RegistryInfoAttribute() {}

        public RegistryInfoAttribute(string path, string key) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
            Contract.Requires<ArgumentNullException>(key != null);
            Path = path;
            Key = key;
        }

        public string Path { get; private set; }
        public string Key { get; private set; }

        class NullRegistryInfo : RegistryInfoAttribute {}
    }
}