// <copyright company="SIX Networks GmbH" file="IScreenOpener.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Applications.Services
{
    public interface IScreenOpener
    {
        void Open<T>(T viewModel) where T : class, IScreenViewModel;
        Task OpenAsync<T>(T viewModel) where T : class, IScreenViewModel;
    }

    public static class ScreenOpenerExtensions
    {
        static readonly IDictionary<Type, IScreenViewModel> cached = new Dictionary<Type, IScreenViewModel>();

        public static async Task<T> OpenAsyncQuery<T>(this IScreenOpener opener, IAsyncQuery<T> query)
            where T : class, IScreenViewModel {
            var viewModel = await query.Execute().ConfigureAwait(false);
            await opener.OpenAsync(viewModel).ConfigureAwait(false);
            return viewModel;
        }

        public static async Task<T> OpenAsyncQueryCached<T>(this IScreenOpener opener, IAsyncQuery<T> query)
            where T : class, IScreenViewModel {
            var t = typeof (T);
            if (cached.ContainsKey(t)) {
                var vm = cached[t];
                if (vm.IsOpen) {
                    await vm.Activate.ExecuteAsyncTask().ConfigureAwait(false);
                    return (T) vm;
                }
            }
            var viewModel = await query.Execute().ConfigureAwait(false);
            cached.Add(t, viewModel);
            await opener.OpenAsync(viewModel).ConfigureAwait(false);
            HandleClosing(viewModel, t);
            return viewModel;
        }

        static void HandleClosing<T>(T viewModel, Type t) where T : class, IScreenViewModel {
            IDisposable sub = null;
            sub = viewModel.WhenAnyValue(x => x.IsOpen)
                .Where(x => !x)
                .Subscribe(x => {
                    cached.Remove(t);
                    sub.Dispose();
                });
        }
    }
}