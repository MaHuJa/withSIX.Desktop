// <copyright company="SIX Networks GmbH" file="RequestPublishMission.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class RequestPublishMission : EventArgs
    {
        public RequestPublishMission(MissionBase mission) {
            Mission = mission;
        }

        public MissionBase Mission { get; private set; }
    }
}