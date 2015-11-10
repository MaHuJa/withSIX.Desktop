// <copyright company="SIX Networks GmbH" file="WpfAppBootstrapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Amazon.AutoScaling.Model;
using Caliburn.Micro;
using ReactiveUI;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Extensions;
using SN.withSIX.Core.Presentation.Services;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Core.Presentation.Wpf.Services;
using Splat;
using ViewLocator = Caliburn.Micro.ViewLocator;

namespace SN.withSIX.Core.Presentation.Wpf
{
    public class UiRoot : IPresentationService
    {
        readonly IDialogManager _dialogManager;

        public UiRoot(IDialogManager dialogManager) {
            _dialogManager = dialogManager;
            ErrorHandler = new WpfErrorHandler(dialogManager);
        }

        public WpfErrorHandler ErrorHandler { get; }

        public static UiRoot Main { get; set; }

        public async Task<RecoveryOptionResult> ErrorDialog(UserError error, Window window = null) {
            MainLog.Logger.FormattedWarnException(error.InnerException, "UserError");
            if (Common.Flags.IgnoreErrorDialogs)
                return RecoveryOptionResult.FailOperation;
            var settings = new Dictionary<string, object>();
            if (window != null)
                settings["Owner"] = window;
            var t = _dialogManager.ShowDialogAsync(new UserErrorViewModel(error), settings);
            var t2 = error.RecoveryOptions.Cast<RecoveryCommand>()
                .Select(x => x.Select(_ => x.RecoveryResult))
                .Merge()
                .Select(x => x.GetValueOrDefault(RecoveryOptionResult.FailOperation))
                .FirstAsync()
                .ToTask();
            await Task.WhenAll(t, t2).ConfigureAwait(false);
            return await t2.ConfigureAwait(false);
        }
    }

    public abstract class WpfAppBootstrapper<T> : AppBootstrapperBase
    {
        readonly DependencyObjectObservableForProperty workaroundForSmartAssemblyReactiveXAML;
        IExceptionHandler _exceptionHandler;
        protected bool DisplayRootView = true;
        static Type idontic = typeof (IDontIC);
        static Type[] _meepfaces = new[] {typeof (IContextMenu), typeof (IDialog)};
        static Type _transient = typeof(ITransient);
        static Type _singleton = typeof(ISingleton);

        protected override void Configure() {
            base.Configure();
            SetupViewNamespaces();

            var mutableDependencyResolver = Locator.CurrentMutable;
            SetupRx(mutableDependencyResolver);
        }

        protected virtual void SetupRx(IMutableDependencyResolver dependencyResolver) {
            var viewLocator = new DefaultViewLocator();
            dependencyResolver.Register(() => viewLocator, typeof (IViewLocator));
        }

        protected virtual void SetupViewNamespaces() {
            var original = ViewLocator.LocateForModel;
            ViewLocator.LocateForModel = (o, dependencyObject, arg3) => {
                var v = original(o, dependencyObject, arg3);
                // TODO: Lacks CM's Context/target support
                if (v == null || v is TextBlock) {
                    var rxv = (UIElement) ReactiveUI.ViewLocator.Current.ResolveView(o);
                    if (rxv != null)
                        v = rxv;
                }

                var vFor = v as IViewFor;
                if (vFor != null)
                    vFor.ViewModel = o;

                return v;
            };
            //ViewLocator.AddNamespaceMapping("SN.withSIX.Core.Presentation.Wpf.Views", "SN.withSIX.Core.Presentation.Wpf.Views");
            ViewLocator.AddNamespaceMapping("SN.withSIX.Core.Applications.MVVM.ViewModels.Popups",
                "SN.withSIX.Core.Presentation.Wpf.Views.Popups");
            ViewLocator.AddNamespaceMapping("SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs",
                "SN.withSIX.Core.Presentation.Wpf.Views.Dialogs");
            ViewLocator.AddNamespaceMapping("SN.withSIX.Core.Applications.MVVM.ViewModels",
                "SN.withSIX.Core.Presentation.Wpf.Views");
        }

        protected virtual void PreStart() {}

        protected override sealed void OnStartup(object sender, StartupEventArgs e) {
            using (this.Bench()) {
                try {
                    SetupContainer();
                    SetupExceptionHandling();
                    using (MainLog.Bench(null, "WpfAppBootstrapper.PreStart"))
                        PreStart();
                    using (MainLog.Bench(null, "WpfAppBootstrapper.OnStartup"))
                        base.OnStartup(sender, e);
                    if (DisplayRootView)
                        DisplayRootViewFor<T>();
                } catch (Exception ex) {
                    LogError(ex, "Startup");
                    throw;
                }
            }
        }

        void SetupExceptionHandling() {
            _exceptionHandler = Container.GetInstance<IExceptionHandler>();
            UiTaskHandler.SetExceptionHandler(_exceptionHandler);
            OverrideCaliburnMicroActionHandling();
        }

