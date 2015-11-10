// <copyright company="SIX Networks GmbH" file="RealVirtualityModContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;
using LegacyMod = SN.withSIX.Play.Core.Games.Legacy.Mods.Mod;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public abstract class RealVirtualityModContainer<TGameData> :
        ContentContainer<RealVirtualityMod<TGameData>, LegacyMod>
        where TGameData : RealVirtualityGameData, IModdingGameData
    {
        protected override IEnumerable<RealVirtualityMod<TGameData>> ConstructItems(IEnumerable<LegacyMod> mods) {
            return mods.Select(ConstructMod);
        }

        RealVirtualityMod<TGameData> ConstructMod(LegacyMod arg) {
            throw new NotImplementedException();
        }
    }
}