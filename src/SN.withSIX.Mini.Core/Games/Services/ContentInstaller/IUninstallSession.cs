// <copyright company="SIX Networks GmbH" file="IUninstallSession.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Mini.Core.Games.Services.ContentInstaller
{
    public interface IUninstallSession
    {
        Task Uninstall(IUninstallableContent content);
    }
}