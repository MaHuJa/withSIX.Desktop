// <copyright company="SIX Networks GmbH" file="StartWithWindowsHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reflection;
using Microsoft.Win32;

namespace SN.withSIX.Mini.Applications.NotificationHandlers
{
    public class StartWithWindowsHandler
    {
        public void HandleStartWithWindows(bool startWithWindows) {
            var rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var location = Assembly.GetEntryAssembly().Location;
            if (startWithWindows) {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue("withSIX", location);
            } else {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue("withSIX", false);
            }
        }
    }
}