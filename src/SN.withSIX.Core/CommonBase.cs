// <copyright company="SIX Networks GmbH" file="CommonBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reflection;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Core
{
    public static class CommonBase
    {
        public static bool IsMerged() {
            return typeof (CommonBase).Assembly.GetName().Name != "SN.withSIX.Core";
        }
        public static IAssemblyLoader AssemblyLoader { get; set; }// = new AssemblyLoader(Assembly.GetEntryAssembly());

    }
}