// <copyright company="SIX Networks GmbH" file="ViewModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DoNotObfuscate]
    public abstract class ViewModelBase : ReactiveValidatableObjectBase, IViewModel {}

    public abstract class RxViewModelBase : ViewModelBase, IDialog, IRxClose, ISupportsActivation
    {
        protected RxViewModelBase() {
            Activator = new ViewModelActivator();
        }

        public ViewModelActivator Activator { get; }

        // Meh
        ReactiveCommand<bool?> IRxClose.Close { get; set; } =
            ReactiveCommand.CreateAsyncTask(x => Task.FromResult((bool?) null));

        protected void TryClose(bool? b = null) {
            ((IRxClose) this).Close.Execute(b);
        }
    }

    public abstract class PopupBase : ScreenBase, IDialog, IIsOpen
    {
        bool _isOpen;
        bool _staysOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { SetProperty(ref _isOpen, value); }
        }
        public bool StaysOpen
        {
            get { return _staysOpen; }
            set { SetProperty(ref _staysOpen, value); }
        }
    }

    public abstract class MetroPopupBase : RxViewModelBase
    {
        protected MetroPopupBase() {
            ((IRxClose) this).Close = ReactiveCommand.CreateAsyncTask(ConvertToBool);
        }

        // nasty...
        async Task<bool?> ConvertToBool(object arg) {
            return (bool?) arg;
        }
    }

    public abstract class DialogBase : ScreenBase, IDialog //, IMetroDialog
    {}

    public abstract class MetroDialogBase : RxViewModelBase, IMetroDialog
    {
        string _displayName;
        public virtual string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }
    }

    public abstract class ViewModelBase<T> : ViewModelBase, IHaveModel<T> where T : class
    {
        protected ViewModelBase(T model) {
            Contract.Requires<ArgumentNullException>(model != null);

            Model = model;
        }

        public T Model { get; }
    }
}