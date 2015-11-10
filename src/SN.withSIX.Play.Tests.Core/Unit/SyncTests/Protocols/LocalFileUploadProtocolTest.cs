// <copyright company="SIX Networks GmbH" file="LocalFileUploadProtocolTest.cs">
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
    public class LocalFileUploadProtocolTest : UploadProtocolTest
    {
        [SetUp]
        public void SetUp() {
            _fileCopy = A.Fake<ICopyFile>();
            Strategy = new LocalFileUploadProtocol(_fileCopy);
        }

        ICopyFile _fileCopy;


        [Test]
        public void CanUploadLocalFile() {
            Strategy.Upload(new FileUploadSpec("c:/d", "file://c:/a/d"));

            A.CallTo(() => _fileCopy.CopyFile("C:\\d".ToAbsoluteFilePath(), "c:/a/d".ToAbsoluteFilePath()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanÙploadLocalFileAsync() {
            Strategy.UploadAsync(new FileUploadSpec("C:/d", "file://c:/a/d"));

            A.CallTo(() => _fileCopy.CopyFileAsync("c:/d".ToAbsoluteFilePath(), "c:/a/d".ToAbsoluteFilePath()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ThrowsOnProtocolMismatch() {
            Action act = () => Strategy.Upload(new FileUploadSpec("c:/a", new Uri("someprotocol://host/a")));

            act.ShouldThrow<ProtocolMismatch>();
        }
    }
}