// <copyright company="SIX Networks GmbH" file="RepoActionHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;

namespace SN.withSIX.Play.Applications.Services
{
    public class RepoActionHandler : PropertyChangedBase, IRepoActionHandler, IDomainService
    {
        const string StrHostListExhausted =
            "The most common causes for this problem:\n- Security suite interference (Firewall/AntiVirus/etc)\n- Temporary internet connection issues\n- Disk space or permission issues\n- Files/Directories in use because they are open in Windows Explorer or other programs\n- Temporary SIX mirror network overload (try again later)\n- Software bug\n\nSee the troubleshooting guide for more details, solutions and workarounds:\nhttps://community.withsix.com";
        readonly IBusyStateHandler _busyStateHandler;
        StatusMod _activeStatusMod;

        public RepoActionHandler(IBusyStateHandler busyStateHandler) {
            _busyStateHandler = busyStateHandler;
        }

        public StatusMod ActiveStatusMod
        {
            get { return _activeStatusMod; }
            set { SetProperty(ref _activeStatusMod, value); }
        }

        public void PerformStatusActionWithBusyHandling(StatusRepo repo, string actionText, Action act) {
            using (_busyStateHandler.StartSession())
                PerformStatusAction(actionText, act, repo);
        }

        public void PerformStatusAction(string actionText, Action<StatusRepo> act) {
            using (var repo = new StatusRepo())
                PerformStatusAction(actionText, () => act(repo), repo);
        }

        public async Task PerformStatusActionWithBusyHandlingAsync(StatusRepo repo, string actionText, Func<Task> act) {
            using (_busyStateHandler.StartSession())
                await PerformStatusActionAsync(actionText, act, repo).ConfigureAwait(false);
        }

        public async Task PerformStatusActionAsync(string actionText, Func<StatusRepo, Task> act) {
            using (var repo = new StatusRepo())
                await PerformStatusActionAsync(actionText, () => act(repo), repo).ConfigureAwait(false);
        }

        public async Task PerformUpdaterActionSuspendedAsync(string actionText, Func<Task> act) {
            using (_busyStateHandler.StartSession())
            using (_busyStateHandler.StartSuspendedSession())
                await TryUpdaterActionAsync(act, actionText).ConfigureAwait(false);
        }

        public async Task TryUpdaterActionAsync(Func<Task> action, string task) {
            try {
                await action().ConfigureAwait(false);
            } catch (UserDeclinedLicenseException e) {} catch (LicenseRetrievalException e) {
                await UserError.Throw(new InformationalUserError(e,
                    "One or more mod licenses failed to download and display correctly.",
                    "Mod license retrieval failed during " + task));
            } catch (HostListExhausted e) {
                await DealWithUpdateException(e, task);
            } catch (ChecksumException e) {
                await DealWithUpdateException(e, task);
            } catch (IOException e) {
                if (await DealWithUpdateException(e, task))
                    await UserError.Throw(new InformationalUserError(e, "A problem occurred during " + task, null));
            } catch (UnauthorizedAccessException e) {
                if (await DealWithUpdateException(e, task))
                    await UserError.Throw(new InformationalUserError(e, "A problem occurred during " + task, null));
            } catch (Exception e) {
                if (!_busyStateHandler.IsAborted)
                    await UserError.Throw(new InformationalUserError(e, "A problem occurred during " + task, null));
            }
        }

        public async Task<bool> DealWithUpdateException(Exception e, string task, string message = StrHostListExhausted,
            string title = null) {
            if (_busyStateHandler.IsAborted)
                return false;

            if (title == null)
                title = "A problem occurred during " + task;
            await UserError.Throw(new InformationalUserError(e, message, title));
            return true;
        }

        void PerformStatusAction(string actionText, Action act, StatusRepo repo) {
            using (new ActiveMod(this, new StatusMod(actionText) {Repo = repo}))
            using (new RepoWatcher(repo))
                act();
        }

        async Task PerformStatusActionAsync(string actionText, Func<Task> act, StatusRepo repo) {
            using (new ActiveMod(this, new StatusMod(actionText) {Repo = repo}))
            using (new RepoWatcher(repo))
                await act().ConfigureAwait(false);
        }

        class ActiveMod : IDisposable
        {
            readonly IRepoActionHandler _manager;

            public ActiveMod(IRepoActionHandler manager, StatusMod mod) {
                _manager = manager;
                _manager.ActiveStatusMod = mod;
            }

            public void Dispose() {
                Dispose(true);
            }

            void Dispose(bool disposing) {
                if (disposing)
                    _manager.ActiveStatusMod = null;
            }
        }
    }
}