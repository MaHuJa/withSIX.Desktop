// <copyright company="SIX Networks GmbH" file="ISupportMissions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public interface ISupportMissionPublishing
    {
        void PublishMission(string fn);
    }

    public interface ISupportMissions : ISupportMissionPublishing, ISupportContent
    {
        ContentPaths[] MissionPaths { get; }
        bool SupportsContent(Mission mission);
        IEnumerable<LocalMissionsContainer> LocalMissionsContainers();
        void UpdateMissionStates(IReadOnlyCollection<MissionBase> missions);
    }

    public enum GameMissionType
    {
        None,
        Arma2Mission,
        Arma3Mission,
        TakeOnHelicoptersMission,
        IronFrontMission
    }
}