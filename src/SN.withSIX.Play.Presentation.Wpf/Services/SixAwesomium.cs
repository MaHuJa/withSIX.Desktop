// <copyright company="SIX Networks GmbH" file="SixAwesomium.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Windows;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Assemblies;

namespace SN.withSIX.Play.Presentation.Wpf.Services
{
    public class SixAwesomium
    {
        readonly Uri _awesomiumUri;
        readonly Uri _flashUri;

        public SixAwesomium(Uri awesomiumUri, Uri flashUri) {
            _awesomiumUri = awesomiumUri;
            _flashUri = flashUri;
        }

        public void HandleAwesomium() {
            InstallFlash();
        }

        void InstallFlash() {
            try {
                TryInstallFlash();
            } catch (Exception e) {
                MainLog.Logger.FormattedWarnException(e, "Error while installing flash");
                MessageBox.Show(
                    String.Format(
                        "Failed installing pre-requisites, please make sure you are connected to the internet:\n"
                        +
                        "You can try install manually from: http://www.adobe.com/support/flashplayer/downloads.html \n\n"
                        + "Error details: {0}: {1}", e.GetType(), e.Message)
                    + "\n\nFor Support please visit withsix.com/support");
            }
        }

        bool TryInstallFlash() {
            var installer = new FlashInstaller(Path.GetTempPath(), _flashUri);
            if (installer.IsInstalled())
                return false;

            using (BuildPreRequisiteSplashScreen())
                ((Action) installer.Install)();
            return true;
        }

        static SplashScreenHandler BuildPreRequisiteSplashScreen() {
            return new SplashScreenHandler(new SplashScreen("PrerequisitesInstalling.png"));
        }
    }

    public class SplashScreenHandler : IDisposable
    {
        readonly SplashScreen _splashScreen;

        public SplashScreenHandler(SplashScreen splashScreen) {
            _splashScreen = splashScreen;
            splashScreen.Show(true, false);
        }

        public void Dispose() {
            _splashScreen.Close(TimeSpan.FromMilliseconds(0));
        }
    }
}