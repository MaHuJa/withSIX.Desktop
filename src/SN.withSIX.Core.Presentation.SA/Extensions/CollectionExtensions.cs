// <copyright company="SIX Networks GmbH" file="CollectionExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;

namespace SN.withSIX.Core.Presentation.SA.Extensions
{
    public static class CollectionExtensions
    {
        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action) {
            var i = 0;
            foreach (var e in ie)
                action(e, i++);
        }
    }
}