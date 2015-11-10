// <copyright company="SIX Networks GmbH" file="DomainEventHandlerGrabber.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    public class DomainEventHandlerGrabber : IDomainEventHandlerGrabber, IInfrastructureService
    {
        readonly IDbContextLocator _contextFactory;

        public DomainEventHandlerGrabber(IDbContextLocator contextFactory) {
            _contextFactory = contextFactory;
        }

        public IDomainEventHandler Get() {
            var context = _contextFactory.GetGameContext();
            return context.DomainEventHandler;
        }
    }
}