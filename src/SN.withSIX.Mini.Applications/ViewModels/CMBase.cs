// <copyright company="SIX Networks GmbH" file="CMBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public abstract class CMBase : ContextMenuBase //, ISupportsActivation
    {
        /*
        protected CMBase() {
            Activator = new ViewModelActivator();
            // TODO: Activation doesnt work atm? :S
            this.WhenAnyValue(x => x.IsOpen)
                .Subscribe(x => {
                    if (x)
                        using (Activator.Activate())
                            ;
                    else
                        Activator.Deactivate();
                });
        }
        */

        public ViewModelActivator Activator { get; }

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
}