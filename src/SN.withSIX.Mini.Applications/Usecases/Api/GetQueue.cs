using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetQueue : IAsyncQuery<QueueInfo> {}

    public class GetQueueHandler : DbRequestBase, IAsyncRequestHandler<GetQueue, QueueInfo>
    {
        private readonly IQueueManager _queueManager;
        public GetQueueHandler(IDbContextLocator dbContextLocator, IQueueManager queueManager) : base(dbContextLocator) {
            _queueManager = queueManager;
        }

        public Task<QueueInfo> HandleAsync(GetQueue request) {
            return Task.FromResult(_queueManager.Queue);
        }
    }
}