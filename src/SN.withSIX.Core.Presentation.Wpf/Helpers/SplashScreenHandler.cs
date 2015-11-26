using System;
using System.Windows;

namespace SN.withSIX.Core.Presentation.Wpf.Helpers
{
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