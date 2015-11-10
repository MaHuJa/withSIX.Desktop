// <copyright company="SIX Networks GmbH" file="IComparePK.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface IComparePK<in T>
    {
        bool ComparePK(object other);
        bool ComparePK(T other);
    }
}