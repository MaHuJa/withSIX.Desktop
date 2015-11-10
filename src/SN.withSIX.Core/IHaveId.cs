// <copyright company="SIX Networks GmbH" file="IHaveId.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core
{
    public interface IHaveId<out T>
    {
        T Id { get; }
    }
}