// <copyright company="SIX Networks GmbH" file="IExitHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Presentation.Services
{
    public interface IExitHandler
    {
        void Exit(int exitCode = 0);
    }
}