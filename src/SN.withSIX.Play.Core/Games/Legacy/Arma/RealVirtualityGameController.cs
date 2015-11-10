// <copyright company="SIX Networks GmbH" file="RealVirtualityGameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma
{
    public class RealVirtualityGameController : GameController
    {
        static readonly string[] battlEyeExes = {"setup_battleyearma2oa.exe", "setup_battleyearma2rft.exe"};
        static readonly Guid missionRepo = Common.Flags.Staging
            ? new Guid("4f9ee544-70e5-450d-8bdc-38de6415ef57")
            : new Guid("82f4b3b2-ea74-4a7c-859a-20b425caeadb");
        static readonly Guid modRepo = new Guid("1ba63c97-2a18-42a7-8380-70886067582e");
        static readonly Dictionary<Guid, Uri[]> defaultRemotes = new[] {
            GetSet(modRepo),
            GetSet(missionRepo)
        }.ToDictionary(x => x.Key, x => x.Value);
        static readonly Dictionary<Guid, Uri[]> premiumRemotes = new[] {
            GetPremiumSet(modRepo),
            GetPremiumSet(missionRepo)
        }.ToDictionary(x => x.Key, x => x.Value);
        readonly ISupportMissions _missions;

        public RealVirtualityGameController(ISupportContent game) : base(game) {
            _missions = game as ISupportMissions;
        }

        protected override IEnumerable<KeyValuePair<Guid, Uri[]>> DefaultRemotes
        {
            get { return defaultRemotes; }
        }
        protected override IEnumerable<KeyValuePair<Guid, Uri[]>> PremiumRemotes
        {
            get { return premiumRemotes; }
        }

        public async Task<IEnumerable<string>> GetMissionPackages(string gameMissionKey) {
            var index =
                (await PackageManager.Repo.LoadRemotesAsync(missionRepo.ToString()).ConfigureAwait(false)).First().Index;
            return gameMissionKey == null || !index.PackagesContentTypes.ContainsKey(gameMissionKey)
                ? Enumerable.Empty<string>()
                : index.PackagesContentTypes[gameMissionKey];
        }

        public override Task AdditionalHandleModPreRequisites() {
            return WaitOnKillBattleEye();
        }

        Task WaitOnKillBattleEye() {
            return KillBE()
                ? Task.Delay(1500)
                : TaskExt.Default;
        }

        bool KillBE() {
            return battlEyeExes.Select(exe => Tools.Processes.KillByName(exe)).ToArray().Any(x => x);
        }
    }
}