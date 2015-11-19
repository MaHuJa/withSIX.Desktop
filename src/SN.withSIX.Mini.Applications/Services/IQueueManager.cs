using System;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Applications.Services
{
    public interface IQueueManager {
        Task AddToQueue(string title, Func<Task> task);
        Task RemoveFromQueue(Guid id);
        Task Update(QueueItem item);
        QueueInfo Queue { get; }
    }
}