// <copyright company="SIX Networks GmbH" file="IRsyncLauncher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IRsyncLauncher
    {
        ProcessExitResultWithOutput Run(string source, string destination, string key = null);

        ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, string source, string destination,
            string key = null);

        Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, string source,
            string destination,
            string key = null);

        ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, string source, string destination,
            CancellationToken token,
            string key = null);

        Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, string source,
            string destination, CancellationToken token,
            string key = null);
    }
}