// <copyright company="SIX Networks GmbH" file="MissingAddonsMessage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy.Arma.Commands
{
    public class MissingAddonsMessage : MessageBase, IReceiveOnlyMessage
    {
        public static string Command = "Missing_addons";

        public void ParseInput(string substring) {
            Message = substring;
        }
    }
}