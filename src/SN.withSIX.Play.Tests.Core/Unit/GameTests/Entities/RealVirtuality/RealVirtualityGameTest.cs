// <copyright company="SIX Networks GmbH" file="RealVirtualityGameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using ShortBus;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Real Virtuality")]
    public class RealVirtualityGameTest
    {
        [SetUp]
        public void SetUp() {
            _settings =
                A.Fake<RealVirtualitySettings>(
                    o =>
                        o.WithArgumentsForConstructor(new Object[]
                        {Guid.NewGuid(), A.Fake<RealVirtualityStartupParameters>(), A.Fake<GameSettingsController>()}));
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
            _game = new TestRealVirtualityGame(Guid.NewGuid(), _settings);
        }

        RealVirtualitySettings _settings;
        TestRealVirtualityGame _game;

        public class TestRealVirtualityGame : RealVirtualityGame
        {
            public TestRealVirtualityGame(Guid id, RealVirtualitySettings settings) : base(id, settings) {}

            protected override IAbsoluteFilePath GetExecutable() {
                return GetGameDirectory().GetChildFileWithName("someGame.exe");
            }

            public override GameMetaData MetaData {
                get { throw new NotImplementedException(); }
            }

            protected override RvProfileInfo ProfileInfo {
                get { throw new NotImplementedException(); }
            }
        }

        [Test]
        public async Task CanLaunch() {
            var launcher = A.Fake<IRealVirtualityLauncher>();
            A.CallTo(() => launcher.Launch(A<LaunchGameInfo>._))
                .ReturnsLazily(() => Task.FromResult(Process.GetCurrentProcess()));
            await _game.Launch(GameTest.GameLauncher(_game, launcher)).ConfigureAwait(false);
        }

        [Test, Ignore("")]
        public async Task CanLaunchWithAdditionalParameters() {
            _settings.StartupParameters.StartupLine = "-additionalParam=value -additionalSwitch";
            var parPath = Path.Combine(Path.GetTempPath(), _game.Id + ".txt").ToAbsoluteFilePath();
            var mediator = A.Fake<IRealVirtualityLauncher>();
            A.CallTo(
                () =>
                    mediator.WriteParFile(
                        A<WriteParFileInfo>.That.Matches(
                            x => x.GameId == _game.Id && x.Content == "-additionalparam=value\n-additionalswitch")))
                .ReturnsLazily(() => Task.FromResult(parPath));

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.StartupParameters.Should()
                .BeEquivalentTo(new[] {
                    "-par=" + parPath, "-additionalparam=value",
                    "-additionalswitch"
                });
        }

        [Test]
        public async Task CanLaunchWithResetGameKeyEachLaunch_LaunchesJustOnce() {
            _settings.ResetGameKeyEachLaunch = true;
            _settings.LaunchUsingSteam = true;
            var launcher = A.Fake<IRealVirtualityLauncher>();
            var specs = new List<LaunchGameInfoBase>();
            A.CallTo(() => launcher.Launch(A<LaunchGameWithSteamLegacyInfo>._))
                .Invokes((IAsyncRequest<Process> x) => specs.Add((LaunchGameInfoBase) x))
                .ReturnsLazily(() => Task.FromResult(Process.GetCurrentProcess()));

            await _game.Launch(GameTest.GameLauncher(_game, launcher)).ConfigureAwait(false);

            specs.Should().HaveCount(1);
            specs[0].StartupParameters.Should()
                .BeEmpty();
        }
    }
}