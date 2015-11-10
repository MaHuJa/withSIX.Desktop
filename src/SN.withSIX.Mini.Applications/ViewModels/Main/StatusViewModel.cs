// <copyright company="SIX Networks GmbH" file="StatusViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases.Main;

namespace SN.withSIX.Mini.Applications.ViewModels.Main
{
    // TODO: Decide if Icons, Text or Colors are supposed to be part of the VM, or rather part of the Presentation layer..
    public class StatusViewModel : ViewModel, IStatusViewModel
    {
        StatusModel _status;

        public StatusViewModel(IObservable<StatusModel> statusObservable) {
            Abort = ReactiveCommand.CreateAsyncTask(
                async x => await RequestAsync(new AbortSyncing()).ConfigureAwait(false))
                .DefaultSetup("Abort");

            this.WhenActivated(d => { d(statusObservable.ObserveOnMainThread().BindTo(this, x => x.Status)); });
        }

        public StatusModel Status
        {
            get { return _status; }
            private set { this.RaiseAndSetIfChanged(ref _status, value); }
        }
        public ICommand Abort { get; }
        public ICommand SwitchQueue { get; set; }
    }

    public interface IStatusViewModel
    {
        StatusModel Status { get; }
        ICommand Abort { get; }
        ICommand SwitchQueue { get; set; }
    }
}