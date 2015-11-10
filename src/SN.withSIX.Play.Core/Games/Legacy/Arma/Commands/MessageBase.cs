// <copyright company="SIX Networks GmbH" file="MessageBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy.Arma.Commands
{
    public abstract class MessageBase : IMessage
    {
        public string Message { get; set; }
    }
}