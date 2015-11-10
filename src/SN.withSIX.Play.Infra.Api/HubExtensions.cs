// <copyright company="SIX Networks GmbH" file="HubExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.AspNet.SignalR.Client;
using SignalRNetClientProxyMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Extensions;

namespace SN.withSIX.Play.Infra.Api
{
    [DoNotObfuscateType]
    public static class HubExtensions
    {
        const string ProblemOccurred = "A problem occurred while trying to observe the task";
        static readonly ProxyGenerator generator = new ProxyGenerator(true);
        // TODO: If we strongly sign withSIX.Api.Models, we should be able to turn this off again.
        static readonly HubExceptionInterceptor hubExceptionInterceptor = new HubExceptionInterceptor();
        static readonly Func<AggregateException, Exception> defaultExceptionTransformer =
            ExceptionHelpers.GetFirstException;

        public static THub CreateStrongHubProxyWithExceptionUnwrapping<THub>(this HubConnection connection)
            where THub : class, IClientHubProxyBase {
            var hub =
                ReflectionExtensions.Call<Func<HubConnection, bool, THub>>(
                    ClientHubProxyExtensions.CreateStrongHubProxy<THub>)(connection, true);
            return (THub) generator.CreateInterfaceProxyWithTarget(typeof (THub), hub, hubExceptionInterceptor);
        }

        [DoNotObfuscate]
        public static Task<TResult> SetContinuation<TResult>(this Task<TResult> task,
            Func<AggregateException, Exception> exceptionTransformer, Action final) {
            Contract.Requires<ArgumentNullException>(task != null);
            Contract.Requires<ArgumentNullException>(exceptionTransformer != null);
            Contract.Requires<ArgumentNullException>(final != null);

            var tcs = new TaskCompletionSource<TResult>();
            task.ContinueWith(t => {
                try {
                    HandleContinuation(exceptionTransformer, t, tcs);
                } finally {
                    final();
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        [DoNotObfuscate]
        public static Task<TResult> SetContinuation<TResult>(this Task<TResult> task,
            Func<AggregateException, Exception> exceptionTransformer) {
            Contract.Requires<ArgumentNullException>(task != null);
            Contract.Requires<ArgumentNullException>(exceptionTransformer != null);

            var tcs = new TaskCompletionSource<TResult>();
            task.ContinueWith(t => HandleContinuation(exceptionTransformer, t, tcs),
                TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        static void HandleContinuation<TResult>(Func<AggregateException, Exception> exceptionTransformer,
            Task<TResult> t, TaskCompletionSource<TResult> tcs) {
            try {
                if (t.Exception != null)
                    tcs.SetException(exceptionTransformer(t.Exception));
                else
                    tcs.SetResult(t.Result);
            } catch (Exception ex) {
                MainLog.Logger.FormattedErrorException(ex, ProblemOccurred);
                tcs.TrySetException(new Exception(ProblemOccurred));
                throw;
            }
        }

        [DoNotObfuscate]
        public static Task<TResult> SetContinuation<TResult>(this Task<TResult> task, Action final) {
            Contract.Requires<ArgumentNullException>(task != null);
            Contract.Requires<ArgumentNullException>(final != null);
            return SetContinuation(task, defaultExceptionTransformer, final);
        }

        [DoNotObfuscate]
        public static Task SetContinuation(this Task task, Func<AggregateException, Exception> exceptionTransformer,
            Action final) {
            Contract.Requires<ArgumentNullException>(task != null);
            Contract.Requires<ArgumentNullException>(exceptionTransformer != null);
            Contract.Requires<ArgumentNullException>(final != null);

            var tcs = new TaskCompletionSource<Unit>();
            task.ContinueWith(t => {
                try {
                    HandleContinuation(exceptionTransformer, t, tcs);
                } finally {
                    final();
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        [DoNotObfuscate]
        public static Task SetContinuation(this Task task, Func<AggregateException, Exception> exceptionTransformer) {
            Contract.Requires<ArgumentNullException>(task != null);
            Contract.Requires<ArgumentNullException>(exceptionTransformer != null);

            var tcs = new TaskCompletionSource<Unit>();
            task.ContinueWith(t => HandleContinuation(exceptionTransformer, t, tcs),
                TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        static void HandleContinuation(Func<AggregateException, Exception> exceptionTransformer, Task t,
            TaskCompletionSource<Unit> tcs) {
            try {
                if (t.Exception != null)
                    tcs.SetException(exceptionTransformer(t.Exception));
                else
                    tcs.SetResult(Unit.Default);
            } catch (Exception ex) {
                MainLog.Logger.FormattedErrorException(ex, ProblemOccurred);
                tcs.TrySetException(new Exception(ProblemOccurred));
                throw;
            }
        }

        [DoNotObfuscate]
        public static Task SetContinuation(this Task task, Action final) {
            Contract.Requires<ArgumentNullException>(task != null);
            Contract.Requires<ArgumentNullException>(final != null);
            return SetContinuation(task, defaultExceptionTransformer, final);
        }

        class HubExceptionInterceptor : IInterceptor
        {
            static readonly Func<AggregateException, Exception> exceptionTransformer = GetException;

            public void Intercept(IInvocation invocation) {
                invocation.Proceed();
                var returnType = invocation.Method.ReturnType;
                if (!typeof (Task).IsAssignableFrom(returnType))
                    return;

                // Using dynamic here because we need to handle both generic and non generic methods
                invocation.ReturnValue = HubExtensions.SetContinuation((dynamic) invocation.ReturnValue,
                    exceptionTransformer);
            }

            static Exception GetException(AggregateException exception) {
                var ex = exception.GetFirstException();
                var hubEx = ex as HubException;
                return hubEx != null ? hubEx.GetException() : ex;
            }
        }
    }
}