// <copyright company="SIX Networks GmbH" file="ViewModelViewHostFixed.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using ReactiveUI;
using Splat;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     This content control will automatically load the View associated with
    ///     the ViewModel property and display it. This control is very useful
    ///     inside a DataTemplate to display the View associated with a ViewModel.
    ///     This variant works around a bug in the original when updating ViewContractObservable
    /// </summary>
    public class ViewModelViewHostFixed : TransitioningContentControl, IViewFor, IEnableLogger
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (object), typeof (ViewModelViewHostFixed),
                new PropertyMetadata(null, somethingChanged));
        public static readonly DependencyProperty DefaultContentProperty =
            DependencyProperty.Register("DefaultContent", typeof (object), typeof (ViewModelViewHostFixed),
                new PropertyMetadata(null, somethingChanged));
        public static readonly DependencyProperty ViewContractObservableProperty =
            DependencyProperty.Register("ViewContractObservable", typeof (IObservable<string>),
                typeof (ViewModelViewHostFixed), new PropertyMetadata(Observable.Return(default(string))));
        readonly Subject<Unit> updateViewModel = new Subject<Unit>();

        public ViewModelViewHostFixed() {
#if WINRT
            this.DefaultStyleKey = typeof(ViewModelViewHost);
#endif

            // NB: InUnitTestRunner also returns true in Design Mode
            if (ModeDetector.InUnitTestRunner()) {
                ViewContractObservable = Observable.Never<string>();
                return;
            }

            var vmAndContract =
                this.WhenAnyValue(x => x.ViewModel).CombineLatest(this.WhenAnyObservable(x => x.ViewContractObservable),
                    (vm, contract) => new {ViewModel = vm, Contract = contract});

            this.WhenActivated(d => {
                d(vmAndContract.Subscribe(x => {
                    if (x.ViewModel == null) {
                        Content = DefaultContent;
                        return;
                    }

                    var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                    var view = viewLocator.ResolveView(x.ViewModel, x.Contract) ??
                               viewLocator.ResolveView(x.ViewModel, null);

                    if (view == null)
                        throw new Exception(String.Format("Couldn't find view for '{0}'.", x.ViewModel));

                    view.ViewModel = x.ViewModel;
                    Content = view;
                }));
            });
        }

        /// <summary>
        ///     If no ViewModel is displayed, this content (i.e. a control) will be displayed.
        /// </summary>
        public object DefaultContent
        {
            get { return GetValue(DefaultContentProperty); }
            set { SetValue(DefaultContentProperty, value); }
        }
        public IObservable<string> ViewContractObservable
        {
            get { return (IObservable<string>) GetValue(ViewContractObservableProperty); }
            set { SetValue(ViewContractObservableProperty, value); }
        }
        public IViewLocator ViewLocator { get; set; }
        /// <summary>
        ///     The ViewModel to display
        /// </summary>
        public object ViewModel
        {
            get { return GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        static void somethingChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((ViewModelViewHostFixed) dependencyObject).updateViewModel.OnNext(Unit.Default);
        }
    }
}