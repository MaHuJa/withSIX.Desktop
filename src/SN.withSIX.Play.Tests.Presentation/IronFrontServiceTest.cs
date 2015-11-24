// <copyright company="SIX Networks GmbH" file="IronFrontServiceTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Core;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Tests.Presentation
{
    [TestFixture, Ignore("")]
    public class IronFrontServiceTest
    {
        [SetUp]
        public void SetUp() {
            _gameContext = CreateFakeGameList();
            _installer = A.Fake<IronFrontInstaller>();
            _service = CreateIFService();
        }

        IronFrontService CreateIFService() {
            return new IronFrontService(_installer, CreateFakeGameList(), new RepoActionHandler(new BusyStateHandler()));
        }

        Collection GetFakeModSet() {
            var modSetFake = A.Fake<Collection>();
            modSetFake.CallsTo(modSet => modSet.EnabledMods).Returns(CreateIModList("@IF", "@IFA3"));
            return modSetFake;
        }

        IMod[] CreateIModList(params string[] modNames) {
            return modNames.Select(GetFakeMod).ToArray();
        }

        IMod GetFakeMod(string modName) {
            var fakeIfMod = A.Fake<IMod>();
            fakeIfMod.Name = modName;
            return fakeIfMod;
        }

        IGameContext CreateFakeGameList() {
            var gameList = A.Fake<IGameContext>();
            var list = new List<Game> {CreateFakeGame(GameUUids.IronFront), CreateFakeGame(GameUUids.Arma2Oa)};
            /*
            list.Add(CreateFakeGame(GameUUids.Arma1Uuid));
            list.Add(CreateFakeGame(GameUUids.Arma2FreeUuid));
            list.Add(CreateFakeGame(GameUUids.Arma2Uuid));
            list.Add(CreateFakeGame(GameUUids.Arma3Uuid));
            list.Add(CreateFakeGame(GameUUids.DayZSAUuid));
            list.Add(CreateFakeGame(GameUUids.TKOHUuid));
             */

            var dbSet = A.Fake<InMemoryDbSet<Game, Guid>>(o => o.WithArgumentsForConstructor(new Object[] {list}));
            gameList.CallsTo(gl => gl.Games).Returns(dbSet);

            return gameList;
        }

        Game CreateFakeGame(string gameUUid) {
            var game = A.Fake<Game>();
            //game.Id = Guid.Parse(gameUUid);
            return game;
        }

        IGameContext _gameContext;
        IronFrontService _service;
        IronFrontInstaller _installer;

        [Test]
        public void IsIronFrontEnabled_ContainsMods() {
            var modSetFake = GetFakeModSet();
            _service.IsIronFrontEnabled(modSetFake).Should().BeTrue();
        }

/*
        [Test]
        public void IsIronFrontEnabled_IsModset() {
            var modSetFake = A.Fake<Collection>();
            modSetFake.Uuid = "if_a2";
            _service.IsIronFrontEnabled(modSetFake).Should().BeTrue();
        }
*/

        [Test]
        public void IsIronFrontEnabled_NotNull() {
            _service.IsIronFrontEnabled(null).Should().BeFalse();
        }
    }
}