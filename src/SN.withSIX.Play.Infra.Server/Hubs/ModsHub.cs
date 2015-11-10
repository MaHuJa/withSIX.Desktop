// <copyright company="SIX Networks GmbH" file="ModsHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Infra.Server.UseCases;

namespace SN.withSIX.Play.Infra.Server.Hubs
{
    [DoNotObfuscateType]
    public class ModsHub : BaseHub
    {
        public ModsHub(IMediator mediator) : base(mediator) {}

        public async Task<IModInfo> GetModInfoQuery(Guid modId, Guid? gameId) {
            return
                await
                    _mediator.RequestAsync(gameId != null
                        ? new RequestModInformation(modId, gameId.Value)
                        : new RequestModInformation(modId, gameId.Value)).ConfigureAwait(false);
        }

        public async Task<Dictionary<Guid, IModInfo>> GetInstalledModInfosQueryByGame(Guid gameId) {
            return await _mediator.RequestAsync(new RequestModInformationByGame(gameId, true)).ConfigureAwait(false);
        }
    }
}