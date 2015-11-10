// <copyright company="SIX Networks GmbH" file="IContentEngineGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;

namespace SN.withSIX.ContentEngine.Core
{
    public interface IContentEngineGame
    {
        IAbsoluteDirectoryPath WorkingDirectory { get; }
    }
}