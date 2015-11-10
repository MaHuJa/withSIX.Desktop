// <copyright company="SIX Networks GmbH" file="CarrierCommandSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class CarrierCommandSettings : RealVirtualitySettings
    {
        public CarrierCommandSettings(Guid gameId, CarrierCommandStartupParmeters sp, GameSettingsController controller)
            : base(gameId, sp, controller) {
            StartupParameters = sp;
        }

        public new CarrierCommandStartupParmeters StartupParameters { get; private set; }
    }

    // TODO
    public class CarrierCommandStartupParmeters : ArmaStartupParams
    {
        public CarrierCommandStartupParmeters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}