// <copyright company="SIX Networks GmbH" file="FileDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class FileDownloader : TransferBase<IDownloadProtocol>, IFileDownloader
    {
        readonly IAuthProvider _authProvider;

        public FileDownloader(IEnumerable<IDownloadProtocol> strategies,
            IAuthProvider authProvider) {
            RegisterProtocolStrategies(strategies);
            _authProvider = authProvider;
        }

        public void Download(FileDownloadSpec spec) {
            DownloadInternal(GetSpec(spec));
        }

        public Task DownloadAsync(FileDownloadSpec spec) {
            return DownloadAsyncInternal(GetSpec(spec));
        }

        protected virtual void DownloadInternal(FileDownloadSpec spec) {
            ConfirmStrategySupported(spec.Uri);
            spec.LocalFile.RemoveReadonlyWhenExists();
            GetStrategy(spec.Uri).Download(spec);
        }

        protected virtual Task DownloadAsyncInternal(FileDownloadSpec spec) {
            ConfirmStrategySupported(spec.Uri);
            spec.LocalFile.RemoveReadonlyWhenExists();
            return GetStrategy(spec.Uri).DownloadAsync(spec);
        }

        FileDownloadSpec GetSpec(FileDownloadSpec spec) {
            return spec.Progress == null
                ? new FileDownloadSpec(_authProvider.HandleUri(spec.Uri), spec.LocalFile) {
                    Verification = spec.Verification,
                    CancellationToken = spec.CancellationToken
                }
                : new FileDownloadSpec(_authProvider.HandleUri(spec.Uri), spec.LocalFile, spec.Progress) {
                    Verification = spec.Verification,
                    CancellationToken = spec.CancellationToken
                };
        }
    }

    [DoNotObfuscate]
    public abstract class TransferException : Exception
    {
        protected TransferException(string message, Exception inner) : base(message, inner) {}
        protected TransferException(string message) : base(message) {}
        protected TransferException() {}
    }

    [DoNotObfuscate]
    public class ProtocolNotSupported : TransferException
    {
        public ProtocolNotSupported() {}
        public ProtocolNotSupported(string scheme) : base(scheme) {}
    }

    [DoNotObfuscate]
    public class DownloadException : TransferException
    {
        public DownloadException() {}

        public DownloadException(string message, Exception inner = null)
            : base(message, inner) {}

        public DownloadException(string message, string output = null, string parameters = null,
            Exception inner = null)
            : base(message, inner) {
            Output = output;
            Parameters = parameters;
        }

        public string Output { get; protected set; }
        public string Parameters { get; protected set; }
    }

    public class HttpDownloadException : DownloadException
    {
        public HttpStatusCode StatusCode;
        public HttpDownloadException(string message, WebException webException) : base(message, webException) {}
    }

    public class FtpDownloadException : DownloadException
    {
        public FtpStatusCode StatusCode;
        public FtpDownloadException(string message, WebException webException) : base(message, webException) {}
    }

    [DoNotObfuscate]
    public class DownloadSoftException : DownloadException
    {
        public DownloadSoftException() {}

        public DownloadSoftException(string message, string output = null, string parameters = null,
            Exception inner = null)
            : base(message, output, parameters, inner) {}
    }
}