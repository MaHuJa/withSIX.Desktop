// <copyright company="SIX Networks GmbH" file="IUnitOfWork.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Core.Applications.Infrastructure
{
    public interface IUnitOfWork
    {
        Task<int> SaveChanges();
    }
}