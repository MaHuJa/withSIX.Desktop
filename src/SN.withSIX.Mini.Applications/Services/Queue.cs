using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Services
{
    public class QueueInfo
    {
        // TODO: concurrent?
        public List<QueueItem> Items { get; set; }
    }

    public class QueueItem
    {
        public QueueItem(string title, Task task) {
            Title = title;
            Task = task;
        }

        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Title { get; protected set; }
        public ProgressState ProgressState { get; set; }
        public CompletionState State { get; set; }

        [JsonIgnore]
        public CancellationTokenSource CancelToken { get; set; }

        [JsonIgnore]
        public Task Task { get; set; }
    }

    public class ProgressState
    {
        public double Progress { get; set; }
        public string Action { get; set; }
        public double Speed { get; set; }
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

        public Task AddToQueue(string title, Func<Task> taskFactory) {
            var task = taskFactory();
            var item = new QueueItem(title, task);

            BuildContinuation(taskFactory, item, task);

            Queue.Items.Add(item);
            return _messenger.AddToQueue(item);
        }

        private void BuildContinuation(Func<Task> taskFactory, QueueItem item, Task task) {
            item.Task = task.ContinueWith(x => ProcessError(taskFactory, item, x), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(x => {
                    item.State = CompletionState.Failure;
                    return Update(item);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task ProcessError(Func<Task> taskFactory, QueueItem item, Task x) {
            var result =
                await UserError.Throw(UiTaskHandler.HandleException(x.Exception, "Queue action: " + item.Title));
            if (result == RecoveryOptionResult.RetryOperation) {
                BuildContinuation(taskFactory, item, taskFactory());
                return;
            }
            // TODO: Or should we use some else?
            throw x.Exception;
        }

        public Task RemoveFromQueue(Guid id) {
            Queue.Items.RemoveAll(x => x.Id == id);
            return _messenger.RemoveFromQueue(id);
        }

        public Task Update(QueueItem item) {
            return _messenger.Update(item);
        }

        public QueueInfo Queue { get; } = new QueueInfo();
    }
}
