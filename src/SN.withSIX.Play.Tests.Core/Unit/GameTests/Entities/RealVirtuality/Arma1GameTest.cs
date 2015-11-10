// <copyright company="SIX Networks GmbH" file="Arma1GameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Arma 1")]
    public class Arma1GameTest
    {
        [SetUp]
        public void SetUp() {
            _game = new Arma1Game(Guid.NewGuid(), new GameSettingsController());
            _settings = _game.Settings;
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
        }

        ArmaSettings _settings;
        Arma1Game _game;
        static readonly GameModType[] supportedModTypes = {
            GameModType.Arma1Mod, GameModType.Arma1StMod, GameModType.Rv2Mod, GameModType.Rv2MinMod,
            GameModType.RvMinMod
        };
        static readonly GameModType[] notSupportedModTypes =
            ((GameModType[]) Enum.GetValues(typeof (GameModType))).Except(supportedModTypes).ToArray();

        [Test]
        public async Task CanLaunch() {
            _game.Settings.LaunchUsingSteam = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGameWithSteamLegacy(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\arma.exe");
            parameters.SteamAppId.Should().Be(33910);
        }

        [Test]
        public async Task CanLaunchWithServerMode() {
            _settings.ServerMode = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game).ConfigureAwait(false);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\armaserver.exe");
        }

        [Test]
        public async Task CanQueryServers() {
            var mediator = A.Fake<IGameServerQueryHandler>();
            var info = await mediator.FakeGamespyServerQuery(_game).ConfigureAwait(false);

            info.Tag.Should().Be("armapc");
        }

        [Test]
        public void HasCorrectMetaData() {
            _game.MetaData.Name.Should().Be("ARMA: Armed Assault");
            _game.MetaData.Author.Should().Be("Bohemia Interactive");
        }

        [Test]
        public void NotSupportedModTypes() {
            foreach (var ns in notSupportedModTypes)
                _game.SupportsContent(new Mod(Guid.Empty) { Type = ns }).Should().BeFalse();
        }

        [Test]
        public void SupportsCorrectModTypes() {
            foreach (var s in supportedModTypes)
                _game.SupportsContent(new Mod(Guid.Empty) { Type = s }).Should().BeTrue();
        }
    }
}