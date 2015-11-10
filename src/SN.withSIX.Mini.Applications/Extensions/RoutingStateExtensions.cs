// <copyright company="SIX Networks GmbH" file="RoutingStateExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace SN.withSIX.Mini.Applications.Extensions
{
    public static class RoutingStateExtensions
    {
        public static async Task NavigateIfNotAlready<T>(this RoutingState state, Func<T> objCreator) {
            var currentViewModel = state.GetCurrentViewModel();
            if (currentViewModel.GetType() == typeof (T))
                return;
            await state.Navigate.ExecuteAsyncTask(objCreator()).ConfigureAwait(false);
        }

        public static async Task NavigateIfNotAlready<T>(this RoutingState state, Func<Task<T>> objCreator) {
            var currentViewModel = state.GetCurrentViewModel();
            if (currentViewModel.GetType() == typeof (T))
                return;
            await state.Navigate.ExecuteAsyncTask(await objCreator().ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}