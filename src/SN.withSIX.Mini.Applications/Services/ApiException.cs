// <copyright company="SIX Networks GmbH" file="ApiException.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Applications.Services
{
    public class ApiException : IDomainEvent
    {
        public ApiException(Exception exception) {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}