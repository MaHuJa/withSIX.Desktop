// <copyright company="SIX Networks GmbH" file="RealVirtualityMissionContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class RealVirtualityMissionContainer<TGameData> : MissionContentContainer<RealVirtualityMission<TGameData>>
        where TGameData : RealVirtualityGameData, IMissionGameData
    {
        protected override IEnumerable<RealVirtualityMission<TGameData>> ConstructItems(IEnumerable<Mission> mods) {
            throw new NotImplementedException();
        }
    }
}