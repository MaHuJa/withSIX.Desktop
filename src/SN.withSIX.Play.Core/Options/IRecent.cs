// <copyright company="SIX Networks GmbH" file="IRecent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Options
{
    public interface IRecent : IObjectTag
    {
        DateTime? LastUsed { get; }
    }
}