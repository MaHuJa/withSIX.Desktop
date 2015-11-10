// <copyright company="SIX Networks GmbH" file="IHaveTimestamp.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core
{
    public interface IHaveTimestamp
    {
        DateTime Timestamp { get; }
    }
}