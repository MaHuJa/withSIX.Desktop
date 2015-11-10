// <copyright company="SIX Networks GmbH" file="Arma2GameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Arma 2")]
    public class Arma2GameTest
    {
        [SetUp]
        public void SetUp() {
            _game = new Arma2Game(Guid.NewGuid(), new GameSettingsController());
            _settings = _game.Settings;
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
        }

        ArmaSettings _settings;
        Arma2Game _game;

        [Test]
        public async Task CanLaunch() {
            _game.Settings.LaunchUsingSteam = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGameWithSteamLegacy(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\arma2.exe");
            parameters.SteamAppId.Should().Be(33910);
        }

        [Test]
        public async Task CanLaunchWithServerMode() {
            _settings.ServerMode = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\arma2server.exe");
        }

        [Test]
        public async Task CanQueryServers() {
            var mediator = A.Fake<IGameServerQueryHandler>();
            var info = await mediator.FakeGamespyServerQuery(_game).ConfigureAwait(false);
            info.Tag.Should().Be("arma2pc");
        }

        [Test]
        public void HasCorrectMetaData() {
            _game.MetaData.Name.Should().Be("Arma 2");
            _game.MetaData.Author.Should().Be("Bohemia Interactive");
        }

        [Test]
        public void SupportsCorrectMissionTypes([Values(GameMissionType.Arma2Mission)] GameMissionType type) {
            _game.SupportsContent(new Mission(Guid.Empty) { ContentType = type }).Should().BeTrue();
        }

        [Test]
        public void SupportsCorrectModTypes(
            [Values(GameModType.Arma2Mod, GameModType.Arma2StMod, GameModType.Arma2CaMod, GameModType.Rv3Mod,
                GameModType.Rv3MinMod, GameModType.Rv2MinMod, GameModType.RvMinMod)] GameModType type) {
                    _game.SupportsContent(new Mod(Guid.Empty) { Type = type }).Should().BeTrue();
        }
    }
}