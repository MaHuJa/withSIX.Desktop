// <copyright company="SIX Networks GmbH" file="FileUploadSpec.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NDepend.Path;

namespace SN.withSIX.Sync.Core.Transfer.Specs
{
    public class FileUploadSpec : TransferSpec
    {
        public FileUploadSpec(IAbsoluteFilePath localFile, string url) : this(localFile, new Uri(url), null) {}
        public FileUploadSpec(IAbsoluteFilePath localFile, Uri uri) : this(localFile, uri, null) {}

        public FileUploadSpec(IAbsoluteFilePath localFile, string url, ITransferProgress progress)
            : this(localFile, new Uri(url), progress) {}

        public FileUploadSpec(string localFile, string url) : this(localFile.ToAbsoluteFilePath(), url, null) {}
        public FileUploadSpec(string localFile, Uri uri) : this(localFile.ToAbsoluteFilePath(), uri) {}

        public FileUploadSpec(string localFile, string url, ITransferProgress progress)
            : this(localFile.ToAbsoluteFilePath(), url, progress) {}

        public FileUploadSpec(IAbsoluteFilePath localFile, Uri uri, ITransferProgress progress)
            : base(uri, localFile, progress) {}
    }
}