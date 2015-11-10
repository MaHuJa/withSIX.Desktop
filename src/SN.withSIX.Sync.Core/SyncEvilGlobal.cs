// <copyright company="SIX Networks GmbH" file="SyncEvilGlobal.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using SN.withSIX.Core.Services;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core
{
    [Obsolete(
        "This only exists because the classes using this, were not designed to work with DI in mind. This should however be rectified asap.."
        )]
    public static class SyncEvilGlobal
    {
        public static IFileDownloadHelper DownloadHelper { get; private set; }
        public static IFileDownloader FileDownloader { get; private set; }
        public static IStringDownloader StringDownloader { get; private set; }
        public static Func<ExportLifetimeContext<IHostChecker>> GetHostChecker { get; private set; }

        public static void Setup(EvilGlobalServices services) {
            DownloadHelper = services.DownloadHelper;
            FileDownloader = services.Downloader;
            StringDownloader = services.StringDownloader;
            GetHostChecker = services.GetHostChecker;
        }
    }

    [Obsolete(
        "This only exists because the classes using this, were not designed to work with DI in mind. This should however be rectified asap.."
        )]
    public class EvilGlobalServices : IDomainService
    {
        public readonly IFileDownloader Downloader;
        public readonly IFileDownloadHelper DownloadHelper;
        public readonly Func<ExportLifetimeContext<IHostChecker>> GetHostChecker;
        public readonly IStringDownloader StringDownloader;

        public EvilGlobalServices(IFileDownloadHelper downloadHelper, IFileDownloader fileDownloader,
            IStringDownloader stringDownloader,
            Func<ExportLifetimeContext<IHostChecker>> getHostChecker) {
            DownloadHelper = downloadHelper;
            Downloader = fileDownloader;
            StringDownloader = stringDownloader;
            GetHostChecker = getHostChecker;
        }
    }
}