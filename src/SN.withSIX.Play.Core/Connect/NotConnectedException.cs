// <copyright company="SIX Networks GmbH" file="NotConnectedException.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Api.Models.Exceptions;

namespace SN.withSIX.Play.Core.Connect
{
    public class NotConnectedException : UserException
    {
        public NotConnectedException() : base("This action requires you to be connected") {}
    }
}