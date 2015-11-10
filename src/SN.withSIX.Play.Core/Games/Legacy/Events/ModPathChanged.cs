// <copyright company="SIX Networks GmbH" file="ModPathChanged.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NDepend.Path;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class ModPathChanged : EventArgs
    {
        public readonly IAbsoluteDirectoryPath OldPath;
        public readonly IAbsoluteDirectoryPath Path;

        public ModPathChanged(IAbsoluteDirectoryPath value, IAbsoluteDirectoryPath oldValue) {
            Path = value;
            OldPath = oldValue;
        }
    }

    public class SynqPathChanged : EventArgs
    {
        public readonly IAbsoluteDirectoryPath OldPath;
        public readonly IAbsoluteDirectoryPath Path;

        public SynqPathChanged(IAbsoluteDirectoryPath value, IAbsoluteDirectoryPath oldValue) {
            Path = value;
            OldPath = oldValue;
        }
    }
}