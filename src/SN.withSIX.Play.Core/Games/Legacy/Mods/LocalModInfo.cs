// <copyright company="SIX Networks GmbH" file="LocalModInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public abstract class LocalModInfo
    {
        public abstract string Name { get; protected set; }
        public abstract IAbsoluteDirectoryPath Path { get; protected set; }
    }
}