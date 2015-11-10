// <copyright company="SIX Networks GmbH" file="SynqUninstallerSession.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Mini.Presentation.Wpf.Services
{
    public class SynqUninstallerSession : IUninstallSession
    {
        readonly IUninstallContentAction2<IUninstallableContent> _action;
        PackageManager _pm;
        Repository _repository;

        public SynqUninstallerSession(IUninstallContentAction2<IUninstallableContent> action) {
            _action = action;
        }

        public async Task Uninstall(IUninstallableContent content) {
            await new ContentStatusChanged(content, ItemState.Uninstalling).RaiseEvent().ConfigureAwait(false);
            var dir = _action.Paths.Path.GetChildDirectoryWithName(content.PackageName);
            if (dir.Exists)
                dir.DirectoryInfo.Delete(true);

            using (_repository = new Repository(GetRepositoryPath(), true)) {
                _pm = new PackageManager(_repository, _action.Paths.Path, true);
                _pm.DeletePackageIfExists(new SpecificVersion(content.PackageName));
            }

            var lc = content as LocalContent;
            if (lc != null && lc.ContentId != Guid.Empty) {
                if (lc is ModLocalContent)
                    _action.Status.Mods.Uninstall.Add(lc.ContentId);
                else if (lc is MissionLocalContent) {
                    _action.Status.Missions.Uninstall.Add(lc.ContentId);
                    // TODO
                    //} else if (lc is CollectionLocalContent)
                    //{
                    //_action.Status.Collections.Uninstall.Add(lc.ContentId);
                }
            }
            await new ContentStatusChanged(content, ItemState.Uninstalled).RaiseEvent().ConfigureAwait(false);
        }

        IAbsoluteDirectoryPath GetRepositoryPath() {
            return _action.Paths.RepositoryPath.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory);
        }
    }
}