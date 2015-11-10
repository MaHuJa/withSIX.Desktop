// <copyright company="SIX Networks GmbH" file="OverlayViewModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Overlays
{
    public abstract class ScreenLightViewModelBase<T> : ScreenBase<T> where T : class
    {
        [Browsable(false)]
        public new string DisplayName
        {
            get { return base.DisplayName; }
            set { base.DisplayName = value; }
        }
        [Browsable(false)]
        public new bool IsActive
        {
            get { return base.IsActive; }
        }
        [Browsable(false)]
        public new bool IsInitialized
        {
            get { return base.IsInitialized; }
        }
        [Browsable(false)]
        public new object Parent
        {
            get { return base.Parent; }
        }
        [Browsable(false)]
        public new T ParentShell
        {
            get { return base.ParentShell; }
        }
        [Browsable(false)]
        public new bool IsNotifying
        {
            get { return base.IsNotifying; }
            set { base.IsNotifying = value; }
        }
        [Browsable(false)]
        public new IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changing
        {
            get { return base.Changing; }
        }
        [Browsable(false)]
        public new IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changed
        {
            get { return base.Changed; }
        }
        [Browsable(false)]
        public new IObservable<Exception> ThrownExceptions
        {
            get { return base.ThrownExceptions; }
        }
        [Browsable(false)]
        public new bool IsValid
        {
            get { return base.IsValid; }
        }
    }

    public abstract class OverlayViewModelBase : ScreenLightViewModelBase<OverlayConductor>, ITransient
    {
        bool _smallHeader;

        protected OverlayViewModelBase() {
            this.SetCommand(x => x.CloseCommand).Subscribe(x => TryClose());
        }

        [Browsable(false)]
        public ReactiveCommand CloseCommand { get; private set; }
        [Browsable(false)]
        public bool SmallHeader
        {
            get { return _smallHeader; }
            set { SetProperty(ref _smallHeader, value); }
        }
    }
}