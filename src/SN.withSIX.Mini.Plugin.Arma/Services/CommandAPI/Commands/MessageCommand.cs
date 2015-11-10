// <copyright company="SIX Networks GmbH" file="MessageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI.Commands
{
    public class MessageCommand : MessageBase, ISendMessage
    {
        public static string Command = "message";
        public MessageCommand() {}

        public MessageCommand(string message) {
            Message = message;
        }

        public string ToGameCommand() {
            return Command + " " + Message;
        }
    }
}