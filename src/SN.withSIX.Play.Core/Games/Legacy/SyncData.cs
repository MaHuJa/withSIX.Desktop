// <copyright company="SIX Networks GmbH" file="SyncData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DataContract(Name = "SyncData", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    [Obsolete("Just used for backwards compatibility to import 1.3 custom modsets and repositories")]
    public class SyncData
    {
        static Type[] _knownTypes;
        [DataMember] List<CustomCollection> _CustomModSets;
        [DataMember] Dictionary<string, SixRepo> _Repositories;
        [Obsolete]
        public Dictionary<string, SixRepo> Repositories
        {
            get { return _Repositories; }
            set { _Repositories = value; }
        }
        [Obsolete]
        public List<CustomCollection> CustomModSets
        {
            get { return _CustomModSets; }
            set { _CustomModSets = value; }
        }
    }
}