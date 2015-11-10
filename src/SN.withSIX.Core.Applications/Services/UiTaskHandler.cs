// <copyright company="SIX Networks GmbH" file="UiTaskHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core.Applications.Services
{
    public static class UiTaskHandler
    {
        static IExceptionHandler _exceptionHandler;

        public static Task<bool> TryAction(Func<Task> act, string action = null) {
            return _exceptionHandler.TryExecuteAction(act, action);
        }

        public static void SetExceptionHandler(IExceptionHandler handler) {
            _exceptionHandler = handler;
        }

        public static ReactiveCommand<Unit> CreateAsyncVoidTask(Func<Task> task) {
            return ReactiveCommand.CreateAsyncTask(async x => await task().ConfigureAwait(false));
        }

        public static TCommand DefaultSetup<TCommand>(this TCommand command, string name)
            where TCommand : IReactiveCommand {
            SetupDefaultThrownExceptionsHandler(command, name);
            command.Setup(name);
            return command;
        }

        static void SetupDefaultThrownExceptionsHandler<TCommand>(TCommand command, string action)
            where TCommand : IReactiveCommand {
            // ThrownExceptions does not listen to Subscribe errors, but only in async task errors!
            command.ThrownExceptions
                .Select(x => HandleException(x, action))
                .SelectMany(UserError.Throw)
                .Where(x => x == RecoveryOptionResult.RetryOperation)
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(command);
        }

        public static UserError HandleException(Exception ex, string action = "Action") {
            return _exceptionHandler.HandleException(ex, action);
        }

        static TCommand Setup<TCommand>(this TCommand command, string name) where TCommand : IReactiveCommand {
#if DEBUG || (!MAIN_RELEASE && !BETA_RELEASE)
            SetupBencher(command, name);
#endif
            return command;
        }

        static void SetupBencher(IReactiveCommand command, string name = null) {
            Bencher bencher = null;
            command.IsExecuting.Subscribe(x => HandleBencher(x, ref bencher, name));
        }

        static void HandleBencher(bool b, ref Bencher bencher, string name) {
            if (b)
                bencher = new Bencher("UICommand", name ?? "TODO");
            else if (bencher != null)
                bencher.Dispose();
        }

        public static void RegisterHandler(IExceptionHandlerHandle exceptionHandlerHandle) {
            _exceptionHandler.RegisterHandler(exceptionHandlerHandle);
        }
    }
}