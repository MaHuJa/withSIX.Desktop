// <copyright company="SIX Networks GmbH" file="ShellViewModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using IScreen = Caliburn.Micro.IScreen;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public abstract class ShellViewModelBase : ReactiveConductor<IScreen>.Collection.OneActive, IShellViewModelBase
        /*, IHandle<KeyEventArgs>*/
    {
        [DoNotObfuscate] protected readonly ObservableAsPropertyHelper<bool> _mainContentEnabled;
        [DoNotObfuscate] protected readonly ObservableAsPropertyHelper<bool> _modalItemShowing;
        protected IModalScreen _modalActiveItem;
        protected bool _modalViewCanCancel;

        protected ShellViewModelBase() {
            this.SetCommand(x => x.BackCommand).Subscribe(x => ActiveItem.TryClose());

            // TODO
            //_modalItemShowing = this.WhenAnyValue(x => x.ModalActiveItem != null)
            //.ToProperty(this, x => x.ModalItemShowing, false, Scheduler.Immediate);
            //_mainContentEnabled = this.WhenAny(x => x.ModalItemShowing, x => !x.Value)
            //.ToProperty(this, x => x.MainContentEnabled, false, Scheduler.Immediate);
        }

        public ReactiveCommand BackCommand { get; protected set; }
        public bool ModalViewCanCancel
        {
            get { return _modalViewCanCancel; }
            set { SetProperty(ref _modalViewCanCancel, value); }
        }
        public bool MainContentEnabled
        {
            get { return _mainContentEnabled.Value; }
        }
        public abstract void ShowDashboard();

        [DoNotObfuscate]
        public void CancelModalView() {
            var item = ModalActiveItem;
            if (item != null)
                item.Cancel();
        }

        [DoNotObfuscate]
        public void HideModalView() {
            ModalActiveItem = null;
        }

        [DoNotObfuscate]
        public void ShowModalView(IModalScreen viewModel) {
            viewModel.Parent = this;
            ModalViewCanCancel = viewModel.ShowBackButton;
            viewModel.Activate();
            ModalActiveItem = viewModel;
        }

        public IModalScreen ModalActiveItem
        {
            get { return _modalActiveItem; }
            set { SetProperty(ref _modalActiveItem, value); }
        }
        public bool ModalItemShowing
        {
            get { return _modalItemShowing.Value; }
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            ShowDashboard();
        }

        /*        #region IHandle events

        public void Handle(KeyEventArgs e) {
            if (!ModalItemShowing || !e.IsNavigateBack())
                return;
            CancelModalView();
            e.Handled = true;
        }

        #endregion*/

        public abstract void ShowOverlay(IScreen overlayViewModelBase);
    }

    public abstract class ShellViewModelFullBase : ShellViewModelBase, IShellViewModelFullBase
    {
        public abstract void ShowAbout();
        public abstract void ShowLicenses();
        public abstract void ShowOptions();
        public abstract void UpdateSoftware(bool b);
    }
}