// <copyright company="SIX Networks GmbH" file="GameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Games.Services.Infrastructure;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities
{
    public class TestGame : Game
    {
        public TestGame(Guid id, GameSettings settings) : base(id, settings) {}

        public override GameMetaData MetaData {
            get { throw new NotImplementedException(); }
        }

        protected override string GetStartupLine() {
            throw new NotImplementedException();
        }

        protected override IAbsoluteFilePath GetExecutable() {
            return GetGameDirectory().GetChildFileWithName("someGame.exe");
        }

        public override Task<int> Launch(IGameLauncherFactory factory) {
            throw new NotImplementedException();
        }

        public override Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory, string identifier) {
            throw new NotImplementedException();
        }
    }

    [TestFixture, Category("Game Base")]
    public class GameTest
    {
        [SetUp]
        public void SetUp() {
            _settings =
                A.Fake<GameSettings>(
                    o =>
                        o.WithArgumentsForConstructor(new Object[]
                        {Guid.NewGuid(), A.Fake<GameStartupParameters>(), A.Fake<GameSettingsController>()}));
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();

            _game = new TestGame(Guid.NewGuid(), _settings);
        }

        GameSettings _settings;
        TestGame _game;

        public static IGameLauncherFactory GameLauncher<T>(ILaunchWith<T> game, T launcher)
            where T : class, IGameLauncher {
            var factory = A.Fake<IGameLauncherFactory>();
            A.CallTo(() => factory.Create(game))
                .Returns(launcher);
            return factory;
        }

        [Test]
        public async Task CanLaunch() {
            A.CallTo(() => _settings.StartupParameters.Get())
                .Returns(new[] {"testParameter"});

            var mediator = A.Fake<IBasicGameLauncher>();
            var parameters = await mediator.FakeLaunchGame(_game).ConfigureAwait(false);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\someGame.exe");
            parameters.WorkingDirectory.ToString().Should().Be(@"C:\temp");
            parameters.StartupParameters.Should().BeEquivalentTo(new[] {"testParameter"});
            parameters.LaunchAsAdministrator.Should().BeFalse();
        }

        [Test]
        public async Task CanLaunchWithAdminRights() {
            _settings.LaunchAsAdministrator = true;

            var mediator = A.Fake<IBasicGameLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game).ConfigureAwait(false);

            parameters.LaunchAsAdministrator.Should().BeTrue();
        }

        [Test]
        public async Task CanLaunchWithInjectSteam() {
            _settings.InjectSteam = true;
            var mediator = A.Fake<IBasicGameLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game).ConfigureAwait(false);

            parameters.InjectSteam.Should().BeTrue();
            parameters.LaunchAsAdministrator.Should().BeFalse();
        }
    }
}