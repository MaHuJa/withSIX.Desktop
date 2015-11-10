// <copyright company="SIX Networks GmbH" file="AllInArmaMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public class AllInArmaMod : RealVirtualityMod<ArmaGameData>
    {
        public AllInArmaMod(Guid id, PackageItem package, ModMetaData metaData) : base(id, package, metaData) {}

        public override Task Install(ArmaGameData gameData, IMediator mediator) {
            // Convert AllInArma
            return base.Install(gameData, mediator);
        }
    }
}