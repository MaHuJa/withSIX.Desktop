// <copyright company="SIX Networks GmbH" file="CastleExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Castle.DynamicProxy;

namespace SN.withSIX.ContentEngine.Infra
{
    static class CastleExtensions
    {
        internal static T Proxify<T, T2>(this T instance) {
            return (T) new ProxyGenerator().CreateClassProxyWithTarget(typeof (T), new[] {typeof (T2)}, instance,
                new Interceptor());
        }

        internal static T Proxify<T, T2>(this T instance, params object[] constructorArguments) {
            return
                (T) new ProxyGenerator().CreateClassProxyWithTarget(typeof (T), new[] {typeof (T2)}, instance,
                    ProxyGenerationOptions.Default, constructorArguments,
                    new Interceptor());
        }
    }
}