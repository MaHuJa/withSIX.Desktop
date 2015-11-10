// <copyright company="SIX Networks GmbH" file="IDataDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IDataDownloader
    {
        byte[] Download(Uri uri);
        Task<byte[]> DownloadAsync(Uri uri);
        byte[] Download(Uri uri, ITransferProgress progress);
        Task<byte[]> DownloadAsync(Uri uri, ITransferProgress progress);
    }
}