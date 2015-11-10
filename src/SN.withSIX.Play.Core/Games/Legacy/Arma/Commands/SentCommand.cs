// <copyright company="SIX Networks GmbH" file="SentCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma.Commands
{
    public class SentCommand
    {
        public SentCommand(TaskCompletionSource<IMessage> tcs, IMessage originalCommand) {
            Tcs = tcs;
            OriginalCommand = originalCommand;
        }

        public TaskCompletionSource<IMessage> Tcs { get; set; }
        public IMessage OriginalCommand { get; set; }
    }
}