// <copyright company="SIX Networks GmbH" file="ReplyCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI.Commands
{
    public class ReplyCommand : MessageCommand, ISendReceiveMessage
    {
        public new static string Command = "reply";
        public ReplyCommand() {}

        public ReplyCommand(string message) {
            Message = message;
        }

        public new string ToGameCommand() {
            return Command + " " + Message;
        }

        public void ParseInput(string substring) {
            Message = substring;
        }
    }
}