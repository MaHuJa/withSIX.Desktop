// <copyright company="SIX Networks GmbH" file="HubBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.NotificationHandlers;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.Usecases.Main;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Infra.Api.Hubs
{
    // TODO: AOP! and only apply the actionstarted based on if it's supposed to be a user action... not just getapi info
    // ahh!! is that the issue with the action received; the getInfo call?! that only happens after a page refresh? hmm
    public abstract class HubBase<T> : Hub<T> where T : class
    {
        protected static Task<TResponseData> RequestAsync<TResponseData>(ICompositeCommand<TResponseData> message) {
            return ApiAction(message.Execute, message);
        }

        protected static Task<TResponse> RequestAsync<TResponse>(IAsyncRequest<TResponse> command) {
            return ApiAction(command.Execute, command);
        }

        protected static Task<T> OpenScreenAsync<T>(IAsyncQuery<T> query) where T : class, IScreenViewModel {
            return query.OpenScreen();
        }

        protected static Task<T> OpenScreenCached<T>(IAsyncQuery<T> query) where T : class, IScreenViewModel {
            return query.OpenScreenCached();
        }

        static async Task<TResponse> ApiAction<TResponse>(Func<Task<TResponse>> action, object command) {
            var isUserAction = command.GetType().GetAttribute<ApiUserActionAttribute>() != null;
            retry:
            try {
                if (isUserAction)
                    await new ApiUserActionStarted().RaiseEvent().ConfigureAwait(false);
                var r = await action().ConfigureAwait(false);
                if (isUserAction)
                    await new ApiUserActionFinished().RaiseEvent().ConfigureAwait(false);
                return r;
            } catch (NotLoggedinException ex) {
                // TODO: The hub actions should decide on this :(
                await OpenScreenCached(new GetLogin()).ConfigureAwait(false);
                return default(TResponse); // Pff...
            } catch (Exception ex) {
                // TODO: Improve handling
                //await new ApiException(ex).RaiseEvent().ConfigureAwait(false);
                var result =
                    await UserError.Throw(UiTaskHandler.HandleException(ex, "API action: " + command.GetType().Name));
                if (result == RecoveryOptionResult.RetryOperation)
                    goto retry;
                // TODO: Or should we use some else?
                throw;
            }
        }
    }
}