// <copyright company="SIX Networks GmbH" file="ServiceInstall.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Updater.Presentation.Wpf
{
    [RunInstaller(true)]
    [DoNotObfuscateType]
    public class SixElevatedServiceInstaller : Installer
    {
        public SixElevatedServiceInstaller() {
            var serviceProcessInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller.DisplayName = "Play withSIX Updater Service";
            serviceInstaller.Description =
                "Allows file operations to be performed by an elevated service so user will not be bothered by constant UAC screens.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = SixElevatedServiceMisc.SixElevatedServiceName;

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);

            Committed += OnInstalled;
        }

        void OnInstalled(object sender, InstallEventArgs e) {
            var controller = new ServiceController(SixElevatedServiceMisc.SixElevatedServiceName);
            controller.Start();
        }
    }
}