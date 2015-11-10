// <copyright company="SIX Networks GmbH" file="FileUploader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class FileUploader : TransferBase<IUploadProtocol>, IFileUploader
    {
        readonly IAuthProvider _authProvider;

        public FileUploader(IEnumerable<IUploadProtocol> strategies,
            IAuthProvider authProvider) {
            RegisterProtocolStrategies(strategies);
            _authProvider = authProvider;
        }

        public void Upload(FileUploadSpec spec) {
            UploadInternal(spec.LocalFile, spec.Uri, spec.Progress);
        }

        public Task UploadAsync(FileUploadSpec spec) {
            return UploadAsyncInternal(spec.LocalFile, spec.Uri, spec.Progress);
        }

        void UploadInternal(IAbsoluteFilePath localFile, Uri uri, ITransferProgress progress = null) {
            ConfirmStrategySupported(uri);
            GetStrategy(uri).Upload(GetSpec(localFile, uri, progress));
        }

        Task UploadAsyncInternal(IAbsoluteFilePath localFile, Uri uri,
            ITransferProgress progress = null) {
            ConfirmStrategySupported(uri);
            return
                GetStrategy(uri).UploadAsync(GetSpec(localFile, uri, progress));
        }

        FileUploadSpec GetSpec(IAbsoluteFilePath localFile, Uri uri, ITransferProgress progress = null) {
            return progress == null
                ? new FileUploadSpec(localFile, _authProvider.HandleUri(uri))
                : new FileUploadSpec(localFile, _authProvider.HandleUri(uri), progress);
        }

        FileUploadSpec GetSpec(FileUploadSpec spec) {
            return spec.Progress == null
                ? new FileUploadSpec(spec.LocalFile, _authProvider.HandleUri(spec.Uri)) {
                    Verification = spec.Verification,
                    CancellationToken = spec.CancellationToken
                }
                : new FileUploadSpec(spec.LocalFile, _authProvider.HandleUri(spec.Uri), spec.Progress) {
                    Verification = spec.Verification,
                    CancellationToken = spec.CancellationToken
                };
        }
    }

    public class UploadException : TransferException
    {
        public UploadException() {}

        public UploadException(string message, Exception inner = null)
            : base(message, inner) {}

        public UploadException(string message, string output = null, string parameters = null,
            Exception inner = null)
            : base(message, inner) {
            Output = output;
            Parameters = parameters;
        }

        public string Output { get; protected set; }
        public string Parameters { get; protected set; }
    }

    public class UploadSoftException : UploadException
    {
        public UploadSoftException() {}

        public UploadSoftException(string message, string output = null, string parameters = null,
            Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    public class HttpUploadException : UploadException
    {
        public HttpStatusCode StatusCode;
        public HttpUploadException(string message, WebException webException) : base(message, webException) {}
    }

    public class FtpUploadException : UploadException
    {
        public FtpStatusCode StatusCode;
        public FtpUploadException(string message, WebException webException) : base(message, webException) {}
    }
}