// <copyright company="SIX Networks GmbH" file="FlyoutViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public interface IIsOpen
    {
        bool IsOpen { get; set; }
    }

    public interface IFlyoutViewModel : ISomeViewModel, IIsOpen
    {
        bool IsModal { get; }
        void Toggle();
    }

    public abstract class FlyoutViewModel : SomeViewModel, IFlyoutViewModel
    {
        bool _isOpen;

        public void Toggle() {
            IsOpen = !IsOpen;
        }

        public virtual bool IsModal => false;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { this.RaiseAndSetIfChanged(ref _isOpen, value); }
        }
    }

    public abstract class ModalFlyoutViewModel : FlyoutViewModel
    {
        public override bool IsModal => true;
    }
}