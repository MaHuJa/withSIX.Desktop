// <copyright company="SIX Networks GmbH" file="GameLauncherServiceTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Services
{
    /* // TODO
    [TestFixture]
    public class GameLauncherServiceTest
    {
        [SetUp]
        public void SetUp() {
            _processManager = A.Fake<IProcessManager>();
            _eventAggregator = A.Fake<IEventAggregator>();
            _fileWriter = A.Fake<IGameFileWriterService>();
            _gameLauncher = new GameLauncherService(_processManager, _eventAggregator, _fileWriter);

            _fakeProcess = Process.GetCurrentProcess();
            A.CallTo(() => _processManager.Start(A<ProcessStartInfo>._))
                .Returns(_fakeProcess);
        }

        IProcessManager _processManager;
        IEventAggregator _eventAggregator;
        GameLauncherService _gameLauncher;
        Process _fakeProcess;
        IGameFileWriterService _fileWriter;

        [Test]
        public void CanDispatchLaunch() {
            var launchable = A.Fake<ILaunchable>();
            _gameLauncher.Launch(launchable);

            A.CallTo(() => launchable.Launch(_gameLauncher))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanDispatchLaunchWithIFileWriter() {
            var launchable = A.Fake<ILaunchableRequireFileWriter>();
            _gameLauncher.Launch(launchable);

            A.CallTo(() => launchable.LaunchWith(_gameLauncher, A<IGameFileWriterService>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task CanLaunchDefault() {
            _gameLauncher.Launch(new LaunchGameCommand(new FileInfo("someExecutable"),
                new DirectoryInfo("someWorkingDirectory"), new[] {"launchParam1"}));

            A.CallTo(
                () => _processManager.Start(A<ProcessStartInfo>.That.Matches(x =>
                    x.FileName.Equals("someExecutable")
                    && x.WorkingDirectory.Equals("someWorkingDirectory")
                    && x.Arguments.Contains("launchParam1"))));
            A.CallTo(() => _eventAggregator.Publish(A<GameLaunchedEvent>.That.Matches(x => x.Id == _fakeProcess.Id)));
        }

        [Test, Ignore("")]
        public async Task CanLaunchWithJava() {}

        [Test, Ignore("")]
        public async Task CanLaunchWithSteam() {}

        [Test, Ignore("")]
        public async Task CanLaunchWithSteamLegacy() {}
    }
    */
}