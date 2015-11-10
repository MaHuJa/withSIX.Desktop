// <copyright company="SIX Networks GmbH" file="UploaderTest.cs">
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

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class UploaderTest
    {
        [SetUp]
        public void SetUp() {
            _strategy = A.Fake<IUploadProtocol>();
            A.CallTo(() => _strategy.Schemes)
                .Returns(new[] {"http"});

            var authProvider = A.Fake<IAuthProvider>();
            A.CallTo(() => authProvider.HandleUri(new Uri(TestUrl)))
                .Returns(new Uri(TestUrl));

            _uploader = new FileUploader(new[] {_strategy}, authProvider);
        }

        IUploadProtocol _strategy;
        FileUploader _uploader;
        const string TestUrl = "http://remoteb";

        static FileUploadSpec GetUploadSpec(IAbsoluteFilePath localFile, string remoteFile, ITransferProgress progress) {
            return
                A<FileUploadSpec>.That.Matches(
                    x => x.LocalFile == localFile && x.Uri == new Uri(remoteFile) && x.Progress == progress);
        }

        static FileUploadSpec GetUploadSpec(IAbsoluteFilePath localFile, string remoteFile) {
            return
                A<FileUploadSpec>.That.Matches(
                    x => x.LocalFile == localFile && x.Uri == new Uri(remoteFile));
        }

        [Test]
        public async Task CanUploadAsyncWithRegisteredProtocol() {
            await _uploader.UploadAsync("C:\\locala".ToAbsoluteFilePath(), TestUrl);

            A.CallTo(() => _strategy.UploadAsync(GetUploadSpec("C:\\locala".ToAbsoluteFilePath(), TestUrl)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }


        [Test]
        public async Task CanUploadAsyncWithRegisteredProtocolWithProgress() {
            var progress = A.Fake<ITransferProgress>();

            await _uploader.UploadAsync("C:\\locala".ToAbsoluteFilePath(), TestUrl, progress);

            A.CallTo(() => _strategy.UploadAsync(GetUploadSpec("C:\\locala".ToAbsoluteFilePath(), TestUrl, progress)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanUploadWithRegisteredProtocol() {
            _uploader.Upload("C:\\locala".ToAbsoluteFilePath(), TestUrl);

            A.CallTo(() => _strategy.Upload(GetUploadSpec("C:\\locala".ToAbsoluteFilePath(), TestUrl)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanUploadWithRegisteredProtocolWithProgress() {
            var progress = A.Fake<ITransferProgress>();

            _uploader.Upload("C:\\locala".ToAbsoluteFilePath(), TestUrl, progress);

            A.CallTo(() => _strategy.Upload(GetUploadSpec("C:\\locala".ToAbsoluteFilePath(), TestUrl, progress)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }


        [Test]
        public void ThrowsOnUnknownScheme() {
            Action act = () => _uploader.Upload("C:\\a".ToAbsoluteFilePath(), "rsync://a");

            act.ShouldThrow<ProtocolNotSupported>();
        }
    }
}