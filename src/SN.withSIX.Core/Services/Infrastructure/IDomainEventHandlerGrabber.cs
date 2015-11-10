// <copyright company="SIX Networks GmbH" file="IDomainEventHandlerGrabber.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Services.Infrastructure
{
    public interface IDomainEventHandlerGrabber
    {
        IDomainEventHandler Get();
    }
}