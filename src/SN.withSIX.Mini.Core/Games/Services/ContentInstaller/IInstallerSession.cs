// <copyright company="SIX Networks GmbH" file="IInstallerSession.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Core.Games.Services.ContentInstaller
{
    public interface IInstallerSession
    {
        Task Install(IEnumerable<IContentSpec<IPackagedContent>> content);
        Task Synchronize();
        void Abort();
        void RunCE(IPackagedContent content);
    }
}