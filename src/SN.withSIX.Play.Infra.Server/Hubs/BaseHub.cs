// <copyright company="SIX Networks GmbH" file="BaseHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Microsoft.AspNet.SignalR;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Play.Infra.Server.Hubs
{
    [DoNotObfuscateType]
    public abstract class BaseHub : Hub, IInfrastructureService
    {
        protected readonly IMediator _mediator;

        protected BaseHub(IMediator mediator) {
            _mediator = mediator;
        }
    }
}