// <copyright company="SIX Networks GmbH" file="ActivationHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using ReactiveUI;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    public static class ActivationHelper
    {
        /// <summary>
        ///     Use this on any aribrary framework element to register to Loaded and Unloaded to activate and deactivate
        ///     observables
        ///     The returned disposable should only be disposed if you want to manually deregister this operation
        /// </summary>
        /// <param name="This"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static IDisposable WhenControlActivated(this FrameworkElement This, Action<Action<IDisposable>> block) {
            if (This is IViewFor)
                throw new NotSupportedException("Use IViewFor WhenActivated instead");
            return This.WhenControlActivated(() => {
                var ret = new List<IDisposable>();
                block(ret.Add);
                return ret;
            });
        }

        /// <summary>
        ///     Use this on any aribrary framework element to register to Loaded and Unloaded to activate and deactivate
        ///     observables
        ///     The returned disposable should only be disposed if you want to manually deregister this operation
        /// </summary>
        /// <param name="This"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static IDisposable WhenControlActivated(this FrameworkElement This, Func<IEnumerable<IDisposable>> block) {
            if (This is IViewFor)
                throw new NotSupportedException("Use IViewFor WhenActivated instead");

            var activationEvents = This.GetActivationForView();

            return handleViewActivation(block, activationEvents);
        }

        public static IDisposable handleViewActivation(Func<IEnumerable<IDisposable>> block,
            Tuple<IObservable<Unit>, IObservable<Unit>> activation) {
            var viewDisposable = new SerialDisposable();

            return new CompositeDisposable(
                // Activation
                activation.Item1.Subscribe(_ => {
                    // NB: We need to make sure to respect ordering so that the cleanup
                    // happens before we invoke block again
                    viewDisposable.Disposable = Disposable.Empty;
                    viewDisposable.Disposable = new CompositeDisposable(block());
                }),
                // Deactivation
                activation.Item2.Subscribe(_ => { viewDisposable.Disposable = Disposable.Empty; }),
                viewDisposable);
        }

        public static Tuple<IObservable<Unit>, IObservable<Unit>> GetActivationForView(this FrameworkElement fe) {
            if (fe == null)
                return Tuple.Create(Observable.Empty<Unit>(), Observable.Empty<Unit>());

            var viewLoaded = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(x => fe.Loaded += x,
                x => fe.Loaded -= x).Select(_ => Unit.Default);
            var viewHitTestVisible = fe.WhenAnyValue(v => v.IsHitTestVisible);

            var viewActivated = viewLoaded.CombineLatest(viewHitTestVisible, (l, h) => h)
                .Where(v => v)
                .Select(_ => Unit.Default);

            var viewUnloaded = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(x => fe.Unloaded += x,
                x => fe.Unloaded -= x).Select(_ => Unit.Default);

            return Tuple.Create(viewActivated, viewUnloaded);
        }
    }
}