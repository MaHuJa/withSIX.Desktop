// <copyright company="SIX Networks GmbH" file="AssemblyHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;

namespace SN.withSIX.Core.Presentation.Assemblies
{
    public class AssemblyHandler
    {
        public static readonly string Bitness = Environment.Is64BitProcess ? "x64" : "x86";

        public void Register() {
            var path = Path.Combine(CommonBase.AssemblyLoader.GetEntryPath(), Bitness);
            Environment.SetEnvironmentVariable("path",
                string.Join(";", path,
                    Environment.GetEnvironmentVariable("path")));
        }
    }
}