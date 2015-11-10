// <copyright company="SIX Networks GmbH" file="IDbContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Mini.Applications.Services.Infra
{
    public interface IDbContext
    {
        IDomainEventHandler DomainEventHandler { get; }
        void AddTransactionCallback(Action act);
        void AddTransactionCallback(Func<Task> act);
    }
}