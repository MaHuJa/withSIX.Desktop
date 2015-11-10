// <copyright company="SIX Networks GmbH" file="HubExceptionExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Microsoft.AspNet.SignalR.Client;
using SN.withSIX.Api.Models.Exceptions;

namespace SN.withSIX.Play.Core.Extensions
{
    public static class HubExceptionExtensions
    {
        public static Exception GetException(this HubException hubException) {
            return UserException.CreateException(hubException.ErrorData) ?? hubException;
        }
    }
}