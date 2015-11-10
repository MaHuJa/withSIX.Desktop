// <copyright company="SIX Networks GmbH" file="IINstallerSessionFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Core.Games.Services.ContentInstaller
{
    public interface IINstallerSessionFactory
    {
        IInstallerSession Create(IInstallContentAction<IInstallableContent> action,
            Func<double, double, Task> progress);

        IUninstallSession CreateUninstaller(IUninstallContentAction2<IUninstallableContent> action);
    }
}