// <copyright company="SIX Networks GmbH" file="DefaultViewLocator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using ReactiveUI;
using Splat;
using Action = Caliburn.Micro.Action;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class DefaultViewLocator : IViewLocator, IEnableLogger
    {
        public DefaultViewLocator(Func<string, string> viewModelToViewFunc = null) {
            ViewModelToViewFunc = viewModelToViewFunc ??
                                  (vm => interfaceifyTypeName(vm.Replace("ViewModel", "View").Replace("Designtime", "")));
        }

        public Func<string, string> ViewModelToViewFunc { get; set; }

        /// <summary>
        ///     Returns the View associated with a ViewModel, deriving the name of
        ///     the Type via ViewModelToViewFunc, then discovering it via
        ///     ServiceLocator.
        /// </summary>
        /// <param name="viewModel">
        ///     The ViewModel for which to find the
        ///     associated View.
        /// </param>
        /// <returns>The View for the ViewModel.</returns>
        public IViewFor ResolveView<T>(T viewModel, string contract = null)
            where T : class {
            // Given IFooBarViewModel (whose name we derive from T), we'll look 
            // for a few things:
            // * IFooBarView that implements IViewFor
            // * IViewFor<IFooBarViewModel>
            // * IViewFor<FooBarViewModel> (the original behavior in RxUI 3.1)

            var attrs = viewModel.GetType().GetTypeInfo().GetCustomAttributes(typeof (ViewContractAttribute), true);

            if (attrs.Any())
                contract = contract ?? ((ViewContractAttribute) attrs.First()).Contract;

            // IFooBarView that implements IViewFor (or custom ViewModelToViewFunc)
            var typeToFind = ViewModelToViewFunc(viewModel.GetType().AssemblyQualifiedName);

            var ret = attemptToResolveView(Reflection.ReallyFindType(typeToFind, false), contract);
            if (ret != null) {
                SetupCaliburnMicro(viewModel, ret);
                return ret;
            }

            // IViewFor<FooBarViewModel> (the original behavior in RxUI 3.1)
            var viewType = typeof (IViewFor<>);
            try {
                ret = attemptToResolveView(viewType.MakeGenericType(viewModel.GetType()), contract);
            } catch (Exception) {
                throw;
            }
            // Workaround so we don't need to IViewFor each specific UserError
            if (ret == null && viewModel is UserError)
                ret = attemptToResolveView(viewType.MakeGenericType(typeof (UserError)), contract);

            if (ret != null)
                SetupCaliburnMicro(viewModel, ret);
            return ret;
        }

        IViewFor attemptToResolveView(Type type, string contract) {
            if (type == null)
                return null;

            var ret = default(IViewFor);

            try {
                ret = (IViewFor) Locator.Current.GetService(type, contract);
            } catch (Exception ex) {
                this.Log().ErrorException("Failed to instantiate view: " + type.FullName, ex);
                throw;
            }

            return ret;
        }

        [Obsolete("Switch to bindings instead")]
        static void SetupCaliburnMicro<T>(T viewModel, IViewFor resolvedView) {
            var dependencyObject = ((DependencyObject) resolvedView);
            dependencyObject.SetValue(View.IsGeneratedProperty, true);
            ViewModelBinder.Bind(viewModel, dependencyObject, null);
            Action.SetTargetWithoutContext(dependencyObject, viewModel);
        }

        static string interfaceifyTypeName(string typeName) {
            var idxComma = typeName.IndexOf(',');
            var idxPeriod = typeName.LastIndexOf('.', idxComma - 1);
            return typeName.Insert(idxPeriod + 1, "I");
        }
    }
}