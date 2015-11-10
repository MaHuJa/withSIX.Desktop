// <copyright company="SIX Networks GmbH" file="ReflectionExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace SN.withSIX.Core.Extensions
{
    public static class AttributeUtils
    {
        public static bool HasAttribute<TAttribute>(this MemberInfo member, bool inherit = true)
            where TAttribute : Attribute {
            return member.IsDefined(typeof (TAttribute), inherit);
        }

        public static TAttribute GetAttribute<TAttribute>(this MemberInfo member, bool inherit = true)
            where TAttribute : Attribute {
            return member.GetAttributes<TAttribute>(inherit).FirstOrDefault();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo member, bool inherit = true)
            where TAttribute : Attribute {
            return member.GetCustomAttributes(typeof (TAttribute), inherit).Cast<TAttribute>();
        }
    }

    public static class ReflectionExtensions
    {
        public static TAct Call<TAct>(this object obj, TAct act) {
            MethodInfo methodInfo = (new MethodOf<TAct>(act));
            return methodInfo.CreateDelegate<TAct>(obj);
        }

        public static TAct Call<TAct>(TAct act) {
            return Call(null, act);
        }

        public static TAct CallGeneric<TAct>(this object obj, TAct act, params Type[] types) {
            MethodInfo methodInfo = (new MethodOf<TAct>(act));
            var genMethod = methodInfo.GetGenericMethodDefinition();
            var constructedMethod = genMethod.MakeGenericMethod(types);
            return constructedMethod.CreateDelegate<TAct>(obj);
        }

        public static MethodInfo GetGeneric<TAct>(this object obj, TAct act, params Type[] types) {
            MethodInfo methodInfo = (new MethodOf<TAct>(act));
            var genMethod = methodInfo.GetGenericMethodDefinition();
            var constructedMethod = genMethod.MakeGenericMethod(types);
            return constructedMethod;
        }

        public static MethodInfo GetGeneric<TAct>(TAct act, params Type[] types) {
            return GetGeneric(null, act, types);
        }

        static TAct CreateDelegate<TAct>(this MethodInfo methodInfo, object instance = null) {
            Contract.Requires<ArgumentException>((!methodInfo.IsStatic && instance != null) || methodInfo.IsStatic);
            if (instance != null)
                return (TAct) (object) methodInfo.CreateDelegate(typeof (TAct), instance);
            return (TAct) (object) methodInfo.CreateDelegate(typeof (TAct));
        }

        public static TAct CallGeneric<TAct>(TAct act, params Type[] types) {
            return CallGeneric(null, act, types);
        }

        public static IEnumerable<Type> GetTypeParameters(this Type type, Type searchType) {
            // Query.
            return
                from interfaceType in type.GetInterfaces()
                where interfaceType.IsGenericType
                let baseInterface = interfaceType.GetGenericTypeDefinition()
                where baseInterface == searchType
                select interfaceType.GetGenericArguments().First();
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic) {
            while (toCheck != null && toCheck != typeof (object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                    return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }

    public class MethodOf<T>
    {
        public MethodOf(T func) {
            var del = func as Delegate;
            if (del == null)
                throw new ArgumentException("Cannot convert func to Delegate.", "func");

            Method = del.Method;
        }

        MethodInfo Method { get; }

        public static implicit operator MethodOf<T>(T func) {
            return new MethodOf<T>(func);
        }

        public static implicit operator MethodInfo(MethodOf<T> methodOf) {
            return methodOf.Method;
        }
    }
}