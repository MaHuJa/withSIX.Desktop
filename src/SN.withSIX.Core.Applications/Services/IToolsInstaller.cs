// <copyright company="SIX Networks GmbH" file="IToolsInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Sync.Core.Legacy.Status;

namespace SN.withSIX.Core.Applications.Services
{
    public interface IToolsInstaller
    {
        Task DownloadAndInstallTools(StatusRepo repo);
        Task<bool> ConfirmToolsInstalled(bool thoroughCheck);
    }
}