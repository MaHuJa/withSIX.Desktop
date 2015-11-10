// <copyright company="SIX Networks GmbH" file="ArmaModContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Linq;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class ArmaModContainer : RealVirtualityModContainer<ArmaGameData>
    {
        public ArmaModContainer(DirectoryInfo directory, IGameContext gameContext) {
            foreach (var dir in directory.GetDirectories("@*")) {
                var syncMod = gameContext.Mods.FirstOrDefault(mo => mo.CppName == dir.Name);
                if (syncMod != null) {
                    throw new NotImplementedException("SyncManager does not hold the correct mod type");
                    _content.Add((dynamic) syncMod);
                }
                var mod = new RealVirtualityMod<ArmaGameData>(Guid.Empty, null,
                    new ModMetaData {Name = dir.Name, FullName = dir.Name});
                _content.Add(mod);
            }
        }
    }
}