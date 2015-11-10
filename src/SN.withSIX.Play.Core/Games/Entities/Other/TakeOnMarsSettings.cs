// <copyright company="SIX Networks GmbH" file="TakeOnMarsSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class TakeOnMarsSettings : RealVirtualitySettings
    {
        public TakeOnMarsSettings(Guid gameId, TakeOnMarsStartupParams sp, GameSettingsController controller)
            : base(gameId, sp, controller) {
            StartupParameters = sp;
        }

        public new TakeOnMarsStartupParams StartupParameters { get; private set; }
    }

    // TODO
    public class TakeOnMarsStartupParams : RealVirtualityStartupParameters
    {
        public TakeOnMarsStartupParams(params string[] parameters) : base(parameters) {}
    }
}