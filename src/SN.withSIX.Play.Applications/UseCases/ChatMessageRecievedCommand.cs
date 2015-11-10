// <copyright company="SIX Networks GmbH" file="ChatMessageRecievedCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class ChatMessageRecievedCommand : IAsyncRequest<UnitType> {}

    public class ChatMessageRecievedCommandHandler : IAsyncRequestHandler<ChatMessageRecievedCommand, UnitType>
    {
        public async Task<UnitType> HandleAsync(ChatMessageRecievedCommand request) {
            throw new NotImplementedException();
        }
    }
}