// <copyright company="SIX Networks GmbH" file="RsyncDownloadProtocolTest.cs">
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
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols
{
    [TestFixture]
    public class RsyncDownloadProtocolTest : DownloadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _rsyncLauncher = A.Fake<IRsyncLauncher>();
            Strategy = new RsyncDownloadProtocol(_rsyncLauncher);
        }

        IRsyncLauncher _rsyncLauncher;

        [Test]
        public void CanDownloadRsync() {
            Strategy.Download(new FileDownloadSpec(new Uri("rsync://host/b"), "b"));

            A.CallTo(() => _rsyncLauncher.RunAndProcess(A<ITransferProgress>._, "rsync://host/b", "b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadRsyncAsync() {
            await Strategy.DownloadAsync(new FileDownloadSpec(new Uri("rsync://host/b"), "b"));

            A.CallTo(() => _rsyncLauncher.RunAndProcessAsync(A<ITransferProgress>._, "rsync://host/b", "b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanDownloadRsyncAsyncWithProgress() {
            var progress = A.Fake<ITransferProgress>();

            await
                Strategy.DownloadAsync(new FileDownloadSpec(new Uri("rsync://host/b"), "C:\\b".ToAbsoluteFilePath(),
                    progress));

            A.CallTo(() => _rsyncLauncher.RunAndProcessAsync(progress, "rsync://host/b", "C:\\b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Download(new FileDownloadSpec("someprotocol://host/a", "C:\\a"));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}