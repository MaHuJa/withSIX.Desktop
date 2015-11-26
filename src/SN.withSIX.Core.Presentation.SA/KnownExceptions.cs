// <copyright company="SIX Networks GmbH" file="KnownExceptions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Markup;
using SmartAssembly.SmartExceptionsCore;
using SN.withSIX.Core.Presentation.SA.ViewModels;
using SN.withSIX.Core.Presentation.SA.Views;
using Application = System.Windows.Application;

namespace SN.withSIX.Core.Presentation.SA
{
    public class KnownExceptions
    {
        static readonly Regex RXHL = new Regex(@"(http://[^\s\t\n\r]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        // defined here because otherwise burned empty into the error message.

        public static bool MainWindowShown { get; set; }
        public static string IsPremium { get; set; }
        public static string UserId { get; set; }

        static string HandleHyperlinks(string str) {
            return RXHL.Replace(str,
                match => String.Format("<controls:MyHyperlink NavigateUri=\"{0}\" >{0}</controls:MyHyperlink>",
                    match.Value));
            //.Replace("<", "&lt;")
            //.Replace(">", "&gt;");
        }

        Tuple<string, bool> HandleCompositionError(CompositionException e, ReportExceptionEventArgs et) {
            Tuple<string, bool> handled = null;
            if (e.Errors == null)
                return handled;
            foreach (var error in e.Errors.Where(x => x.Exception != null)) {
                if (handled == null || handled.Item1 == null)
                    handled = InternalHandler(error.Exception, et, true);
            }

            return handled;
        }

        Tuple<string, bool> HandleAggregateException(AggregateException aggregrateException,
            ReportExceptionEventArgs et) {
            Tuple<string, bool> handled = null;
            foreach (var error in aggregrateException.Flatten().InnerExceptions) {
                if (handled == null || handled.Item1 == null)
                    handled = InternalHandler(error, et); // Does not set the loader to true
            }

            return handled;
        }

        public bool Handle(Exception exception, ReportExceptionEventArgs e = null) {
            var r = InternalHandler(exception, e);

            if (string.IsNullOrEmpty(r.Item1))
                return r.Item2;

            var moreInfo = FormatException(exception);

            if (e != null) {
                e.AddCustomProperty("Six.Handled", r.Item1);
                e.TryToContinue = false;
            }

            ExitMessageHandler(exception, r, moreInfo);

            return r.Item2;
        }

        Tuple<string, bool> HandleReflectionTypeLoadException(ReflectionTypeLoadException e,
            ReportExceptionEventArgs et) {
            Tuple<string, bool> handled = null;
            foreach (var error in e.LoaderExceptions) {
                if (handled == null || handled.Item1 == null)
                    handled = InternalHandler(error, et, true);
            }

            return handled;
        }

        Tuple<string, bool> InternalHandler(Exception exception, ReportExceptionEventArgs et,
            bool loader = false) {
            Contract.Requires<ArgumentNullException>(exception != null);
            Contract.Requires<ArgumentNullException>(et != null);

            if (exception is ReportHandledException)
                return Tuple.Create((string) null, true);

            if (exception.InnerException != null) {
                var r = InternalHandler(exception.InnerException, et, loader);
                if (r != null)
                    return r;
            }

            var compositionException = exception as CompositionException;
            if (compositionException != null) {
                var r = HandleCompositionError(compositionException, et);
                if (r != null && r.Item1 != null)
                    return r;
            }

            var aggregrateException = exception as AggregateException;
            if (aggregrateException != null) {
                var r = HandleAggregateException(aggregrateException, et);
                if (r != null && r.Item1 != null)
                    return r;
            }

            var loaderException = exception as ReflectionTypeLoadException;
            if (loaderException != null) {
                var r = HandleReflectionTypeLoadException(loaderException, et);
                if (r != null && r.Item1 != null)
                    return r;
            }

            // Corrupted system files: https://dev-heaven.net/issues/28675 and https://dev-heaven.net/issues/34551
            // Desktop Composition: http://dev-heaven.net/issues/26847
            // SyncFlush: http://dev-heaven.net/issues/26098
            if (exception is COMException) {
                if (exception.Message.Contains("HRESULT")) {
                    if (exception.Message.Contains(@"{56FDF344-FD6D-11D0-958A-006097C9A090}") ||
                        exception.StackTrace.Contains("ApplyTaskbarItemInfo"))
                        return Tuple.Create(StrHresultTaskbar, false);
                    return Tuple.Create(StrHresult, true);
                }

                return Tuple.Create(StrInterop, true);
            }

            // HRESULT_FROM_WIN32(ERROR_NOT_FOUND)
            if (exception is XamlParseException) {
                if (exception.StackTrace != null
                    && exception.StackTrace.Contains("Standard.NativeMethods.GetCurrentThemeName"))
                    return Tuple.Create(StrChrome, true);
            }

            // Unable to Open configuration file: http://dev-heaven.net/issues/26856
            // Root Element Missing: http://dev-heaven.net/issues/21669
            // Not relevant for PwS
            //if (exception is ConfigurationErrorsException) {
            //return Tuple.Create(StrConfiguration, true);
            //}

            if (exception is TypeInitializationException) {
                // UIAutomationCore.dll: http://dev-heaven.net/issues/24462
                if (
                    exception.Message.IndexOf("System.Windows.Automation.InvokePatternIdentifiers",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    return Tuple.Create(StrTypeInitUiAutomationCore, true);
            }
            if (exception is DllNotFoundException) {
                // UIAutomationCore.dll: http://dev-heaven.net/issues/24462
                if (exception.Message.IndexOf("UIAutomationCore.dll", StringComparison.OrdinalIgnoreCase) >= 0)
                    return Tuple.Create(StrTypeInitUiAutomationCore, true);

                if (exception.Message.IndexOf(@"\Microsoft.NET\Framework\", StringComparison.OrdinalIgnoreCase) >= 0)
                    return Tuple.Create(StrTypeInitFramework, true);

                if (exception.Message.IndexOf(@"\Windows\", StringComparison.OrdinalIgnoreCase) >= 0)
                    return Tuple.Create(StrBadImageFormat, true);
            }

            if (exception is NotImplementedException) {
                if (exception.StackTrace != null
                    && exception.StackTrace.Contains("MS.Internal.AppModel.ITaskbarList.HrInit"))
                    return Tuple.Create(StrTbaritem, false);
            }

            if (exception is BadImageFormatException)
                return Tuple.Create(StrBadImageFormat, true);

            // More common errors, so try to catch them only in more specific situations
            // Retrieving the Com class ....: http://dev-heaven.net/issues/26851
            // But probably also when e.g System.Windows.Interactivity.dll is missing etc!!
            if (exception is FileNotFoundException) {
                if (loader
                    || exception.Message.Contains("System.")
                    || exception.Message.Contains("Microsoft.")
                    /*                    || exception.Message.Contains("System.Reactive.Linq,")
                    || exception.Message.Contains("System.Windows.Interactivity,")*/)
                    return Tuple.Create(StrFileNotFound, true);
            }

            if (exception is FileLoadException) {
                return Tuple.Create(exception.Message.Contains("'System.Core,") ? StrKB2468871 : StrCorrupt,
                    true);
            }

            return Tuple.Create((string) null, true);
        }

        static string Escape(string input) {
            return SecurityElement.Escape(input);

            //XmlDocument doc = new XmlDocument();
            //var xmlNode = doc.CreateNode("text", "mynode", "");
            //xmlNode.InnerText = input;
            //return xmlNode.OuterXml;
        }

        public static string ParseMessage(string msg, bool handleHyperlinks = true) {
            var escaped = Escape(msg);
            return handleHyperlinks
                ? HandleHyperlinks(escaped).Replace("\n", "<LineBreak />")
                : escaped.Replace("\n", "<LineBreak />");
        }

        void ExitMessageHandler(Exception e, Tuple<string, bool> r, string moreInfo = null) {
            const string title = "A known fatal error has occurred";

            var message = new XmlSanitizer().SanitizeXmlString(r.Item1);

            TryShowDialog(moreInfo, message, title, e);

            if (!r.Item2)
                return;

            Thread.Sleep(3000);
            Environment.Exit(1);
        }

        static void TryShowDialog(string moreInfo, string message, string title, Exception e) {
            try {
                ShowDialog(moreInfo, message, title, e);
            } catch (Exception exception) {
                var errorInfo = "\n\nError using proper error dialog:" + FormatException(exception);
                MessageBox.Show(moreInfo == null ? message + errorInfo : message + "\n\n" + moreInfo + errorInfo, title,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ShowDialog(string moreInfo, string message, string title, Exception e) {
            var app = Application.Current;
            var mainWindow = app == null || !MainWindowShown ? null : app.MainWindow;

            var vm = new ExceptionDialogViewModel(moreInfo) {
                // Not formatted... moreInfo == null ? null : HandleHyperlinks(Escape(moreInfo))
                Message = ParseMessage(message),
                Title = title,
                Exception = e
            };
            var window = new ExceptionDialogView {Owner = mainWindow, DataContext = vm};
            window.ShowDialog();
        }

        static string AddPrefix(string msg, int level = 0) {
            if (level == 0)
                return msg;
            var prefix = new string(' ', level*4);
            return string.Join("\n", msg.Split(separators, StringSplitOptions.None).Select(x => prefix + x));
        }

        static string PrettyPrint(IDictionary dict) {
            if (dict == null)
                return String.Empty;
            var dictStr = "[";
            var keys = dict.Keys;
            var i = 0;
            foreach (var key in keys) {
                dictStr += key + "=" + dict[key];
                if (i++ < keys.Count - 1)
                    dictStr += ", ";
            }
            return dictStr + "]";
        }

        public static string FormatException(Exception e, int level = 0) {
            if (e == null)
                throw new ArgumentNullException("e", "Exception to format can't be null");
            var str = new List<string> {
                AddPrefix(String.Format("Type: {0}", e.GetType()), level),
                AddPrefix(String.Format("Message:\n{0}", AddPrefix(e.Message, 1)), level),
                AddPrefix(String.Format("Source: {0}", e.Source), level),
                AddPrefix(String.Format("TargetSite: {0}", e.TargetSite), level)
            };

            ProcessAdditionalExceptionInformation(e, str);

            if (e.Data != null && e.Data.Count > 0)
                str.Add(AddPrefix(String.Format("Data: {0}", AddPrefix(PrettyPrint(e.Data), 1)), level));

            if (e.StackTrace != null)
                str.Add(AddPrefix(String.Format("StackTrace:\n{0}", AddPrefix(e.StackTrace, 1)), level));

            ProcessAdditionalEmbeddingExceptionTypes(e, level, str);

            return String.Join("\n", str);
        }

        static void ProcessAdditionalExceptionInformation(Exception e, ICollection<string> str) {
            var ee = e as ExternalException;
            if (ee != null)
                str.Add(AddPrefix(String.Format("ErrorCode: {0}", ee.ErrorCode)));

            var w32 = e as Win32Exception;
            if (w32 != null)
                str.Add(AddPrefix(String.Format("NativeErrorCode: {0}", w32.NativeErrorCode)));
        }

        static void ProcessAdditionalEmbeddingExceptionTypes(Exception e, int level, List<string> str) {
            var ae = e as AggregateException;
            if (ae != null) {
                str.AddRange(ae.Flatten().InnerExceptions.Select(
                    a => AddPrefix("Inner Exception:\n" + FormatException(a, 1), level)));
            } else {
                if (e.InnerException != null)
                    str.Add(AddPrefix("Inner Exception:\n" + FormatException(e.InnerException, 1), level));
            }

            var rle = e as ReflectionTypeLoadException;
            if (rle != null
                && rle.LoaderExceptions != null) {
                str.AddRange(rle.LoaderExceptions.Select(
                    a => AddPrefix("Inner Exception:\n" + FormatException(a, 1), level)));
            }

            var ce = e as CompositionException;
            if (ce != null
                && ce.Errors != null) {
                str.AddRange(ce.Errors.Select(
                    error => AddPrefix(
                        String.Format("CompositionError Description: {0}, Element: {1}, Exception: {2}",
                            error.Description, error.Element,
                            error.Exception == null ? null : FormatException(error.Exception, 1)), level)));
            }
        }

        #region messages

        const string StrPossiblyCorruptNet =
            "Or possibly corrupt .NET framework 4.5 - try uninstalling it, reinstalling the latest version (http://www.microsoft.com/en-us/download/details.aspx?id=42643), running all windows updates and restart.";

        static readonly string StrInterop =
            "Desktop composition probably lost during program operation. Please restart the software.";

        public static readonly string StrConfiguration =
            "Please try removing the configuration folders and files in Appdata\\Local\\Play withSIX and Appdata\\Roaming\\Play withSIX";

        static readonly string Sfc = "Please try running: 'sfc /scannow' from an Administrator command prompt.";

        static readonly string StrBadImageFormat =
            "Probably corrupted or missing System Files." + Sfc + " And retry." +
            "\n" +
            "If still issues, try reinstalling the .NET 4.5 framework http://www.microsoft.com/en-us/download/details.aspx?id=42643";

        static readonly string StrTypeInitUiAutomationCore =
            "Probably missing, wrong version or corrupted UIAutomationCore.dll\n" + StrBadImageFormat;

        static readonly string StrTypeInitFramework =
            "Probably missing or corrupted .NET Framework files, try repairing/reinstalling the .NET framework. Or corrupted system files, " +
            Sfc + " And retry.";

        static readonly string StrReinstall =
            "Probably corrupted or missing application files.\n\nPlease try repairing the application, either by:\n- Running the software from the installed Start Menu icon\n- Or running the repair action in Windows Control Panel, Programs and Features, Play withSIX\n- Or reinstalling the software (Uninstall, completely delete all application traces (files, folders and shortcuts) then install the latest available version), and try again.";

        static readonly string StrCorrupt = StrReinstall + "\n\n" + StrPossiblyCorruptNet;
        static readonly string StrFileNotFound = StrCorrupt;

        static readonly string StrTbaritem =
            "Something appears to be wrong when handling the Taskbar, please report issue with details to Support";

        static readonly string StrChrome =
            "Something appears to be wrong when handling the WindowChrome\nThis is usually caused by the active Windows Theme. Please try changing the Windows Theme to another. See the FAQ for more info";

        static readonly string StrHresult =
            "System files are probably corrupted, please run the system file checker (from admin command prompt): sfc /scannow\n\n"
            + StrPossiblyCorruptNet;

        static readonly string StrHresultTaskbar =
            "Possibly related to the Windows 7 (and newer) Taskbar: http://msdn.microsoft.com/en-us/library/windows/desktop/dd378460(v=vs.85).aspx\nMore details: https://community.withsix.com\n\n" +
            StrHresult;

        static readonly string StrKB2468871 =
            "It appears the Microsoft .NET 4 hotfix 'KB2468871v2' is not installed and Play withSIX cannot continue\n\nPlease install the hotfix and try again:\nhttp://support.microsoft.com/kb/2468871 or:\nhttp://www.microsoft.com/en-us/download/details.aspx?id=3556\n\nAlternatively you could install the .NET Framework 4.5 instead:\nhttp://www.microsoft.com/en-us/download/details.aspx?id=30653";
        static readonly string[] separators = {"\r\n", "\n"};

        #endregion
    }
}