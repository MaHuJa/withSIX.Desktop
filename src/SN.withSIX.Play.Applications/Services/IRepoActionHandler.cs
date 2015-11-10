// <copyright company="SIX Networks GmbH" file="IRepoActionHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using SN.withSIX.Sync.Core.Legacy.Status;

namespace SN.withSIX.Play.Applications.Services
{
    [ContractClass(typeof (RepoHandlerContract))]
    public interface IRepoActionHandler
    {
        StatusMod ActiveStatusMod { get; set; }
        void PerformStatusActionWithBusyHandling(StatusRepo repo, string actionText, Action act);
        void PerformStatusAction(string actionText, Action<StatusRepo> act);
        Task PerformStatusActionWithBusyHandlingAsync(StatusRepo repo, string actionText, Func<Task> act);
        Task PerformStatusActionAsync(string actionText, Func<StatusRepo, Task> act);
        Task PerformUpdaterActionSuspendedAsync(string actionText, Func<Task> act);
        Task TryUpdaterActionAsync(Func<Task> action, string task);
        Task<bool> DealWithUpdateException(Exception e, string task, string message, string title);
    }

    [ContractClassFor(typeof (IRepoActionHandler))]
    public abstract class RepoHandlerContract : IRepoActionHandler
    {
        public void PerformStatusActionWithBusyHandling(StatusRepo repo, string actionText, Action act) {
            Contract.Requires<ArgumentNullException>(repo != null);
            Contract.Requires<ArgumentNullException>(actionText != null);
            Contract.Requires<ArgumentNullException>(act != null);
        }

        public void PerformStatusAction(string actionText, Action<StatusRepo> act) {
            Contract.Requires<ArgumentNullException>(actionText != null);
            Contract.Requires<ArgumentNullException>(act != null);
        }

        public Task PerformStatusActionWithBusyHandlingAsync(StatusRepo repo, string actionText, Func<Task> act) {
            Contract.Requires<ArgumentNullException>(repo != null);
            Contract.Requires<ArgumentNullException>(actionText != null);
            Contract.Requires<ArgumentNullException>(act != null);
            return default(Task);
        }

        public Task PerformStatusActionAsync(string actionText, Func<StatusRepo, Task> act) {
            Contract.Requires<ArgumentNullException>(actionText != null);
            Contract.Requires<ArgumentNullException>(act != null);
            return default(Task);
        }

        public Task PerformUpdaterActionSuspendedAsync(string actionText, Func<Task> act) {
            Contract.Requires<ArgumentNullException>(actionText != null);
            Contract.Requires<ArgumentNullException>(act != null);
            return default(Task);
        }

        public Task TryUpdaterActionAsync(Func<Task> action, string task) {
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentNullException>(task != null);
            return default(Task);
        }

        public Task<bool> DealWithUpdateException(Exception e, string task, string message, string title) {
            Contract.Requires<ArgumentNullException>(e != null);
            return Task.FromResult(default(bool));
        }

        public abstract StatusMod ActiveStatusMod { get; set; }
    }
}