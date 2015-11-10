// <copyright company="SIX Networks GmbH" file="ActLikeExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reflection;
using ImpromptuInterface;

namespace SN.withSIX.ContentEngine.Infra
{
    static class ActLikeExtensions
    {
        internal static object ActLike(object o, Type type) {
            var mi =
                typeof (ActLikeExtensions).GetMethod("ActLikeInner",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type);
            return mi.Invoke(null, new[] {o});
        }

        internal static TActLike ActLikeInner<TActLike>(object o) where TActLike : class {
            return o.ActLike<TActLike>();
        }

        /// <summary>
        ///     Uses reflection to get the field value from an object.
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <returns>The field value from the object.</returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }
}