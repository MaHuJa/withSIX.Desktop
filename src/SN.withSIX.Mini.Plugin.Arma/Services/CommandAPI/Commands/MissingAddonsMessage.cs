// <copyright company="SIX Networks GmbH" file="MissingAddonsMessage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI.Commands
{
    public class MissingAddonsMessage : MessageBase, IReceiveOnlyMessage
    {
        public static string Command = "Missing_addons";

        public void ParseInput(string substring) {
            Message = substring;
        }
    }
}