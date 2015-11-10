// <copyright company="SIX Networks GmbH" file="ArmaServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class ArmaServer : RealVirtualityServer<ArmaGame>
    {
        static readonly string[] sysMods = {
            "Arma 2", "arma2", "expansion", "CA",
            "Arma 2: Operation Arrowhead",
            "Arma 2: British Armed Forces (Lite)",
            "Arma 2: Private Military Company (Lite)",
            "Arma 2: British Armed Forces",
            "Arma 2: Private Military Company",
            "Arma 2: Army of The Czech Republic",
            "Arma 3", "arma3", "Arma 3 Zeus", "Arma 3 Karts", "Arma 3 DLC Bundle"
        };
        public ArmaServer(ArmaGame game, ServerAddress address) : base(game, address) {}

        public override void UpdateModInfo(string modInfo) {
            if (modInfo == null)
                return;

            UpdateModInfo(modInfo.Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
        }

        public override void UpdateModInfo(string[] mods) {
            if (mods == null)
                return;

            mods = mods.Where(x => sysMods.None(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (!mods.SequenceEqual(Mods))
                Mods = mods;
        }
    }
}