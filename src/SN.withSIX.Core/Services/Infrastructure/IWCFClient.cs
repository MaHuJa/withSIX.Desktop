// <copyright company="SIX Networks GmbH" file="IWCFClient.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Diagnostics;

namespace SN.withSIX.Core.Services.Infrastructure
{
    public interface IWCFClient
    {
        int PerformOperation(params string[] args);
        Process LaunchGame(params string[] args);
    }
}