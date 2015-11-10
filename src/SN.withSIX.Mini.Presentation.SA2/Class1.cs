// <copyright company="SIX Networks GmbH" file="Class1.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using SmartAssembly.SmartExceptionsCore;

namespace SN.withSIX.Mini.Presentation.SA2
{
    public class UnhandledExceptionHandler : SmartAssembly.SmartExceptionsCore.UnhandledExceptionHandler
    {
        protected override void OnReportException(ReportExceptionEventArgs e) {
            //LogError(e.Exception);
            //TryAddFullExceptionTrace(e);
            TryAddSystemInfo(e);
        }

        void TryAddSystemInfo(ReportExceptionEventArgs e) {
            try {
                e.AddCustomProperty("Elevation Status", (!new UacHelper().CheckUac()).ToString());
            } catch {}
            try {
                e.AddCustomProperty("SEMVER", GetProductVersion());
            } catch {}
            try {
                e.AddCustomProperty("Uptime", (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds + "s");
            } catch {}
            // TODO: Attach Log file?
            e.TryToContinue = false;
            e.SendReport();
            // TODO
            /*
            try {
                e.AddCustomProperty("IsPremium", KnownExceptions.IsPremium);
            } catch { }
            try {
                e.AddCustomProperty("UserId", KnownExceptions.UserId);
            } catch { }
*/
        }

        public string GetProductVersion() {
            var attr = Attribute
                .GetCustomAttribute(
                    Assembly.GetEntryAssembly(),
                    typeof (AssemblyInformationalVersionAttribute))
                as AssemblyInformationalVersionAttribute;
            return attr.InformationalVersion;
        }

        protected override void OnFatalException(FatalExceptionEventArgs e) {
            //LogError(e.FatalException);
            MessageBox.Show(e.FatalException.ToString(), string.Format("{0} Fatal Error", ApplicationName),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnSecurityException(SecurityExceptionEventArgs e) {
            //LogError(e.FatalException);
            MessageBox.Show(e.SecurityMessage + "\n" + e.SecurityException,
                string.Format("{0} Security Error", ApplicationName),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool AttachApp() {
            try {
                AttachExceptionHandler(new UnhandledExceptionHandler());
                return true;
            } catch (SecurityException) {
                var securityMessage =
                    string.Format(
                        "{0} cannot initialize itself because some permissions are not granted.\n\nYou probably try to launch {0} in a partial-trust situation. It's usually the case when the application is hosted on a network share.\n\nYou need to run {0} in full-trust, or at least grant it the UnmanagedCode security permission.\n\nTo grant this application the required permission, contact your system administrator, or use the Microsoft .NET Framework Configuration tool.",
                        ApplicationName);

                MessageBox.Show(securityMessage, string.Format("{0} Security Error", ApplicationName),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}