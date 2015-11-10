// <copyright company="SIX Networks GmbH" file="IFileQueueDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IFileQueueDownloader
    {
        Task DownloadAsync(FileQueueSpec spec);
        Task DownloadAsync(FileQueueSpec spec, CancellationToken token);
    }

    public class FileQueueSpec
    {
        public readonly IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> Files;
        public readonly IAbsoluteDirectoryPath Location;

        public FileQueueSpec(IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> files,
            IAbsoluteDirectoryPath location) {
            Files = files;
            Location = location;
        }

        public FileQueueSpec(IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> files,
            string location) : this(files, location.ToAbsoluteDirectoryPath()) {}
    }
}