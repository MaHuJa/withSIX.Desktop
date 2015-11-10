// <copyright company="SIX Networks GmbH" file="RsyncUploadProtocolTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols
{
    [TestFixture]
    public class RsyncUploadProtocolTest : UploadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _rsyncLauncher = A.Fake<IRsyncLauncher>();
            Strategy = new RsyncUploadProtocol(_rsyncLauncher);
        }

        IRsyncLauncher _rsyncLauncher;


        [Test]
        public void CanUploadRsync() {
            Strategy.Upload(new FileUploadSpec("b", new Uri("rsync://host/b")));

            A.CallTo(() => _rsyncLauncher.RunAndProcess(A<ITransferProgress>._, "b", "rsync://host/b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanUploadRsyncAsync() {
            await Strategy.UploadAsync(new FileUploadSpec("b", new Uri("rsync://host/b")));

            A.CallTo(() => _rsyncLauncher.RunAndProcessAsync(A<ITransferProgress>._, "b", "rsync://host/b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanUploadRsyncAsyncWithProgress() {
            var progress = A.Fake<ITransferProgress>();

            await Strategy.UploadAsync(new FileUploadSpec("b", "rsync://host/b", progress));

            A.CallTo(() => _rsyncLauncher.RunAndProcessAsync(progress, "b", "rsync://host/b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Upload(new FileUploadSpec("a", new Uri("someprotocol://host/a")));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}