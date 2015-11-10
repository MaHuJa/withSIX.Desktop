// <copyright company="SIX Networks GmbH" file="GetInstalled.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Installed;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class GetInstalled : IAsyncQuery<IInstalledViewModel>, IHaveId<Guid>
    {
        public GetInstalled(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetInstalledHandler : DbQueryBase, IAsyncRequestHandler<GetInstalled, IInstalledViewModel>
    {
        public GetInstalledHandler(IDbContextLocator dbContextLocator)
            : base(dbContextLocator) {}

        public async Task<IInstalledViewModel> HandleAsync(GetInstalled request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<InstalledViewModel>();
        }
    }
}