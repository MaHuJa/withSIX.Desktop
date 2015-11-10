// <copyright company="SIX Networks GmbH" file="IHaveType.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface IHaveType<T>
    {
        T Type { get; set; }
    }
}