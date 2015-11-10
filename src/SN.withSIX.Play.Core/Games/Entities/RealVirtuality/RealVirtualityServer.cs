// <copyright company="SIX Networks GmbH" file="RealVirtualityServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public abstract class RealVirtualityServer<TGame> : Server<TGame> where TGame : RealVirtualityGame, ISupportServers
    {
        protected RealVirtualityServer(TGame game, ServerAddress address)
            : base(game, address) {}

        public override bool HasMod(Collection ms, bool filterModded, bool filterIncompatible) {
            if (ms == null)
                return !Modded;
            //Logger.Debug("!!! ModServerFilter: {0} {1} {2} {3} {4}", this, ms, ms.Mods.FirstOrDefault(), String.Join(";", ms.Aliases), String.Join(";", Mods.ToArray()));

            var customMs = ms as CustomCollection;
            if (customMs != null && customMs.CustomRepo != null)
                return IsOfficial;

            var mod = ms.GetFirstMod();
            if (mod == null)
                return !filterModded || !Modded;

            if (filterIncompatible && !AllowsSignatures())
                return false;

            return !filterModded || HasMod(mod);
        }

        bool AllowsSignatures() {
            if (VerifySignatures < 1)
                return true;

            var signs = Game.CalculatedSettings.Signatures;
            return !signs.Any() || signs
                .None(x => !Signatures.Contains(x, StringComparer.InvariantCultureIgnoreCase));
        }

        bool HasMod(IMod mod) {
            var mods = Mods;

            var gn = mod.Name;
            if (mods.Any(x => x.Equals(gn, StringComparison.OrdinalIgnoreCase)))
                return true;

            var cppName = mod.CppName;
            if (!String.IsNullOrWhiteSpace(cppName)) {
                if (mods.Contains(cppName, StringComparer.OrdinalIgnoreCase))
                    return true;
            }

            var aliases = mod.Aliases;
            if (aliases == null || !aliases.Any())
                return false;
            return mods.Any(x => aliases.Contains(x, StringComparer.OrdinalIgnoreCase));
        }
    }
}