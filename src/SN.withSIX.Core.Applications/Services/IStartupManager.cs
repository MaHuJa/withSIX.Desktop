// <copyright company="SIX Networks GmbH" file="IStartupManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Core.Applications.Services
{
    public interface IStartupManager
    {
        Task Exit();
        void RegisterServices();
    }
}