// <copyright company="SIX Networks GmbH" file="IronFrontService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.Services
{
    public class IronFrontService : IApplicationService
    {
        readonly IRepoActionHandler _actionHandler;
        readonly IGameContext _gameContext;
        readonly Guid[] _ifCollections = {
            new Guid("a6292017-015e-4258-831e-88ef556de692"),
            new Guid("894b8a60-5387-406c-8b5e-891298e94d30")
        };
        readonly string[] _ifMods = {"@IF", "@IFA3"};
        readonly IronFrontInstaller _ironFrontInstaller;

        public IronFrontService(IronFrontInstaller ironFrontInstaller, IGameContext gameContext,
            IRepoActionHandler actionHandler) {
            Contract.Requires<ArgumentNullException>(ironFrontInstaller != null);
            Contract.Requires<ArgumentNullException>(gameContext != null);
            Contract.Requires<ArgumentNullException>(actionHandler != null);

            _ironFrontInstaller = ironFrontInstaller;
            _gameContext = gameContext;
            _actionHandler = actionHandler;
        }

        public bool IsIronFrontEnabled(Collection collection) {
            return collection != null && (_ifCollections.Contains(collection.Id) ||
                                          collection.EnabledMods.Select(x => x.Name)
                                              .Any(_ifMods.ContainsIgnoreCase));
        }

        public bool IsIronFrontInstalled(Game game) {
            var ironFrontInfo = GetIronFrontInfo(game);
            return ironFrontInfo.IsInstalled() && _ironFrontInstaller.ConfirmPatchedToLatestVersion(ironFrontInfo);
        }

        public void InstallIronFrontArma(Game game) {
            _ironFrontInstaller.Install(GetIronFrontInfo(game), _actionHandler.PerformStatusActionWithBusyHandling);
        }

        IronFrontInfo GetIronFrontInfo(Game game) {
            var ironFront = _gameContext.Games.Find(GameUuids.IronFront);
            if (!ironFront.InstalledState.IsInstalled)
                throw new OaIronfrontNotFoundException("Iron front not found at");

            if (!game.InstalledState.IsInstalled)
                throw new OaIronfrontNotFoundException("Primary game not found?");

            return new IronFrontInfo(ironFront.InstalledState.Directory,
                game.InstalledState.Directory, Path.Combine(Path.GetTempPath(), "IFA").ToAbsoluteDirectoryPath(),
                new IfaStatus(), game.Id == GameUuids.Arma3 ? IfaGameEdition.Arma3 : IfaGameEdition.Arma2CO);
        }
    }

    public class OaIronfrontNotFoundException : Exception
    {
        public OaIronfrontNotFoundException(string message) : base(message) {}
    }
}