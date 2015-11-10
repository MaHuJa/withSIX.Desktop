// <copyright company="SIX Networks GmbH" file="ArmaGameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Arma Base")]
    public class ArmaGameTest
    {
        [SetUp]
        public void SetUp() {
            _settings = A.Fake<ArmaSettings>();
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
            _game = new TestArmaGame(Guid.NewGuid(), _settings);
        }

        ArmaSettings _settings;
        TestArmaGame _game;

        public class TestArmaGame : ArmaGame
        {
            static readonly SeparateClientAndServerExecutable executables =
                new SeparateClientAndServerExecutable("arma.exe", "armaserver.exe");
            public TestArmaGame(Guid id, ArmaSettings settings) : base(id, settings) {}

           protected override SeparateClientAndServerExecutable Executables {
                get { return executables; }
            }

            public override GameMetaData MetaData {
                get { throw new NotImplementedException(); }
            }

            protected override RvProfileInfo ProfileInfo {
                get { return new RvProfileInfo("abc", "cba", ".armaprofile"); }
            }
            protected override ServersQuery ServerQueryInfo {
                get { throw new NotImplementedException(); }
            }

            public override Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler service) {
                throw new NotImplementedException();
            }

            public override Task QueryServer(ServerQueryState state) {
                throw new NotImplementedException();
            }

            protected override IEnumerable<GameModType> GetSupportedModTypes() {
                throw new NotImplementedException();
            }

            public override bool SupportsContent(Mission mission) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public async Task CanLaunch() {
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\arma.exe");
        }

        [Test]
        public async Task CanLaunchWithAdditionalParameters() {
            _settings.StartupParameters.StartupLine = "-additionalParam=value -additionalSwitch";
            var parPath = Path.Combine(Path.GetTempPath(), _game.Id + ".txt").ToAbsoluteFilePath();
            var mediator = A.Fake<IRealVirtualityLauncher>();
            A.CallTo(
                () =>
                    mediator.WriteParFile(
                        A<WriteParFileInfo>.That.Matches(
                            x => x.GameId == _game.Id && x.Content == "-mod=\n-additionalparam=value\n-additionalswitch")))
                .ReturnsLazily(() => Task.FromResult(parPath));

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.StartupParameters.Should()
                .BeEquivalentTo(new[] {
                    "-par=" + parPath, "-additionalparam=value",
                    "-additionalswitch"
                });
        }

        [Test]
        public async Task CanLaunchWithServerMode() {
            _settings.ServerMode = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\armaserver.exe");
        }
    }
}