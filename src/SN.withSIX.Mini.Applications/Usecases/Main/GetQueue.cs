// <copyright company="SIX Networks GmbH" file="GetQueue.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels.Main.Queue;

namespace SN.withSIX.Mini.Applications.Usecases.Main
{
    public class GetQueue : IAsyncQuery<IQueueViewModel> {}

    public class GetQueueHandler : IAsyncRequestHandler<GetQueue, IQueueViewModel>
    {
        public Task<IQueueViewModel> HandleAsync(GetQueue request) {
            return Task.FromResult<IQueueViewModel>(new QueueViewModel());
        }
    }
}