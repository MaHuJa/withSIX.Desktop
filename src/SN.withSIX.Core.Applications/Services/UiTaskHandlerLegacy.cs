// <copyright company="SIX Networks GmbH" file="UiTaskHandlerLegacy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Core.Applications.Services
{
    public static class UiTaskHandlerExtensions
    {
        public static async Task<UnitType> Void<T>(this Task<T> task) {
            await task.ConfigureAwait(false);
            return UnitType.Default;
        }

        public static async Task<UnitType> Void(this Task task) {
            await task.ConfigureAwait(false);
            return UnitType.Default;
        }

        [Obsolete("Use RXUI or custom factory methods instead")]
        public static ReactiveCommand<TOut> SetNewCommand<T, TOut>(this ReactiveCommand<TOut> command, T target,
            Expression<Func<T, ReactiveCommand<TOut>>> memberLamda)
            where T : class {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(memberLamda != null);

            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
                throw new Exception("Not a member expression? " + target);
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
                throw new Exception("Not a property? " + target);
            command.DefaultSetup(target.GetCallerName(property));
            property.SetValue(target, command, null);
            return command;
        }

        [Obsolete("Use RXUI or custom factory methods instead")]
        public static ReactiveCommand SetCommand<T>(this T target, Expression<Func<T, ReactiveCommand>> memberLamda)
            where T : class {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(memberLamda != null);

            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
                throw new Exception("Not a member expression? " + target);
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
                throw new Exception("Not a property? " + target);
            var reactiveCommand = CreateCommand(GetCallerName(target, property));
            property.SetValue(target, reactiveCommand, null);
            return reactiveCommand;
        }

        [Obsolete("Use RXUI or custom factory methods instead")]
        public static ReactiveCommand SetCommand<T>(this T target, Expression<Func<T, ReactiveCommand>> memberLamda,
            IObservable<bool> ena,
            bool initialCondition = true) where T : class {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(memberLamda != null);
            Contract.Requires<ArgumentNullException>(ena != null);

            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
                throw new Exception("Not a member expression? " + target);
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
                throw new Exception("Not a property? " + target);
            var reactiveCommand = CreateCommand(ena, GetCallerName(target, property), initialCondition);
            property.SetValue(target, reactiveCommand, null);
            return reactiveCommand;
        }

        static string GetCallerName<T>(this T target, PropertyInfo property) {
            return target.GetType().Name + "." + property.Name;
        }

        public static ReactiveCommand ToCommand(this Action action, [CallerMemberName] string name = null) {
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentNullException>(name != null);
            var command = CreateCommand(name);
            command.Subscribe(action);
            return command;
        }

        public static ReactiveCommand ToAsyncCommand(this Func<Task> action, string name) {
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentNullException>(name != null);
            var command = CreateCommand(name);
            command.RegisterAsyncTask(action).Subscribe();
            return command;
        }

        static ReactiveCommand CreateCommand(string name = null) {
            var command = new ReactiveCommand();
            command.DefaultSetup(name);
            return command;
        }

        static ReactiveCommand CreateCommand(IObservable<bool> ena, string name, bool initialCondition = true) {
            Contract.Requires<ArgumentNullException>(ena != null);
            Contract.Requires<ArgumentNullException>(name != null);

            var command = new ReactiveCommand(ena, initialCondition);
            command.DefaultSetup(name);
            return command;
        }

        public static ReactiveCommand CreateCommand(string name, bool allowMultiple, bool initialCondition = true) {
            Contract.Requires<ArgumentNullException>(name != null);

            var command = new ReactiveCommand(null, allowMultiple, null, initialCondition);
            command.DefaultSetup(name);
            return command;
        }
    }
}