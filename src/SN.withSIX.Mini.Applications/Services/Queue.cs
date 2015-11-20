using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Services
{
    public class QueueInfo
    {
        // TODO: concurrent?
        public List<QueueItem> Items { get; protected set; } = new List<QueueItem>();
    }

    public class QueueItem
    {
        public QueueItem(string title, Func<Action<ProgressState>, CancellationToken, Task> taskFactory) {
            Title = title;
            TaskFactory = taskFactory;
        }

        public void Run(Action stateUpdated) {
            Task = TaskFactory(ps => {
                if (ps.Equals(ProgressState))
                    return;
                ProgressState = ps;
                stateUpdated();
            }, CancelToken.Token);
        }

        public Func<Action<ProgressState>,  CancellationToken, Task> TaskFactory { get; protected set; }

        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Title { get; protected set; }
        public ProgressState ProgressState { get; set; }
        public CompletionState State { get; protected set; }

        public void UpdateState(CompletionState state) {
            State = state;
            Finished = DateTime.UtcNow;
            CancelToken.Dispose();
            CancelToken = null;
            TaskFactory = null;
            Task = null;
        }

        public DateTime Created { get; protected set; } = DateTime.UtcNow;
        public DateTime? Finished { get; protected set; }

        [JsonIgnore]
        public CancellationTokenSource CancelToken { get; protected internal set; }

        [JsonIgnore]
        public Task Task { get; set; }

        public void Cancel() {
            if (CancelToken == null)
                throw new ValidationException("Not cancelable");
            CancelToken.Cancel();
            State = CompletionState.Canceled; // handled by the manager instead?
        }
    }

    public class ProgressState : IEquatable<ProgressState>
    {
        public ProgressState(double progress, long speed, string action) {
            Progress = progress;
            Speed = speed;
            Action = action;
        }

        public double Progress { get; }
        public string Action { get; }
        public long Speed { get; }
        public DateTime LastUpdate { get; protected set; } = DateTime.UtcNow;
        public bool Equals(ProgressState other) {
            return other != null && other.GetHashCode() == GetHashCode();
        }

        public override bool Equals(object other) {
            var o = other as ProgressState;
            return o != null && Equals(o);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Progress.GetHashCode();
                hashCode = (hashCode*397) ^ (Action?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Speed.GetHashCode();
                return hashCode;
            }
        }
    }

    public enum CompletionState
    {
        NotComplete,
        Success,
        Failure,
        Canceled
    }

    public class QueueUpdate
    {
        public Guid Id { get; set; }
        public QueueItem Item { get; set; }
    }

    public class QueueManager : IApplicationService, IQueueManager
    {
        private readonly IQueueHubMessenger _messenger;

        public QueueManager(IQueueHubMessenger messenger) {
            _messenger = messenger;
        }
        
        // TODO: progress handling
        public async Task<Guid> AddToQueue(string title, Func<Action<ProgressState>, CancellationToken, Task> taskFactory) {
            var cts = new CancellationTokenSource();
            var item = new QueueItem(title, taskFactory) { CancelToken = cts};

            item.Run(() => _messenger.Update(item));
            BuildContinuation(item);

            Queue.Items.Add(item);
            await _messenger.AddToQueue(item).ConfigureAwait(false);
            return item.Id;
        }

        private void BuildContinuation(QueueItem item) {
            item.Task = BuildContinuationInternal(item);
        }

        private async Task BuildContinuationInternal(QueueItem item) {
            try {
                await item.Task;
            } catch (OperationCanceledException) {
                item.UpdateState(CompletionState.Canceled);
                await Update(item).ConfigureAwait(false);
            } catch (Exception ex) {
                if (await HandleError(item, ex).ConfigureAwait(false))
                    return;
                item.UpdateState(CompletionState.Failure);
                await Update(item).ConfigureAwait(false);
                throw; // not sure..
            }
            item.UpdateState(CompletionState.Success);
            await Update(item).ConfigureAwait(false);
        }

        private async Task<bool> HandleError(QueueItem item, Exception ex) {
            var result =
                await UserError.Throw(UiTaskHandler.HandleException(ex, "Queue action: " + item.Title));
            switch (result) {
            case RecoveryOptionResult.RetryOperation:
                var cts = new CancellationTokenSource();
                item.CancelToken = cts;
                item.Run(() => _messenger.Update(item));
                BuildContinuation(item);
                return true;
            case RecoveryOptionResult.CancelOperation:
                item.UpdateState(CompletionState.Canceled);
                await Update(item).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        public Task RemoveFromQueue(Guid id) {
            Queue.Items.RemoveAll(x => x.Id == id);
            return _messenger.RemoveFromQueue(id);
        }

        public Task Cancel(Guid id) {
            var item = Queue.Items.First(x => x.Id == id);
            item.Cancel();
            return _messenger.Update(item);
        }


        public Task Update(QueueItem item) {
            return _messenger.Update(item);
        }

        public QueueInfo Queue { get; } = new QueueInfo();
    }
}
