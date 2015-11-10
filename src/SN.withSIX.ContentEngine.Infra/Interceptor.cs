// <copyright company="SIX Networks GmbH" file="Interceptor.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace SN.withSIX.ContentEngine.Infra
{
    class Interceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation) {
            object instance = ActLikeExtensions.GetInstanceField(invocation.Proxy.GetType(), invocation.Proxy,
                "__target");

            var attr =
                (ActualTypeAttribute[])
                    invocation.GetConcreteMethod().GetCustomAttributes(typeof (ActualTypeAttribute), true);
            var arguments = new List<object>();

            for (int i = 0; i < invocation.Arguments.Length; i++) {
                var type = attr[0].Types[i];
                if (type == null || type == typeof (object))
                    arguments.Add(invocation.Arguments[i]);
                else {
                    var actLike = ActLikeExtensions.ActLike(invocation.Arguments[i], type);
                    arguments.Add(actLike);
                }
            }

            var memebers =
                instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var inv = memebers.First(x => x.Name == invocation.GetConcreteMethod().Name);

            invocation.ReturnValue = inv.Invoke(instance, arguments.ToArray());
        }
    }
}