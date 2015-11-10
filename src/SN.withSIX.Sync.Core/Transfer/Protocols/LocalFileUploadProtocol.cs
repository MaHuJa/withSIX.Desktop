// <copyright company="SIX Networks GmbH" file="LocalFileUploadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public class LocalFileUploadProtocol : UploadProtocol
    {
        static readonly IEnumerable<string> schemes = new[] {"file"};
        readonly ICopyFile _fileCopy;

        public LocalFileUploadProtocol(ICopyFile fileCopy) {
            _fileCopy = fileCopy;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        public override void Upload(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            _fileCopy.CopyFile(spec.LocalFile, GetPathFromUri(spec));
        }

        public override Task UploadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            return
                _fileCopy.CopyFileAsync(spec.LocalFile, GetPathFromUri(spec));
        }

        static IAbsoluteFilePath GetPathFromUri(TransferSpec spec) {
            return spec.Uri.LocalPath.ToAbsoluteFilePath();
            // spec.Uri.IsAbsoluteUri ? spec.Uri.AbsolutePath : spec.Uri.ToString();
        }
    }
}