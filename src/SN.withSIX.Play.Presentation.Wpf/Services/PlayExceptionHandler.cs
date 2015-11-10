// <copyright company="SIX Networks GmbH" file="PlayExceptionHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using ReactiveUI;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Presentation.Wpf.Services;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Services
{
    public class PlayExceptionHandler : DefaultWpfExceptionhandler
    {
        protected override UserError HandleExceptionInternal(Exception ex, string action = "Action") {
            Contract.Requires<ArgumentNullException>(action != null);

            if (ex is NotConnectedException)
                return new NotConnectedUserError(innerException: ex);

            if (ex is NotLoggedInException)
                return new NotLoggedInUserError(innerException: ex);
            if (ex is BusyStateHandler.BusyException)
                return new BusyUserError(innerException: ex);

            return Handle(ex, action);
        }
    }
}