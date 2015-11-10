// <copyright company="SIX Networks GmbH" file="IHaveGameId.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Mini.Core.Games
{
    public interface IHaveGameId
    {
        Guid GameId { get; }
    }
}