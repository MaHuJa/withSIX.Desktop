// <copyright company="SIX Networks GmbH" file="RsyncUploadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public class RsyncUploadProtocol : UploadProtocol
    {
        static readonly IEnumerable<string> schemes = new[] {"rsync"};
        readonly IRsyncLauncher _rsyncLauncher;

        public RsyncUploadProtocol(IRsyncLauncher rsyncLauncher) {
            _rsyncLauncher = rsyncLauncher;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        public override async Task UploadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            ProcessExitResult(
                await
                    _rsyncLauncher.RunAndProcessAsync(spec.Progress, spec.LocalFile.ToString(), spec.Uri.ToString())
                        .ConfigureAwait(false), spec);
        }

        public override void Upload(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            ProcessExitResult(
                _rsyncLauncher.RunAndProcess(spec.Progress, spec.LocalFile.ToString(), spec.Uri.ToString()), spec);
        }

        static void ProcessExitResult(ProcessExitResultWithOutput result, TransferSpec spec) {
            if (result.ExitCode == 0)
                return;
            throw new RsyncException(
                String.Format("Did not exit gracefully (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                    CreateTransferExceptionMessage(spec)),
                result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
        }
    }
}