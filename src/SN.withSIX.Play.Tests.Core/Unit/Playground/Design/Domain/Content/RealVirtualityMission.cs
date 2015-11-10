// <copyright company="SIX Networks GmbH" file="RealVirtualityMission.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public abstract class RealVirtualityMission<TGameData> : Mission<TGameData, RealVirtualityLaunchGlobalState>
        where TGameData : RealVirtualityGameData, IMissionGameData
    {
        protected RealVirtualityMission(Guid id, MissionMetaData metaData, PackageItem package)
            : base(id, metaData, package) {}
    }
}