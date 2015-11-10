// <copyright company="SIX Networks GmbH" file="RsyncDownloadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    [DoNotObfuscate]
    public class RsyncException : DownloadException
    {
        public RsyncException(string message, string output = null, string parameters = null, Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    [DoNotObfuscate]
    public class RsyncSoftException : RsyncException
    {
        public RsyncSoftException(string message, string output = null, string parameters = null, Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    public class RsyncDownloadProtocol : DownloadProtocol
    {
        static readonly IEnumerable<string> schemes = new[] {"rsync"};
        readonly IRsyncLauncher _rsyncLauncher;

        public RsyncDownloadProtocol(IRsyncLauncher rsyncLauncher) {
            _rsyncLauncher = rsyncLauncher;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        public override async Task DownloadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            ProcessExitResult(
                await
                    _rsyncLauncher.RunAndProcessAsync(spec.Progress, spec.Uri.ToString(), spec.LocalFile.ToString(),
                        spec.CancellationToken)
                        .ConfigureAwait(false), spec);
            VerifyIfNeeded(spec, spec.LocalFile);
        }

        public override void Download(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            ProcessExitResult(
                _rsyncLauncher.RunAndProcess(spec.Progress, spec.Uri.ToString(), spec.LocalFile.ToString(),
                    spec.CancellationToken), spec);
            VerifyIfNeeded(spec, spec.LocalFile);
        }

        static void ProcessExitResult(ProcessExitResultWithOutput result, TransferSpec spec) {
            switch (result.ExitCode) {
            case 0:
                break;
            case -1:
                throw new RsyncSoftException(
                    String.Format("Aborted/Killed (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 5:
                throw new RsyncSoftException(
                    String.Format("Server full (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 10:
                throw new RsyncSoftException(
                    String.Format("Connection refused (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError,
                    result.StartInfo.Arguments);
            case 12:
                throw new RsyncSoftException(
                    String.Format("Could not retrieve file (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 14:
                throw new RsyncSoftException(
                    String.Format("Could not retrieve file due to IPC error (PID: {0}, Status: {1}). {2}", result.Id,
                        result.ExitCode, CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 23:
                throw new RsyncSoftException(
                    String.Format("Could not retrieve file (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 24:
                throw new RsyncSoftException(
                    String.Format("Could not retrieve file (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 30:
                throw new RsyncSoftException(
                    String.Format("Could not retrieve file due to Timeout (PID: {0}, Status: {1}). {2}", result.Id,
                        result.ExitCode, CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 35:
                throw new RsyncSoftException(
                    String.Format("Could not retrieve file due to Timeout (PID: {0}, Status: {1}). {2}", result.Id,
                        result.ExitCode, CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            default:
                throw new RsyncException(
                    String.Format("Did not exit gracefully (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            }
        }
    }
}