// <copyright company="SIX Networks GmbH" file="FileDownloadSpec.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NDepend.Path;

namespace SN.withSIX.Sync.Core.Transfer.Specs
{
    public class FileDownloadSpec : TransferSpec
    {
        public FileDownloadSpec(string url, IAbsoluteFilePath localFile) : this(new Uri(url), localFile, null) {}
        public FileDownloadSpec(Uri uri, IAbsoluteFilePath localFile) : this(uri, localFile, null) {}

        public FileDownloadSpec(string url, IAbsoluteFilePath localFile, ITransferProgress progress)
            : this(new Uri(url), localFile, progress) {}

        public FileDownloadSpec(string url, string localFile) : this(url, localFile.ToAbsoluteFilePath(), null) {}
        public FileDownloadSpec(Uri uri, string localFile) : this(uri, localFile.ToAbsoluteFilePath()) {}

        public FileDownloadSpec(string url, string localFile, ITransferProgress progress)
            : this(url, localFile.ToAbsoluteFilePath(), progress) {}

        public FileDownloadSpec(Uri uri, IAbsoluteFilePath localFile, ITransferProgress progress)
            : base(uri, localFile, progress) {}
    }
}