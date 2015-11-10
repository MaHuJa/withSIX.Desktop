// <copyright company="SIX Networks GmbH" file="NotLoggedInException.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Api.Models.Exceptions;

namespace SN.withSIX.Play.Core.Connect
{
    public class NotLoggedInException : UserException
    {
        public NotLoggedInException() : base("This action requires you to be logged in") {}
    }
}