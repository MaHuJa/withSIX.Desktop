// <copyright company="SIX Networks GmbH" file="HttpDownloadProtocolTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Core;
using SN.withSIX.Play.Tests.Core.Support;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols
{
    [TestFixture]
    public class HttpDownloadProtocolTest : DownloadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _webClient = A.Fake<IWebClient>();
            var factory = SharedSupport.CreateFakeWebClientExportFactory(_webClient);
            Strategy = new HttpDownloadProtocol(factory, A.Fake<Tools.FileTools.IFileOps>());
        }

        IWebClient _webClient;
        const string URL = "ftp://host/a";
        const string LocalFile = "a";
        const string LocalFileTmp = LocalFile + ".sixtmp";

        [Test]
        public void CanDownloadFtp() {
            Strategy.Download(new FileDownloadSpec(URL, LocalFile));

            A.CallTo(() => _webClient.DownloadFileTaskAsync(new Uri(URL), LocalFileTmp))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadFtpAsync() {
            await Strategy.DownloadAsync(new FileDownloadSpec(URL, LocalFile));

            A.CallTo(() => _webClient.DownloadFileTaskAsync(new Uri(URL), LocalFileTmp))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanDownloadHttp() {
            Strategy.Download(new FileDownloadSpec("http://host/a", LocalFile));

            A.CallTo(() => _webClient.DownloadFileTaskAsync(new Uri("http://host/a"), LocalFileTmp))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadHttpAsync() {
            await Strategy.DownloadAsync(new FileDownloadSpec("http://host/a", LocalFile));

            A.CallTo(() => _webClient.DownloadFileTaskAsync(new Uri("http://host/a"), LocalFileTmp))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadHttpAsyncWithProgress() {
            await Strategy.DownloadAsync(new FileDownloadSpec("http://host/a", LocalFile, A.Fake<ITransferProgress>()));

            A.CallTo(() => _webClient.DownloadFileTaskAsync(new Uri("http://host/a"), LocalFileTmp))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanDownloadHttpWithProgress() {
            Strategy.Download(new FileDownloadSpec("http://host/a", LocalFile, A.Fake<ITransferProgress>()));

            A.CallTo(() => _webClient.DownloadFileTaskAsync(new Uri("http://host/a"), LocalFileTmp))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Download(new FileDownloadSpec("someprotocol://host/a", LocalFile));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}