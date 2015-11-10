// <copyright company="SIX Networks GmbH" file="ServerQueryResult.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Servers;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public abstract class ServerQueryResult
    {
        protected ServerQueryResult(IDictionary<string, string> settings, bool isMasterResult = false) {
            Contract.Requires<ArgumentNullException>(settings != null);
            Settings = settings;
            IsMasterResult = isMasterResult;
        }

        public bool IsMasterResult { get; private set; }
        public long Ping { get; set; } = Common.MagicPingValue;
        public IDictionary<string, string> Settings { get; }
        public Player[] Players { get; set; }
        public abstract ServerQueryMode Mode { get; }

        public string GetSettingOrDefault(string settingName) {
            return Settings.ContainsKey(settingName) ? Settings[settingName] : null;
        }
    }

    public class GamespyServerQueryResult : ServerQueryResult
    {
        public GamespyServerQueryResult(IDictionary<string, string> settings, bool isMasterResult = false)
            : base(settings, isMasterResult) {}

        public override ServerQueryMode Mode
        {
            get { return ServerQueryMode.Gamespy; }
        }
    }

    public class SourceServerQueryResult : ServerQueryResult
    {
        public SourceServerQueryResult(IDictionary<string, string> settings, bool isMasterResult = false)
            : base(settings, isMasterResult) {}

        public override ServerQueryMode Mode
        {
            get { return ServerQueryMode.Steam; }
        }
    }

    public class SourceMasterServerQueryResult : ServerQueryResult
    {
        public SourceMasterServerQueryResult(IDictionary<string, string> settings) : base(settings, true) {}
        public override ServerQueryMode Mode
        {
            get { return ServerQueryMode.Steam; }
        }
    }
}