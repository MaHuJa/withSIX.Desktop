// <copyright company="SIX Networks GmbH" file="InterfaceSettingsTabViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Annotations;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Settings;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    public interface IInterfaceSettingsTabViewModel : IAppSettingsViewModel
    {
        bool OptOutReporting { get; set; }
        bool ShowDesktopNotifications { get; set; }
        bool StartWithWindows { get; set; }
        string Version { get; }
        ICommand ViewLicense { get; }
        ICommand ImportPwsSettings { get; }
    }

    [Order(1)]
    public class InterfaceSettingsTabViewModel : SettingsTabViewModel, IInterfaceSettingsTabViewModel
    {
        public InterfaceSettingsTabViewModel() {
            ImportPwsSettings = ReactiveCommand.CreateAsyncTask(async x => await ImportSettings()).DefaultSetup("Import PwS Settings");
        }
        public override string DisplayName => "General";
        public bool OptOutReporting { get; set; }
        public bool ShowDesktopNotifications { get; set; }
        public bool StartWithWindows { get; set; }
        public string Version { get; set; }
        public ICommand ViewLicense { get; }
            =
            ReactiveCommand.CreateAsyncTask(x => RequestAsync(new OpenWebLink(ViewType.License))).DefaultSetup("View License");
        public ICommand ImportPwsSettings { get; }

        async Task ImportSettings() {
            await RequestAsync(new ImportPwsSettings()).ConfigureAwait(false);
            await
                Cheat.DialogManager.MessageBoxAsync(new MessageBoxDialogParams("Settings imported succesfully",
                    "Success"));
        }
    }
}