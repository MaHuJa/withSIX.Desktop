// <copyright company="SIX Networks GmbH" file="HttpUploadProtocolTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Play.Tests.Core.Support;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols
{
    [TestFixture]
    public class HttpUploadProtocolTest : UploadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _webClient = A.Fake<IWebClient>();
            var factory = SharedSupport.CreateFakeWebClientExportFactory(_webClient);
            Strategy = new HttpUploadProtocol(factory);
        }

        IWebClient _webClient;
        const string LocalFile = "a";
        const string URL = "ftp://host/a";

        [Test]
        public void CanUploadFtp() {
            Strategy.Upload(new FileUploadSpec(LocalFile, URL));

            A.CallTo(() => _webClient.UploadFileTaskAsync(new Uri(URL), LocalFile))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanUploadFtpAsync() {
            await Strategy.UploadAsync(new FileUploadSpec(LocalFile, URL));

            A.CallTo(() => _webClient.UploadFileTaskAsync(new Uri(URL), LocalFile))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanUploadHttp() {
            Strategy.Upload(new FileUploadSpec(LocalFile, "http://host/a"));

            A.CallTo(() => _webClient.UploadFileTaskAsync(new Uri("http://host/a"), LocalFile))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanUploadHttpAsync() {
            await Strategy.UploadAsync(new FileUploadSpec(LocalFile, "http://host/a"));

            A.CallTo(() => _webClient.UploadFileTaskAsync(new Uri("http://host/a"), LocalFile))
                .MustHaveHappened(Repeated.Exactly.Once);
        }


        [Test]
        public void CanUploadHttps() {
            Strategy.Upload(new FileUploadSpec(LocalFile, "https://host/a"));

            A.CallTo(() => _webClient.UploadFileTaskAsync(new Uri("https://host/a"), LocalFile))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanUploadHttpsAsync() {
            await Strategy.UploadAsync(new FileUploadSpec(LocalFile, "https://host/a"));

            A.CallTo(() => _webClient.UploadFileTaskAsync(new Uri("https://host/a"), LocalFile))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Upload(new FileUploadSpec(LocalFile, new Uri("someprotocol://host/a")));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}