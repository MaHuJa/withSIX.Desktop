// <copyright company="SIX Networks GmbH" file="IReceiveMessage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI.Commands
{
    public interface IReceiveMessage : IMessage
    {
        void ParseInput(string substring);
    }
}