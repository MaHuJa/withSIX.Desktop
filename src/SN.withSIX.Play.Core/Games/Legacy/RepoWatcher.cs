// <copyright company="SIX Networks GmbH" file="RepoWatcher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Legacy.Status;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class RepoWatcher : IDisposable
    {
        const int TimerTime = 150;
        TimerWithElapsedCancellation _timer;

        public RepoWatcher(StatusRepo repo) {
            _timer = new TimerWithElapsedCancellation(TimerTime, () => {
                repo.UpdateTotals();
                return true;
            });
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing)
                return;
            if (_timer == null)
                return;

            _timer.Dispose();
            _timer = null;
        }
    }
}