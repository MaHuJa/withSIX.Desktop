// <copyright company="SIX Networks GmbH" file="LocalFileDownloadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public class LocalFileDownloadProtocol : DownloadProtocol
    {
        static readonly IEnumerable<string> schemes = new[] {"file"};
        readonly ICopyFile _fileCopy;

        public LocalFileDownloadProtocol(ICopyFile fileCopy) {
            _fileCopy = fileCopy;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        public override void Download(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            _fileCopy.CopyFile(GetPathFromUri(spec), spec.LocalFile);
            VerifyIfNeeded(spec, spec.LocalFile);
        }

        public override async Task DownloadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            await _fileCopy.CopyFileAsync(GetPathFromUri(spec), spec.LocalFile).ConfigureAwait(false);
            VerifyIfNeeded(spec, spec.LocalFile);
        }

        static IAbsoluteFilePath GetPathFromUri(TransferSpec spec) {
            return spec.Uri.LocalPath.ToAbsoluteFilePath();
            //spec.Uri.IsAbsoluteUri ? spec.Uri.AbsolutePath : spec.Uri.ToString();
        }
    }
}