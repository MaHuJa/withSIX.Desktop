// <copyright company="SIX Networks GmbH" file="Arma2OaSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma2OaSettings : ArmaSettings
    {
        public Arma2OaSettings(Guid gameId, ArmaStartupParams startupParameters, GameSettingsController controller)
            : base(gameId, startupParameters, controller) {}

        public ServerQueryMode ServerQueryMode
        {
            get { return GetEnum<ServerQueryMode>(); }
            set { SetEnum(value); }
        }
    }

    public class Arma2CoSettings : Arma2OaSettings
    {
        public Arma2CoSettings(Guid gameId, ArmaStartupParams startupParameters, GameSettingsController controller,
            ArmaSettings arma2Settings, Arma2FreeSettings arma2FreeSettings)
            : base(gameId, startupParameters, controller) {
            Arma2Original = arma2Settings;
            Arma2Free = arma2FreeSettings;
        }

        public ArmaSettings Arma2Original { get; set; }
        public Arma2FreeSettings Arma2Free { get; set; }
    }
}