// <copyright company="SIX Networks GmbH" file="InstallerSessionFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Presentation.Wpf.Services;

namespace SN.withSIX.Mini.Presentation.Wpf.Factories
{
    public class InstallerSessionFactory : IINstallerSessionFactory, IPresentationService
    {
        readonly Func<bool> _isPremium;
        readonly IToolsInstaller _toolsInstaller;
        readonly IContentEngine _contentEngine;

        public InstallerSessionFactory(Func<bool> isPremium, IToolsInstaller toolsInstaller, IContentEngine contentEngine) {
            _isPremium = isPremium;
            _toolsInstaller = toolsInstaller;
            _contentEngine = contentEngine;
        }

        public IInstallerSession Create(
            IInstallContentAction<IInstallableContent> action,
            Func<double, double, Task> progress) {
            switch (action.InstallerType) {
            case InstallerType.Synq:
                return new SynqInstallerSession(action, _toolsInstaller, _isPremium(), progress, _contentEngine);
            default:
                throw new NotSupportedException(action.InstallerType + " is not supported!");
            }
        }

        public IUninstallSession CreateUninstaller(IUninstallContentAction2<IUninstallableContent> action) {
            return new SynqUninstallerSession(action);
        }
    }
}