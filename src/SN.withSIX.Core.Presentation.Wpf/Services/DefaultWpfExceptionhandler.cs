// <copyright company="SIX Networks GmbH" file="DefaultWpfExceptionhandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class DefaultWpfExceptionhandler : IExceptionHandler
    {
        readonly ICollection<IExceptionHandlerHandle> _handlers = new List<IExceptionHandlerHandle>();

        public UserError HandleException(Exception ex, string action = "Action") {
            Contract.Requires<ArgumentNullException>(action != null);

            var unwrapped = UnwrapExceptionIfNeeded(ex);

            foreach (var r in _handlers.Select(h => h.HandleException(unwrapped, action)).Where(r => r != null))
                return r;
            return HandleExceptionInternal((dynamic) unwrapped, action);
        }

        public async Task<bool> TryExecuteAction(Func<Task> action, string message = null) {
            Exception e = null;
            try {
                await action().ConfigureAwait(false);
                return true;
            } catch (Exception ex) {
                e = ex;
            }
            return await UserError.Throw(HandleException(e)) != RecoveryOptionResult.FailOperation;
            //return false;
        }

        public void RegisterHandler(IExceptionHandlerHandle exceptionHandlerHandle) {
            _handlers.Add(exceptionHandlerHandle);
        }

        protected virtual UserError HandleExceptionInternal(Exception ex, string action = "Action") {
            return Handle((dynamic) ex, action);
        }

        protected static UserError Handle(RepositoryLockException ex, string action) {
            return new BasicUserError("It seems another program is locking the repository",
                "Please close other applications, like Play withSIX, and try again", innerException: ex);
        }

        protected static UserError Handle(OperationCanceledException ex, string action) {
            return new CanceledUserError(action, innerException: ex);
        }

        protected static UserError Handle(Exception ex, string action) {
            var message = "An unexpected error has occurred while trying to execute the requested action:" +
                          "\n" + ex.Message;
            var title = "An error has occured while trying to '" + GetHumanReadableActionName(action) + "'";
            return new BasicUserError(title, message, innerException: ex);
        }

        protected static string GetHumanReadableActionName(string action) {
            return action.Split('.').Last();
        }

        // TODO: ability to unwrap other exception kinds? -> Extension Method?
        protected static Exception UnwrapExceptionIfNeeded(Exception ex) {
            return ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
        }
    }
}