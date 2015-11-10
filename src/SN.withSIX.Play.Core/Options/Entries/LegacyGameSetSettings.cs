// <copyright company="SIX Networks GmbH" file="LegacyGameSetSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Entries
{
    //            var oldSettings = _settings.GameOptions.GetGameSetSettings(this);
    //            if (oldSettings == null)
    //                return;
    //            Settings.Recent.Mission = oldSettings.RecentMission;
    //            Settings.Recent.ModSet = oldSettings.RecentModSet;
    //            Settings.Recent.Server = oldSettings.RecentServer;
    //            _settings.GameOptions.RemoveGameSettingsOld(this);
    [Obsolete("Should be migrated to new game settings store"),
     DataContract(Name = "GameSetSettings",
         Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models"
         )]
    public class LegacyGameSetSettings : IComparePK<LegacyGameSetSettings>
    {
        public LegacyGameSetSettings(Game game) {
            Contract.Requires<ArgumentNullException>(game != null);

            Uuid = game.Id;
        }

        [DataMember]
        public Guid Uuid { get; private set; }
        [DataMember(Name = "RecentModSet")]
        public RecentCollection RecentCollection { get; set; }
        [DataMember]
        public RecentServer RecentServer { get; set; }
        [DataMember]
        public RecentMission RecentMission { get; set; }

        public bool ComparePK(LegacyGameSetSettings other) {
            if (other == null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            if (other.Uuid == default(Guid) || Uuid == default(Guid))
                return false;
            return other.Uuid.Equals(Uuid);
        }

        public virtual bool ComparePK(object obj) {
            var emp = obj as LegacyGameSetSettings;
            return emp != null && ComparePK(emp);
        }

        public bool Matches(Game game) {
            return game != null && game.Id == Uuid;
        }
    }
}