// <copyright company="SIX Networks GmbH" file="AutoMapperExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using AutoMapper;

namespace SN.withSIX.Mini.Applications.Extensions
{
    public static class MappingExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreAllMembers<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expression
            ) {
            expression.ForAllMembers(opt => opt.Ignore());
            return expression;
        }

        public static async Task<object> ToObject<T>(this Task<T> task) {
            Contract.Requires<ArgumentNullException>(task != null);
            return await task.ConfigureAwait(false);
        }

        public static async Task<TDesired> MapAsync<TSource, TDesired>(this Task<TSource> task, TDesired target)
            where TDesired : class {
            Contract.Requires<ArgumentNullException>(task != null);
            var r = await task.ConfigureAwait(false);
            return r.MapTo(target);
        }

        public static async Task DynamicMapAsync<TSource, TDesired>(this Task<TSource> task, TDesired target)
            where TDesired : class {
            Contract.Requires<ArgumentNullException>(task != null);
            var r = await task.ConfigureAwait(false);
            r.DynamicMap(target);
        }

        public static async Task<TDesired> MapAsync<TDesired>(this Task<object> task) {
            Contract.Requires<ArgumentNullException>(task != null);
            var r = await task.ConfigureAwait(false);
            return r.MapTo<TDesired>();
        }

        public static async Task<TDesired> DynamicMapAsync<TDesired>(this Task<object> task) {
            Contract.Requires<ArgumentNullException>(task != null);
            var r = await task.ConfigureAwait(false);
            return r.DynamicMap<TDesired>();
        }

        public static TDesired MapTo<TDesired>(this object input) {
            return Mapper.Map<TDesired>(input);
        }

        public static TDesired MapTo<TSource, TDesired>(this TSource input, TDesired output) where TDesired : class {
            Contract.Requires<ArgumentNullException>(output != null);
            return Mapper.Map(input, output);
        }

        public static TDesired DynamicMap<TSource, TDesired>(this TSource input) {
            return Mapper.DynamicMap<TSource, TDesired>(input);
        }

        public static void DynamicMap<TSource, TDesired>(this TSource input, TDesired target) where TDesired : class {
            Contract.Requires<ArgumentNullException>(target != null);
            Mapper.DynamicMap(input, target);
        }

        public static TDesired DynamicMap<TDesired>(this object input) {
            return Mapper.DynamicMap<TDesired>(input);
        }

        public static void DynamicMap(this object input, object target) {
            Contract.Requires<ArgumentNullException>(target != null);
            Mapper.DynamicMap(input, target);
        }
    }
}