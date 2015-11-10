// <copyright company="SIX Networks GmbH" file="RsyncLauncherTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Core;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Tests.Core.Support;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.Protocols.Handlers
{
    [TestFixture]
    public class RsyncLauncherTest
    {
        [SetUp]
        public void Setup() {
            _processManager = A.Fake<IProcessManager>();
            _pathConfiguration = new PathConfiguration();
            CommonBase.AssemblyLoader = SharedSupport.GetAssemblyLoader();
            _pathConfiguration.SetPaths();
            _tempRsyncExe = _pathConfiguration.ToolCygwinBinPath.GetChildFileWithName("rsync.exe");
            _launcher = new RsyncLauncher(_processManager, _pathConfiguration, new RsyncOutputParser());
        }

        RsyncLauncher _launcher;
        IAbsoluteFilePath _tempRsyncExe;
        IProcessManager _processManager;
        PathConfiguration _pathConfiguration;
        const string CygdriveCKey = "/cygdrive/c/key";
        const string SshKeyStr = "-o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no";

        string GetSshStr() {
            return string.Format(" -e \"'{0}' {1} -i '{2}'\"",
                _pathConfiguration.ToolCygwinBinPath.GetChildFileWithName("ssh.exe"),
                SshKeyStr, CygdriveCKey);
        }

        [Test]
        public void CanRun() {
            _launcher.Run("patha", "pathb");

            /*
             * TODO
            A.CallTo(() => {
                var startInfo = new ProcessStartInfoBuilder(_tempRsyncExe, RsyncLauncher.DEFAULT_RSYNC_PARAMS + " \"patha\" \"pathb\"") {
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                }.Build();
                return _processManager.LaunchAndGrab(startInfo,
                    A<TimeSpan>._,
                    A<TimeSpan>._);
            })
                .MustHaveHappened(Repeated.Exactly.Once);
                         */
        }

        [Test]
        public void CanRunWithOptionalKey() {
            _launcher.Run("patha", "pathb", @"C:\key");

            /*
             * TODO
            A.CallTo(() => {
                var startInfo = new ProcessStartInfoBuilder(_tempRsyncExe, RsyncLauncher.DEFAULT_RSYNC_PARAMS +
                                                                           GetSshStr() + " \"patha\" \"pathb\"") {
                                                                                                  WorkingDirectory = Directory.GetCurrentDirectory(),
                                                                                              }.Build();
                return _processManager.LaunchAndGrab(startInfo,
                    A<TimeSpan?>._,
                    A<TimeSpan?>._);
            })
                .MustHaveHappened(Repeated.Exactly.Once);
                         */
        }
    }


    [TestFixture]
    public class RsyncOutputParserTest
    {
        [Test]
        public void ParseOutput() {
            var parser = new RsyncOutputParser();
            var progress = A.Fake<ITransferProgress>();
            var process = new Process();
            parser.ParseOutput(process, "3.51M  43%  177.98kB/s    0:00:25", progress);

            progress.Progress.Should().Be(43);
            progress.Speed.Should().Be((long) (177.98*1024));
            progress.Eta.Should().Be(TimeSpan.FromSeconds(25));
        }
    }
}