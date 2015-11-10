// <copyright company="SIX Networks GmbH" file="GameTerminated.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using SN.withSIX.Play.Core.Connect.Events;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class GameTerminated : TimeStampedEvent
    {
        public readonly Process Process;

        public GameTerminated(Process process) {
            Contract.Requires<ArgumentNullException>(process != null);

            Process = process;
        }
    }
}