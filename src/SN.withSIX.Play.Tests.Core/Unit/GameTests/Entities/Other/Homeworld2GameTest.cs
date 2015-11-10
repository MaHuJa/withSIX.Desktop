// <copyright company="SIX Networks GmbH" file="Homeworld2GameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities.Other;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.Other
{
    [TestFixture, Category("Homeworld 2")]
    public class Homeworld2GameTest
    {
        [SetUp]
        public void SetUp() {
            _game = new Homeworld2Game(Guid.NewGuid(), new GameSettingsController());
            _settings = _game.Settings;
            _game.Settings.Directory = Path.GetTempPath().ToAbsoluteDirectoryPath();
        }

        Homeworld2Settings _settings;
        Homeworld2Game _game;

        [Test]
        public async Task CanLaunch() {
            var mediator = A.Fake<IHomeworld2Launcher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.StartupParameters.Should().BeEquivalentTo("-h", "0", "-w", "0");
            parameters.WorkingDirectory.ToString().Should()
                .Be(Path.Combine(Path.GetTempPath(), @"Bin\Release"),
                    "Homeworld 2 working directory is its executable folder");
            parameters.LaunchExecutable.ToString().Should()
                .Be(Path.Combine(Path.GetTempPath(), @"Bin\Release\Homeworld2.exe"));
        }

        [Test]
        public async Task CanLaunchWithAdditionalParameters() {
            _settings.StartupParameters.StartupLine = "-w 1680 -h 1050";
            var mediator = A.Fake<IHomeworld2Launcher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.StartupParameters.Should().BeEquivalentTo(new[] {"-w", "1680", "-h", "1050"});
        }

        [Test]
        public void HasCorrectMetaData() {
            _game.MetaData.Name.Should().Be("Homeworld 2");
            _game.MetaData.Author.Should().Be("Relic Entertainment");
        }

/*        [Test]
        public void SupportsCorrectModTypes(
            [Values(GameModType.Homeworld2Mod)] GameModType type) {
                _game.SupportsContent(new Mod(Guid.Empty) { Type = type }).Should().BeTrue();
        }*/
    }
}