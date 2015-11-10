// <copyright company="SIX Networks GmbH" file="CommandExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI.Legacy;

namespace SN.withSIX.Core.Applications.Extensions
{
    public static class CommandExtensions
    {
        public static IObservable<TOutput> RegisterAsyncTask<TInput, TOutput>(this ReactiveCommand command,
            Func<TInput, Task<TOutput>> func) {
            return command.RegisterAsyncTask(x => func((TInput) x));
        }

        public static IObservable<Unit> RegisterAsyncTaskVoid<TInput>(this ReactiveCommand command,
            Func<TInput, Task> func) {
            return command.RegisterAsyncTask(x => func((TInput) x));
        }

        public static IObservable<Unit> RegisterAsyncTask(this ReactiveCommand command, Func<Task> func) {
            return command.RegisterAsyncTask(x => func());
        }

        public static IDisposable Subscribe<TOutput>(this IObservable<TOutput> observable, Action func) {
            return observable.Subscribe(x => func());
        }
    }
}