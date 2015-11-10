// <copyright company="SIX Networks GmbH" file="ZsyncLauncherTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Core;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Tests.Core.Support;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols.Handlers
{
    [TestFixture]
    public class ZsyncLauncherTest
    {
        [SetUp]
        public void Setup() {
            _pathConfiguration = new PathConfiguration();
            Common.AssemblyLoader = SharedSupport.GetAssemblyLoader();
            _pathConfiguration.SetPaths();
        }

        PathConfiguration _pathConfiguration;

        [Test]
        public void CanRun() {
            var process = A.Fake<IProcessManager>();
            var authProvider = A.Fake<IAuthProvider>();
            A.CallTo(() => authProvider.GetAuthInfoFromUri(A<Uri>._))
                .Returns(new AuthInfo(null, null));

            var launcher = new ZsyncLauncher(process, _pathConfiguration, new ZsyncOutputParser(), authProvider);
            launcher.Run(new Uri("http://host/a"), "a");

            /*
             * TODO
            A.CallTo(
                () => {
                    var startInfo = new ProcessStartInfoBuilder(Path.Combine(_pathConfiguration.ToolCygwinBinPath, "zsync.exe"), "-o \"a\" \"http://host/a\"") {
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                    }.Build();
                    return process.LaunchAndGrab(startInfo, null, null);
                })
                .MustHaveHappened(Repeated.Exactly.Once);
             */
        }
    }

    [TestFixture]
    public class ZsyncOutputParserTest
    {
        [Test]
        public void OtherResponseShouldReturnTrue() {
            var parser = new ZsyncOutputParser();

            var output = parser.VerifyZsyncCompatible("some other response");

            output.Should().BeTrue("because the magic string is not found");
        }

        [Test]
        public void ParseOutput() {
            var parser = new ZsyncOutputParser();
            var progress = A.Fake<ITransferProgress>();
            var process = new Process();

            parser.ParseOutput(process, "###################- 97.3% 141.5 kBps 0:00:01 ETA", progress);

            progress.Progress.Should().Be(97.3);
            progress.Speed.Should().Be((long) (141.5*1024));
            progress.Eta.Should().Be(TimeSpan.FromSeconds(1));
        }

        [Test]
        public void PartialContentResponseShouldReturnFalse() {
            var parser = new ZsyncOutputParser();

            var output =
                parser.VerifyZsyncCompatible(
                    "zsync received a data response (code 200) but this is not a partial content response");

            output.Should().BeFalse("because the magic string is found");
        }

        [Test, Ignore]
        public void ZsyncLoopCheck() {
            var parser = new ZsyncOutputParser();
            var progress = A.Fake<ITransferProgress>();
            using (var process = Process.Start("cmd.exe")) {
                var result = parser.CheckZsyncLoop(process, "downloading from ", progress);
                result = parser.CheckZsyncLoop(process, "downloading from ", progress);
                result = parser.CheckZsyncLoop(process, "downloading from ", progress);

                result.Should().BeTrue("because it is looping");

                if (!process.HasExited)
                    process.Kill();
            }
        }
    }
}