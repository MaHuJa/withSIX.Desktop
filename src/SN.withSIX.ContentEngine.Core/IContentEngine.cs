// <copyright company="SIX Networks GmbH" file="IContentEngine.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.ContentEngine.Core
{
    public interface IContentEngine
    {
        bool ModHasScript(Guid guid);
        IModS LoadModS(IContentEngineContent mod, bool overrideMod = false);
        bool ModHasScript(IContentEngineContent mod);
    }
}