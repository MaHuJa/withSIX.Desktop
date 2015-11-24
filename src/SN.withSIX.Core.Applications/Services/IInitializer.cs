// <copyright company="SIX Networks GmbH" file="IInitializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Core.Applications.Services
{
    public interface IInitializer
    {
        Task Initialize();
        Task Deinitialize();
    }

    public interface IInitializeAfterUI
    {
        Task InitializeAfterUI();
    }
}