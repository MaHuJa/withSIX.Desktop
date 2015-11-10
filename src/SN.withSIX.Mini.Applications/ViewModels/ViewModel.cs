// <copyright company="SIX Networks GmbH" file="ViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public abstract class ViewModel : ReactiveObject, IViewModel, ISupportsActivation
    {
        protected ViewModel() {
            Activator = new ViewModelActivator();
        }

        public ViewModelActivator Activator { get; }

        protected static Task<TResponseData> RequestAsync<TResponseData>(ICompositeCommand<TResponseData> message) {
            return message.Execute();
        }

        protected static Task<TResponseData> RequestAsync<TResponseData>(IAsyncRequest<TResponseData> message) {
            return message.Execute();
        }

        protected static IObservable<T> Listen<T>() {
            return Cheat.MessageBus.Listen<T>();
        }

        // TODO: evaluate how this can be a memory leak problem...
        // Also important: Does the MessageBus only keep the latest entry if asked for, or always
        protected static IObservable<T> ListenIncludeLatest<T>() {
            return Cheat.MessageBus.ListenIncludeLatest<T>();
        }

        protected static Task<T> OpenScreenAsync<T>(IAsyncQuery<T> query) where T : class, IScreenViewModel {
            return query.OpenScreen();
        }

        protected static Task<T> OpenScreenCached<T>(IAsyncQuery<T> query) where T : class, IScreenViewModel {
            return query.OpenScreenCached();
        }
    }

    public interface IViewModel : IReactiveObject {}
}