// <copyright company="SIX Networks GmbH" file="LocalFileDownloadProtocolTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols
{
    [TestFixture]
    public class LocalFileDownloadProtocolTest : DownloadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _fileCopy = A.Fake<ICopyFile>();
            Strategy = new LocalFileDownloadProtocol(_fileCopy);
        }

        ICopyFile _fileCopy;

        [Test]
        public void CanDownloadLocalFile() {
            Strategy.Download(new FileDownloadSpec("file://c:/a/d", "c:\\d".ToAbsoluteFilePath()));

            A.CallTo(() => _fileCopy.CopyFile("c:/a/d".ToAbsoluteFilePath(), "C:\\d".ToAbsoluteFilePath()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanDownloadLocalFileAsync() {
            Strategy.DownloadAsync(new FileDownloadSpec("file://c:/a/d", "c:\\d"));

            A.CallTo(() => _fileCopy.CopyFileAsync("c:/a/d".ToAbsoluteFilePath(), "c:\\d".ToAbsoluteFilePath()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Download(new FileDownloadSpec("someprotocol://host/a", "c:/a"));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}