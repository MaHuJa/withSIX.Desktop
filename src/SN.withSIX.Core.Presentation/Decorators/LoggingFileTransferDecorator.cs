// <copyright company="SIX Networks GmbH" file="LoggingFileTransferDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Core.Presentation.Decorators
{
    public abstract class LoggingFileTransferDecorator : IEnableLogging
    {
        protected abstract void OnFinished(TransferSpec spec);
        protected abstract void OnError(TransferSpec spec, Exception exception);
        protected abstract void OnStart(TransferSpec spec);

        protected async Task Wrap(Func<Task> task, TransferSpec spec) {
            OnStart(spec);
            try {
                await task().ConfigureAwait(false);
            } catch (Exception e) {
                OnError(spec, e);
                throw;
            }
            OnFinished(spec);
        }

        protected void Wrap(Action action, TransferSpec spec) {
            OnStart(spec);
            try {
                action();
            } catch (Exception e) {
                OnError(spec, e);
                throw;
            }
            OnFinished(spec);
        }
    }
}