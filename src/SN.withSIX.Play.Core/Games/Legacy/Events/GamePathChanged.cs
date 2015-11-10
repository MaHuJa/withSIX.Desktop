// <copyright company="SIX Networks GmbH" file="GamePathChanged.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class GamePathChanged
    {
        public readonly IAbsoluteDirectoryPath Path;

        public GamePathChanged(IAbsoluteDirectoryPath value) {
            Path = value;
        }
    }
}