// <copyright company="SIX Networks GmbH" file="LegacyGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [Obsolete("Should be migrated to new game settings"),
     DataContract(Name = "GameSettings",
         Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class LegacyGameSettings : IComparePK<LegacyGameSettings>
    {
        bool? _includeServerMods = true;

        public LegacyGameSettings(Game game) {
            Contract.Requires<ArgumentNullException>(game != null);

            ServerFilter = new ArmaServerFilter();

            Uuid = game.Id;
        }

        [DataMember]
        public Guid Uuid { get; private set; }
        [DataMember]
        public string GamePath { get; set; }
        [DataMember]
        public string ModPath { get; set; }
        [DataMember]
        public string SynqPath { get; set; }
        [DataMember]
        public string StartupParameters { get; set; }
        [DataMember]
        public string AdditionalMods { get; set; }
        [DataMember]
        public ArmaServerFilter ServerFilter { get; set; }
        [DataMember]
        public bool ServerMode { get; set; }
        [DataMember]
        public bool ResetGameKeyEachLaunch { get; set; }
        [DataMember]
        public bool ForceRunAsAdministrator { get; set; }
        [DataMember]
        public bool IncludeServerMods
        {
            get { return _includeServerMods.GetValueOrDefault(true); }
            set { _includeServerMods = value; }
        }
        [DataMember]
        public bool LaunchUsingSteam { get; set; }

        public bool ComparePK(LegacyGameSettings other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;

            if (other.Uuid == default(Guid) || Uuid == default(Guid))
                return false;
            return other.Uuid.Equals(Uuid);
        }

        public virtual bool ComparePK(object obj) {
            var emp = obj as LegacyGameSettings;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context) {
            if (ServerFilter == null)
                ServerFilter = new ArmaServerFilter();
        }
    }
}