// <copyright company="SIX Networks GmbH" file="RxExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace SN.withSIX.Core.Applications.MVVM.Extensions
{
    public static class RxExtensions
    {
        public static IObservable<TSource> ObserveOnMainThread<TSource>(this IObservable<TSource> sourcer) {
            return sourcer.ObserveOn(RxApp.MainThreadScheduler);
        }
    }
}