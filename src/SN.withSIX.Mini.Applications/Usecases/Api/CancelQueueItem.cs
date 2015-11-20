using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class CancelQueueItem : IAsyncVoidCommand
    {
        public CancelQueueItem(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }


    public class CancelQueueItemHandler : DbRequestBase, IAsyncVoidCommandHandler<CancelQueueItem>
    {
        private readonly IQueueManager _queueManager;
        public CancelQueueItemHandler(IDbContextLocator dbContextLocator, IQueueManager queueManager) : base(dbContextLocator)
        {
            _queueManager = queueManager;
        }

        public Task<UnitType> HandleAsync(CancelQueueItem request)
        {
            return _queueManager.Cancel(request.Id).Void();
        }
    }
}