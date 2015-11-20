using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class RetryQueueItem : IAsyncVoidCommand
    {
        public RetryQueueItem(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class RetryQueueItemHandler : DbRequestBase, IAsyncVoidCommandHandler<RetryQueueItem>
    {
        private readonly IQueueManager _queueManager;

        public RetryQueueItemHandler(IDbContextLocator dbContextLocator, IQueueManager queueManager)
            : base(dbContextLocator) {
            _queueManager = queueManager;
        }

        public Task<UnitType> HandleAsync(RetryQueueItem request) {
            return _queueManager.Retry(request.Id).Void();
        }
    }
}