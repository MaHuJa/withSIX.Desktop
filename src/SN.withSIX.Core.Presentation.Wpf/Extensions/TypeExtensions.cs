// <copyright company="SIX Networks GmbH" file="TypeExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetProperties(this Type type) {
            return type.GetProperties()
                .Where(p => p.GetGetMethod().IsPublic)
                .Where(p => p.GetSetMethod().IsPublic);
        }

        public static PropertyInfo GetProperty(this Type type, string name) {
            return type.GetProperties().FirstOrDefault(p => p.Name == name);
        }

        public static MethodInfo GetMethod(this Type type, string name) {
            return type.GetMethods()
                .Where(m => m.IsPublic)
                .FirstOrDefault(p => p.Name == name);
        }
    }
}