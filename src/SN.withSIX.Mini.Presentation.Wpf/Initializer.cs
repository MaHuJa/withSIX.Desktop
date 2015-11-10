// <copyright company="SIX Networks GmbH" file="Initializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    public class Initializer : IInitializer
    {
        readonly IToolsInstaller _toolsInstaller;

        public Initializer(IToolsInstaller toolsInstaller) {
            _toolsInstaller = toolsInstaller;
        }

        public async Task Initialize() {
            //await SetupNotificationIcon().ConfigureAwait(false);
        }

        public async Task Deinitialize() {}

        async Task SetupNotificationIcon() {
            var ps1 = Path.GetTempFileName() + ".ps1";
            var assembly = GetType().Assembly; // Executing assembly??
            using (var fs = new FileStream(ps1, FileMode.Create))
            using (
                var s =
                    assembly.GetManifestResourceStream("SN.withSIX.Mini.Presentation.Wpf.Resources.TrayIcon.ps1")
                )
                await s.CopyToAsync(fs).ConfigureAwait(false);
            using (
                await
                    ReactiveProcess.StartAsync("powershell.exe", "-File \"" + ps1 + "\" '" + assembly.Location + "' 2")
                        .ConfigureAwait(false))
                ;
        }
    }
}