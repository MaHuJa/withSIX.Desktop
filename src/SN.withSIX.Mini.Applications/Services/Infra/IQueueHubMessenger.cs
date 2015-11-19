using System;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Applications.Services.Infra
{
    public interface IQueueHubMessenger {
        Task AddToQueue(QueueItem item);
        Task RemoveFromQueue(Guid id);
        Task Update(QueueItem item);
    }
}