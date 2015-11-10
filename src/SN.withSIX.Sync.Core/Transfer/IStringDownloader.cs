// <copyright company="SIX Networks GmbH" file="IStringDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IStringDownloader
    {
        string Download(Uri uri);
        Task<string> DownloadAsync(Uri uri);
        string Download(Uri uri, ITransferProgress progress);
        Task<string> DownloadAsync(Uri uri, ITransferProgress progress);
    }
}