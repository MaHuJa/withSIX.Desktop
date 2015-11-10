// <copyright company="SIX Networks GmbH" file="BasicUserErrorHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using ReactiveUI;
using SmartAssembly.ReportException;
using SmartAssembly.SmartExceptionsCore;
using SN.withSIX.Core.Presentation.Wpf;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    public class BasicUserErrorHandler
    {
        public static ReportSender SADummy = null;

        public static IDisposable RegisterDefaultHandler(Window window)
            => UserError.RegisterHandler(error => UiRoot.Main.ErrorHandler.Handler(error, window));

        public static void Report(Exception ex) {
            ExceptionReporting.Report(ex);
        }
    }
}