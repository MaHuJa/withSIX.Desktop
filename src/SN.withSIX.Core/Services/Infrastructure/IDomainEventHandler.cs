// <copyright company="SIX Networks GmbH" file="IDomainEventHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Core.Services.Infrastructure
{
    public interface IDomainEventHandler
    {
        void PrepareEvent(object obj, IDomainEvent evt);
        Task RaiseEvents();
    }
}