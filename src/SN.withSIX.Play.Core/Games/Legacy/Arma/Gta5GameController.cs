// <copyright company="SIX Networks GmbH" file="Gta5GameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Sync.Core.Packages;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma
{
    public class Gta5GameController : GameController
    {
        static readonly Guid modRepo = GameUuids.GTAV;
        static readonly Dictionary<Guid, Uri[]> defaultRemotes = new[] {
            GetSet(modRepo)
            //GetSet(MissionRepo)
        }.ToDictionary(x => x.Key, x => x.Value);
        static readonly Dictionary<Guid, Uri[]> premiumRemotes = new[] {
            GetPremiumSet(modRepo)
            //GetPremiumSet(MissionRepo)
        }.ToDictionary(x => x.Key, x => x.Value);
        public Gta5GameController(ISupportContent game) : base(game) {}
        protected override IEnumerable<KeyValuePair<Guid, Uri[]>> DefaultRemotes
        {
            get { return defaultRemotes; }
        }
        protected override IEnumerable<KeyValuePair<Guid, Uri[]>> PremiumRemotes
        {
            get { return premiumRemotes; }
        }

        protected override async Task<BundleManager> CreateRepoIfNotExistent(ContentPaths modPaths,
            IEnumerable<KeyValuePair<Guid, Uri[]>> remotes) {
            var bm = await base.CreateRepoIfNotExistent(modPaths, remotes).ConfigureAwait(false);
            Game.WhenAny(x => x.PrimaryContentPath.Path, x => x.Value).Subscribe(x => {
                bm.PackageManager.Settings.GlobalWorkingPath = x;
                bm.PackageManager.Settings.CheckoutType = CheckoutType.CheckoutWithoutRemoval;
            });
            return bm;
        }
    }
}