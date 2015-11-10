// <copyright company="SIX Networks GmbH" file="IHaveItems.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;

namespace SN.withSIX.Core.Helpers
{
    public interface IHaveItems<TItems, out TCollection> where TCollection : IList<TItems>
    {
        TCollection Items { get; }
    }
}