// <copyright company="SIX Networks GmbH" file="ZsyncDownloadProtocolTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols
{
    [TestFixture]
    public class ZsyncDownloadProtocolTest : DownloadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _zsyncLauncher = A.Fake<IZsyncLauncher>();
            Strategy = new ZsyncDownloadProtocol(_zsyncLauncher);
        }

        IZsyncLauncher _zsyncLauncher;

        [Test]
        public void CanDownloadZsync() {
            Strategy.Download(new FileDownloadSpec(new Uri("zsync://host/c"), "c"));

            A.CallTo(() => _zsyncLauncher.RunAndProcess(A<ITransferProgress>._, new Uri("http://host/c.zsync"), "c"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadZsyncAsync() {
            await Strategy.DownloadAsync(new FileDownloadSpec(new Uri("zsync://host/c"), "c"));

            A.CallTo(
                () => _zsyncLauncher.RunAndProcessAsync(A<ITransferProgress>._, new Uri("http://host/c.zsync"), "c"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadZsyncAsyncWithProgress() {
            var progress = A.Fake<ITransferProgress>();

            await
                Strategy.DownloadAsync(new FileDownloadSpec(new Uri("zsync://host/c"), "c:\\a".ToAbsoluteFilePath(),
                    progress));

            A.CallTo(() => _zsyncLauncher.RunAndProcessAsync(progress, new Uri("http://host/c.zsync"), "C:\\c"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Download(new FileDownloadSpec("someprotocol://host/a", "C:\\a"));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}