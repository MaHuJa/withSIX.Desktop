// <copyright company="SIX Networks GmbH" file="RvContentScanner.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    public class RvContentScanner
    {
        readonly RealVirtualityGame _realVirtualityGame;

        public RvContentScanner(RealVirtualityGame realVirtualityGame) {
            _realVirtualityGame = realVirtualityGame;
        }

        public IEnumerable<LocalContent> ScanForNewContent(IReadOnlyCollection<string> dlcs,
            IEnumerable<IAbsoluteDirectoryPath> paths)
            => paths.SelectMany(x => GetMods(x, dlcs));

        IEnumerable<LocalContent> GetMods(IAbsoluteDirectoryPath d, IReadOnlyCollection<string> dlcs)
            =>
                d.DirectoryInfo.GetDirectories()
                    .Where(x => !dlcs.ContainsIgnoreCase(x.Name))
                    .Select(HandleContent)
                    .Where(x => x != null);

        LocalContent HandleContent(FileSystemInfo dir) {
            var networkContents = _realVirtualityGame.NetworkContent.OfType<ModNetworkContent>();
            var nc = networkContents.FirstOrDefault(
                x => x.PackageName.Equals(dir.Name, StringComparison.CurrentCultureIgnoreCase))
                     ??
                     networkContents.FirstOrDefault(x => x.Aliases.ContainsIgnoreCase(dir.Name));

            if (nc == null)
                return ScanForAddonFolders(dir);

            // TODO: PackageName here could be different from the actual dir name because we also scan aliases
            // we need to fix that!!!
            return !HasContentAlready(nc.PackageName)
                ? new ModLocalContent(nc)
                : null;
        }

        bool HasContentAlready(string value) {
            return
                _realVirtualityGame.LocalContent.Any(
                    x => x.PackageName.Equals(value, StringComparison.CurrentCultureIgnoreCase));
        }

        LocalContent ScanForAddonFolders(FileSystemInfo dir) {
            var di = dir.FullName.ToAbsoluteDirectoryPath();
            var dirs = new[] {"addons", "dta", "common", "dll"};
            if (dirs.Any(x => di.GetChildDirectoryWithName(x).Exists)) {
                return !HasContentAlready(dir.Name)
                    ? new ModLocalContent(dir.Name, dir.Name, _realVirtualityGame.Id)
                    : null;
            }
            return null;
        }
    }
}