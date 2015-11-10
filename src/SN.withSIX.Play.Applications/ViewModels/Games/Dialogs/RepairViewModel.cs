// <copyright company="SIX Networks GmbH" file="RepairViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Dialogs
{
    public interface IRepairViewModel : ISupportsActivation, IDialog
    {
        ReactiveCommand<object> OkCommand { get; }
        ReactiveCommand<Unit> ProcessCommand { get; }
    }

    public class RepairViewModel : DialogBase, IRepairViewModel
    {
        public RepairViewModel() {
            DisplayName = "Diagnose and Repair Synq Repository";
            Activator = new ViewModelActivator();
            ReactiveCommand.CreateAsyncTask(
                x => DomainEvilGlobal.SelectedGame.ActiveGame.Controller.BundleManager.Repo.Repair())
                .SetNewCommand(this, x => x.ProcessCommand)
                .Subscribe();

            ReactiveCommand.Create(ProcessCommand.IsExecuting.Select(x => !x))
                .SetNewCommand(this, x => x.OkCommand)
                .Subscribe(x => TryClose());

            //this.WhenActivated(d => ProcessCommand.Execute(null));
        }

        public ReactiveCommand<Unit> ProcessCommand { get; private set; }
        public ReactiveCommand<object> OkCommand { get; private set; }
        public ViewModelActivator Activator { get; }
    }
}