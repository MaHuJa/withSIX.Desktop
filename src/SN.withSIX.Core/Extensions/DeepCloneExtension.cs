// <copyright company="SIX Networks GmbH" file="DeepCloneExtension.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SN.withSIX.Core.Extensions
{
    public static class DeepCloneExtension
    {
        public static T DeepClone<T>(T source) where T : class {
            Contract.Requires<ArgumentNullException>(source != null);

            using (var ms = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, source);
                ms.Position = 0;
                return (T) formatter.Deserialize(ms);
            }
        }
    }
}