// <copyright company="SIX Networks GmbH" file="QueueDownloaderTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class QueueDownloaderTest
    {
        [SetUp]
        public void SetUp() {
            Downloader = A.Fake<IMultiMirrorFileDownloader>();
        }

        protected IMultiMirrorFileDownloader Downloader;
        protected IFileQueueDownloader QDownloader;

        static IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> GetDefaultDownloads() {
            return new[] {"C:\\a", "c:\\b"}.Select(
                x =>
                    new KeyValuePair<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus>(
                        new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(x, null), new TransferStatus(x)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        protected static FileQueueSpec GetDefaultSpec() {
            return new FileQueueSpec(GetDefaultDownloads(), @"C:\temp");
        }

        static MultiMirrorFileDownloadSpec GetDownloadSpec(string remoteFile) {
            return A<MultiMirrorFileDownloadSpec>.That.Matches(x => x.RemoteFile == remoteFile);
        }

        protected virtual void VerifyDownloads() {
            A.CallTo(() => Downloader.Download(GetDownloadSpec("C:\\a")))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => Downloader.Download(GetDownloadSpec("C:\\b")))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        protected void VerifyAsyncDownloads() {
            A.CallTo(() => Downloader.DownloadAsync(GetDownloadSpec("C:\\a")))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => Downloader.DownloadAsync(GetDownloadSpec("C:\\b")))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        protected virtual IFileQueueDownloader GetQueueDownloader() {
            return new FileQueueDownloader(Downloader);
        }

        [Test]
        public void CanCancelDownloadFilesAsync() {
            var token = new CancellationTokenSource();
            QDownloader = GetQueueDownloader();
            token.Cancel();

            Func<Task> act = () => QDownloader.DownloadAsync(GetDefaultSpec(), token.Token);

            act.ShouldThrow<OperationCanceledException>();
            token.Dispose();
        }

        [Test]
        public async Task CanDownloadFilesAsync() {
            QDownloader = GetQueueDownloader();

            await QDownloader.DownloadAsync(GetDefaultSpec());

            VerifyAsyncDownloads();
        }
    }
}