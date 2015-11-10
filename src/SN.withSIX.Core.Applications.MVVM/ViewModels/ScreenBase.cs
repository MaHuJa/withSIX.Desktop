// <copyright company="SIX Networks GmbH" file="ScreenBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using Caliburn.Micro;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    // Using both ReactiveUI and Caliburn.Micro

    [DoNotObfuscate]
    public abstract class ScreenBase : ReactiveScreen, IViewModel {}

    [DoNotObfuscate]
    public abstract class ScreenBase<T> : ScreenBase where T : class
    {
        protected T ParentShell
        {
            get { return (Parent as T); }
        }
    }

    public abstract class ReactiveModalScreen<T> : ScreenBase<T>, IModalScreen where T : class, IModalShell
    {
        protected ReactiveModalScreen() {
            ShowBackButton = true;
        }

        [DoNotObfuscate]
        public virtual void Cancel() {
            TryClose(false);
        }

        public bool ShowBackButton { get; protected set; }

        [DoNotObfuscate]
        public override void TryClose(bool? dialogResult) {
            if (ParentShell == null)
                return;
            ((IDeactivate) this).Deactivate(true);
            ParentShell.HideModalView();
        }
    }

    public class ReactiveModalScreen<T, T2> : ReactiveModalScreen<T>, IHaveModel<T2>
        where T : class, IModalShell
        where T2 : class
    {
        protected static readonly string ModelProperty = "Model";

        public ReactiveModalScreen(T2 model) {
            Contract.Requires<ArgumentNullException>(model != null);
            Model = model;
        }

        public T2 Model { get; }
    }
}