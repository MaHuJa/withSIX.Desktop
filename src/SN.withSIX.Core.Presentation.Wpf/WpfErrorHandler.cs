// <copyright company="SIX Networks GmbH" file="WpfErrorHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
#if !DEBUG
using SmartAssembly.ReportException;
#endif

namespace SN.withSIX.Core.Presentation.Wpf
{
    public class WpfErrorHandler
    {
        // We cant use SmartAss in non merged assemblies..
        public static Action<Exception> Report = x => { };
        readonly IDialogManager _dialogManager;

        public WpfErrorHandler(IDialogManager dialogManager) {
            _dialogManager = dialogManager;
        }

        public Task<RecoveryOptionResult> Handler(UserError error, Window window = null) {
            return error.RecoveryOptions != null && error.RecoveryOptions.Any()
                ? UiRoot.Main.ErrorDialog(error, window)
                :
#if DEBUG
                UnhandledError(error, window);
#else
            BasicHandler(error, window);
#endif
        }

        Task<RecoveryOptionResult> BasicHandler(UserError error, Window window) {
            return BasicMessageHandler(error, window);
        }

        async Task<RecoveryOptionResult> BasicMessageHandler(UserError userError, Window window) {
            MainLog.Logger.Error(userError.InnerException.Format());
            var id = Guid.Empty;
#if !DEBUG
            var ex = new UserException(userError.ErrorMessage, userError.InnerException);
            id = ex.Id;
            Report(ex);
#endif
            // NOTE: this code really shouldn't throw away the MessageBoxResult
            var message = userError.ErrorCauseOrResolution +
                          "\n\nWe've been notified about the problem (Your ID: " + id + ")." +
                          "\n\nPlease make sure you are running the latest version of the software.\n\nIf the problem persists, please contact Support: http://community.withsix.com";
            var title = (userError.ErrorMessage ?? "An error has occured while trying to process the action");
            var result =
                await
                    _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(message, title) {Owner = window})
                        .ConfigureAwait(false);
            return RecoveryOptionResult.CancelOperation;
        }

        async Task<RecoveryOptionResult> UnhandledError(UserError error, Window window = null) {
            var message = error.ErrorCauseOrResolution;
            var title = error.ErrorMessage;
            var result = await _dialogManager.ExceptionDialogAsync(error.InnerException.UnwrapExceptionIfNeeded(),
                message, title, window).ConfigureAwait(false);
            //await _dialogManager.ExceptionDialogAsync(x.InnerException, x.ErrorCauseOrResolution,
            //x.ErrorMessage);
            //var result = await Dispatcher.InvokeAsync(() => _exceptionHandler.HandleException(arg.InnerException));
            // TODO: Should actually fail and rethrow as an exception to then be catched by unhandled exception handler?
            // TODO: Add proper retry options. e.g for Connect - dont just show a connect dialog, but make it part of the retry flow;
            // pressing Connect, and succesfully connect, should then retry the action?
            return result ? RecoveryOptionResult.CancelOperation : RecoveryOptionResult.FailOperation;
        }
    }


    public class UserException : Exception
    {
        public UserException(string message, Exception innerException) : base(message, innerException) {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}