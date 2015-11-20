using System;
using System.Threading;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Applications.Services
{
    public interface IQueueManager {
        Task<Guid> AddToQueue(string title, Func<Action<ProgressState>, CancellationToken, Task> task);
        Task RemoveFromQueue(Guid id);
        Task Update(QueueItem item);
        QueueInfo Queue { get; }
        Task Cancel(Guid id);
        Task Retry(Guid id);
    }
}