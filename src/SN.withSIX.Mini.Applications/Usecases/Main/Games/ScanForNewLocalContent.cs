// <copyright company="SIX Networks GmbH" file="ScanForNewLocalContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Core;
using SN.withSIX.Mini.Applications.Services.Infra;
using Splat;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class ScanForNewLocalContent : IAsyncVoidCommand, IHaveId<Guid>
    {
        public ScanForNewLocalContent(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ScanForNewLocalContentHandler : DbCommandBase, IAsyncVoidCommandHandler<ScanForNewLocalContent>
    {
        readonly ISetupGameStuff _setup;

        public ScanForNewLocalContentHandler(IDbContextLocator dbContextLocator, ISetupGameStuff setup)
            : base(dbContextLocator) {
            _setup = setup;
        }

        public async Task<UnitType> HandleAsync(ScanForNewLocalContent request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);

            try {
                await _setup.HandleGameContentsWhenNeeded().ConfigureAwait(false);
            } catch (Exception ex) {
                MainLog.Logger.Write(ex.Format(), LogLevel.Warn);
            }

            // TODO: Lock per game?
            await game.ScanForLocalContent().ConfigureAwait(false);

            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}