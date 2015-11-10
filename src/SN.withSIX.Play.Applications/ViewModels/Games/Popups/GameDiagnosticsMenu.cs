// <copyright company="SIX Networks GmbH" file="GameDiagnosticsMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Popups
{
    public class GameDiagnosticsMenu : ContextMenuBase
    {
        readonly IDialogManager _dialogManager;

        public GameDiagnosticsMenu(IDialogManager dialogManager) {
            _dialogManager = dialogManager;
        }

        [MenuItem]
        public Task DiagnoseAndRepairSynqRepository() {
            var vm = new RepairViewModel();
            vm.ProcessCommand.Execute(null);
            return _dialogManager.ShowDialogAsync(vm);
        }
    }
}