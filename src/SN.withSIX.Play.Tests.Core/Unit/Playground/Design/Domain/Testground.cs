// <copyright company="SIX Networks GmbH" file="Testground.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using ShortBus;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class Testground
    {
        public Testground() {
            var controller =
                new ArmaGameController(new Arma2Game(Guid.NewGuid(), new GameSettingsController()));

            var mod = new RealVirtualityMod<ArmaGameData>(Guid.NewGuid(),
                new PackageItem("abc", new RepositoryHandler(), Enumerable.Empty<SpecificVersion>()), new ModMetaData());

            var mediator = A.Fake<IMediator>(); //new Mediator(new SimpleInjectorDependencyResolver(new Container()));
            controller.Install(mod, mediator);

            var list = new List<RealVirtualityMod<ArmaGameData>> {mod};
            foreach (var i in list)
                controller.Install(i, mediator);
        }
    }
}