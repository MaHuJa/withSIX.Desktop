// <copyright company="SIX Networks GmbH" file="UnhandledExceptionHandlerWithAdvancedUI.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using NLog;
using SmartAssembly.SmartExceptionsCore;

namespace SN.withSIX.Core.Presentation.SA
{
    // http://www.simple-talk.com/dotnet/.net-tools/customize-automated-error-reporting-in-smartassembly/
    // http://www.red-gate.com/products/dotnet-development/smartassembly/learn-more/walkthroughs/adding-customized-dialog
    public class UnhandledExceptionHandlerWithAdvancedUI : UnhandledExceptionHandler
    {
        protected override Guid GetUserID() {
            const string registryString = "AnonymousID";

            try {
                var savedID = RegistryHelper.TryReadHKLMRegistryString(registryString);

                if (savedID.Length == 0) {
                    var newID = Guid.NewGuid();
                    RegistryHelper.TrySaveHKLMRegistryString(registryString, newID.ToString("B"));

                    if (RegistryHelper.TryReadHKLMRegistryString(registryString).Length > 0)
                        return newID;
                    return Guid.Empty;
                }
                return new Guid(savedID);
            } catch {
                return Guid.Empty;
            }
        }

        protected override void OnSecurityException(SecurityExceptionEventArgs e) {
            LogError(e.SecurityException);
            var form = new SecurityExceptionForm(e);
            form.ShowDialog();
        }

        protected override void OnReportException(ReportExceptionEventArgs e) {
            LogError(e.Exception);
            TryAddFullExceptionTrace(e);
            TryAddSystemInfo(e);

            // TODO: Find the ReportId... and include it..

            var route = new KnownExceptions().Handle(e.Exception, e);

            if (route) {
                // Route on to SA
                var form = new ExceptionReportingForm(this, e);
                form.ShowDialog();
            } else {
                // Lets just try to continue...
                e.TryToContinue = true;
            }
        }

        static void LogError(Exception ex) {
            try {
                LogManager.GetCurrentClassLogger().Error(KnownExceptions.FormatException(ex), ex);
            } catch (Exception e) {
                try {
                    LogManager.GetCurrentClassLogger().Error("Error during error handling", e);
                    LogManager.GetCurrentClassLogger().Error("Error", ex);
                } catch {}
            }
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

        static void TryAddFullExceptionTrace(ReportExceptionEventArgs e) {
            try {
                e.AddCustomProperty("Full exception trace", KnownExceptions.FormatException(e.Exception));
            } catch {}
        }

        protected override void OnFatalException(FatalExceptionEventArgs e) {
            LogError(e.FatalException);
            MessageBox.Show(e.FatalException.ToString(), string.Format("{0} Fatal Error", ApplicationName),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool AttachApp() {
            try {
                AttachExceptionHandler(new UnhandledExceptionHandlerWithAdvancedUI());
                return true;
            } catch (SecurityException) {
                try {
                    Application.EnableVisualStyles();
                    var securityMessage =
                        string.Format(
                            "{0} cannot initialize itself because some permissions are not granted.\n\nYou probably try to launch {0} in a partial-trust situation. It's usually the case when the application is hosted on a network share.\n\nYou need to run {0} in full-trust, or at least grant it the UnmanagedCode security permission.\n\nTo grant this application the required permission, contact your system administrator, or use the Microsoft .NET Framework Configuration tool.",
                            ApplicationName);
                    var form =
                        new SecurityExceptionForm(new SecurityExceptionEventArgs(securityMessage, false));
                    form.ShowInTaskbar = true;
                    form.ShowDialog();
                } catch (Exception exception) {
                    BasicExceptionDialog(exception);
                }
                return false;
            }
        }

        static void BasicExceptionDialog(Exception exception) {
            MessageBox.Show(exception.ToString(), string.Format("{0} Fatal Error", ApplicationName),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}