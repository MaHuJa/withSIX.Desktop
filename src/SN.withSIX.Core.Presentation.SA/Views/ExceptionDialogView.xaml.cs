// <copyright company="SIX Networks GmbH" file="ExceptionDialogView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows;
using SmartAssembly.ReportException;
using SmartAssembly.SmartExceptionsCore;
using SN.withSIX.Core.Presentation.SA.ViewModels;

namespace SN.withSIX.Core.Presentation.SA.Views
{
    [DoNotObfuscate]
    // This does not use commands but click eventhandlers because it is being used outside WindowManager usage.
    public partial class ExceptionDialogView : Window
    {
        public ExceptionDialogView() {
            InitializeComponent();
        }

        void CancelButtonClick(object sender, RoutedEventArgs e) {
            var vm = (ExceptionDialogViewModel) DataContext;
            vm.Cancel = true;

            Close();
        }

        void CloseButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        void ReportButtonClick(object sender, RoutedEventArgs e) {
            var vm = (ExceptionDialogViewModel) DataContext;

            var ex = vm.Exception;
            if (vm.Message != null)
                ex = new ReportHandledException(vm.Message, ex);
            ExceptionReporting.Report(ex);

            Close();
        }

        void ThrowButtonClick(object sender, RoutedEventArgs e) {
            var vm = (ExceptionDialogViewModel) DataContext;
            vm.Throw = true;

            Close();
        }
    }

    [DoNotObfuscate]
    [Serializable]
    public sealed class ReportHandledException : Exception
    {
        public ReportHandledException(string message)
            : base(message) {}

        public ReportHandledException(string message, Exception innerException)
            : base(message, innerException) {}

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        // Is private because is sealed class
        ReportHandledException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}