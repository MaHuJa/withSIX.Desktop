// <copyright company="SIX Networks GmbH" file="CalculatedGameSettingsUpdated.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class CalculatedGameSettingsUpdated : EventArgs
    {
        public bool OnlyModInfo;

        public CalculatedGameSettingsUpdated(bool onlyModInfo = false, bool modsChanged = false) {
            ModsChanged = modsChanged;
            OnlyModInfo = onlyModInfo;
        }

        public bool ModsChanged { get; set; }
    }
}