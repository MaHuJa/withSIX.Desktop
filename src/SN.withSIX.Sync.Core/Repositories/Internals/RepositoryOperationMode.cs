// <copyright company="SIX Networks GmbH" file="RepositoryOperationMode.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    [DoNotObfuscateType]
    public enum RepositoryOperationMode
    {
        Default,
        SinglePackage
    }
}