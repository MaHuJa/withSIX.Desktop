// <copyright company="SIX Networks GmbH" file="IFileDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IFileDownloader
    {
        Task DownloadAsync(FileDownloadSpec spec);
        void Download(FileDownloadSpec spec);
    }
}