        void OverrideCaliburnMicroActionHandling() {
            var originalInvokeAction = ActionMessage.InvokeAction;
            ActionMessage.InvokeAction =
                actionExecutionContext => ExecuteAction(originalInvokeAction, actionExecutionContext);
        }

        bool ExecuteAction(Action<ActionExecutionContext> originalInvokeAction,
            ActionExecutionContext actionExecutionContext) {
            using (
                MainLog.Bench("UIAction",
                    actionExecutionContext.Target.GetType().Name + "." + actionExecutionContext.Method.Name)) {
                // TODO: This can deadlock ...
                // TODO: Don't use caliburn micro action handling, but use RXUI's task based Commands
                return
                    _exceptionHandler.TryExecuteAction(() => AsyncWrap(originalInvokeAction, actionExecutionContext))
                        .Result;
            }
        }

        static async Task AsyncWrap(Action<ActionExecutionContext> originalInvokeAction,
            ActionExecutionContext actionExecutionContext) {
            originalInvokeAction(actionExecutionContext);
        }

        protected override sealed void OnExit(object sender, EventArgs e) {
            try {
                base.OnExit(sender, e);
                ExitAsync(sender, e).WaitSpecial();
                Exit(sender, e);
            } catch (Exception ex) {
                LogError(ex, "Exit");
                throw;
            }
        }

        protected virtual void Exit(object sender, EventArgs eventArgs) {}
        protected virtual async Task ExitAsync(object sender, EventArgs eventArgs) {}

        static void LogError(Exception ex, string type) {
            try {
                MainLog.Logger.FormattedErrorException(ex, "Error during " + type);
            } catch {}
        }

        protected override void ConfigureContainer() {
            base.ConfigureContainer();
            Container.RegisterSingleton<IExitHandler, ExitHandler>();
            Container.RegisterSingleton<IShutdownHandler, WpfShutdownHandler>();
            Container.RegisterSingleton<IFirstTimeLicense, WpfFirstTimeLicense>();
            Container.RegisterSingleton<IDialogManager, WpfDialogManager>();
            Container.RegisterSingleton<IWindowManager, CustomWindowManager>();

            ExportViews();
            ExportViewModels();
        }

        void ExportViews() {
            Container.RegisterAllInterfacesAndType<UserControl>(AssemblySource.Instance, x => x.Name.EndsWith("View"));
            Container.RegisterAllInterfacesAndType<Window>(AssemblySource.Instance, x => x.Name.EndsWith("View"));
        }

        void ExportViewModels() {
            //Container.RegisterSingleAllInterfacesAndType<IShellViewModel>(AssemblySource.Instance,
                //x => !x.GetInterfaces().Contains(value));

            // nutters
            Container.RegisterSingleAllInterfacesAndType<ViewModelBase>(AssemblySource.Instance, Filter2);
            Container.RegisterSingleAllInterfacesAndType<ReactiveScreen>(AssemblySource.Instance, Filter2);
            Container.RegisterAllInterfacesAndType<ViewModelBase>(AssemblySource.Instance, Filter2Transient);
            Container.RegisterAllInterfacesAndType<ReactiveScreen>(AssemblySource.Instance, Filter2Transient);


            // Used by factories, should not be singleton!!
            Container.RegisterAllInterfacesAndType<IContextMenu>(AssemblySource.Instance, Filter);
            Container.RegisterAllInterfacesAndType<IDialog>(AssemblySource.Instance, Filter);
        }

        bool Filter(Type arg) {
            return FilterInternal(arg, arg.GetInterfaces());
        }

        bool Filter2Transient(Type arg) {
            return Filter2TransientInternal(arg, arg.GetInterfaces());
        }

        bool Filter2(Type arg) {
            return Filter2Internal(arg, arg.GetInterfaces());
        }

        // TODO: compose methods by passing the interfaces instead of getting over and over?
        static bool FilterInternal(Type type, Type[] interfaces) {
            return !interfaces.Contains(idontic);
        }

        static bool Filter2Internal(Type type, Type[] interfaces) {
            return FilterInternal(type, interfaces) && MeepFilter(type, interfaces) && IsSingleTonOverridenOrNotTransient(type, interfaces);
        }

        static bool IsSingleTonOverridenOrNotTransient(Type type, Type[] interfaces) {
            return interfaces.Contains(_singleton) || !interfaces.Contains(_transient);
        }

        static bool Filter2TransientInternal(Type type, Type[] interfaces) {
            return FilterInternal(type, interfaces) && MeepFilter(type, interfaces) && IsTransientAndNotSingletonOverriden(type, interfaces);
        }

        static bool IsTransientAndNotSingletonOverriden(Type type, Type[] interfaces) {
            return interfaces.Contains(_transient) && !interfaces.Contains(_singleton);
        }

        static bool MeepFilter(Type type, Type[] interfaces) {
            return !_meepfaces.Any(interfaces.Contains);
        }
    }
}