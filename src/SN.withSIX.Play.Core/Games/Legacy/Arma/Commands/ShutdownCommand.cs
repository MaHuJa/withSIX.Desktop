// <copyright company="SIX Networks GmbH" file="ShutdownCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy.Arma.Commands
{
    public class ShutdownCommand : ISendMessage
    {
        public static string Command = "shutdown";

        public string ToGameCommand() {
            return Command;
        }
    }
}