// <copyright company="SIX Networks GmbH" file="IReceiveMessage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy.Arma.Commands
{
    public interface IReceiveMessage : IMessage
    {
        void ParseInput(string substring);
    }
}