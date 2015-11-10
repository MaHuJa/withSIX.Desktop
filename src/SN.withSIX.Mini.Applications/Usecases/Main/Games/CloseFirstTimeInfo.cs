// <copyright company="SIX Networks GmbH" file="CloseFirstTimeInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class CloseFirstTimeInfo : IAsyncVoidCommand, IHaveId<Guid>
    {
        public CloseFirstTimeInfo(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class CloseFirstTimeInfoHandler : DbCommandBase, IAsyncVoidCommandHandler<CloseFirstTimeInfo>
    {
        public CloseFirstTimeInfoHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(CloseFirstTimeInfo request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            game.FirstTimeRunShown = true; // Or this more of a 'Setting' ??
            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}