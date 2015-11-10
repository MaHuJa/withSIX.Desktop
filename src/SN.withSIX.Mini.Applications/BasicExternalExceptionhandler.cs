// <copyright company="SIX Networks GmbH" file="BasicExternalExceptionhandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI;
using SN.withSIX.Core.Applications.Errors;

namespace SN.withSIX.Mini.Applications
{
    public abstract class BasicExternalExceptionhandler : IExceptionHandlerHandle
    {
        public abstract UserError HandleException(Exception ex, string action = "Action");

        protected static UserError Handle(Exception exception, string action) {
            return null;
        }
    }
}