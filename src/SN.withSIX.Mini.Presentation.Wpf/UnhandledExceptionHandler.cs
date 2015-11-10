// <copyright company="SIX Networks GmbH" file="UnhandledExceptionHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using ReactiveUI;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Wpf.Services;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    public class UnhandledExceptionHandler : DefaultWpfExceptionhandler, IPresentationService
    {
        protected override UserError HandleExceptionInternal(Exception ex, string action = "Action") {
            Contract.Requires<ArgumentNullException>(action != null);
            return Handle((dynamic) ex, action);
        }
    }
}