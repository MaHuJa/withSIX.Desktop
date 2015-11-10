// <copyright company="SIX Networks GmbH" file="SoftwareUpdateViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels
{
    [DoNotObfuscate]
    public interface ISoftwareUpdateViewModel : IModalScreen {}

    [DoNotObfuscate]
    public class SoftwareUpdateViewModel : ReactiveModalScreen<IShellViewModelFullBase>, ISoftwareUpdateViewModel
    {
        public SoftwareUpdateViewModel(ISoftwareUpdate softwareUpdate) {
            DisplayName = "check for updates";
            SoftwareUpdate = softwareUpdate;

            this.WhenAnyValue(x => x.IsActive)
                .Where(x => x)
                .Subscribe(x => CheckForUpdates());
        }

        public ISoftwareUpdate SoftwareUpdate { get; set; }

        [DoNotObfuscate]
        public async void CheckForUpdates() {
            await SoftwareUpdate.TryCheckForUpdates().ConfigureAwait(false);
        }

        [DoNotObfuscate]
        public void ViewChangelog() {
            BrowserHelper.TryOpenUrlIntegrated(CommonUrls.SuclUrl);
        }

        [ReportUsage]
        [DoNotObfuscate]
        public void Restart() {
            SoftwareUpdate.UpdateAndExitIfNotBusy();
        }
    }
}