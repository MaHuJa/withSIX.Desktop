// <copyright company="SIX Networks GmbH" file="IContentEngineGameContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.ContentEngine.Core;

namespace SN.withSIX.ContentEngine.Infra.Services
{
    public interface IContentEngineGameContext
    {
        IContentEngineGame Get(Guid gameId);
    }
}