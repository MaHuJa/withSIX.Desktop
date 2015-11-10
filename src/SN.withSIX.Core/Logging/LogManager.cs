// <copyright company="SIX Networks GmbH" file="LogManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLog;

namespace SN.withSIX.Core.Logging
{
    public class DefaultLogManager : ILogManager
    {
        const string SixMerged = "Six.Merged";
        public static LogFactory Factory = new LogFactory(LogManager.Configuration);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual ILogger GetLogger(string type) {
            return new LoggerBase(type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual ILogger GetCurrentClassLogger() {
            var frame = new StackFrame(1, false);
            return new LoggerBase(LogManager.GetLogger(frame.GetMethod().DeclaringType.FullName));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual ILogger GetCurrentClassLogger(Type loggerType) {
            var frame = new StackFrame(1, false);
            return new LoggerBase(Factory.GetLogger(frame.GetMethod().DeclaringType.FullName, loggerType));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual ILogger GetCurrentClassLoggerOrMerged() {
            var frame = new StackFrame(1, false);
            return
                new LoggerBase(
                    Factory.GetLogger(Common.Flags.Merged ? SixMerged : frame.GetMethod().DeclaringType.FullName));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual ILogger GetCurrentClassLoggerOrMerged(Type loggerType) {
            var frame = new StackFrame(1, false);
            return
                new LoggerBase(Factory.GetLogger(
                    Common.Flags.Merged ? SixMerged : frame.GetMethod().DeclaringType.FullName, loggerType));
        }

        public virtual ILogger GetLoggerOrMerged(Type type) {
            return
                new LoggerBase(Factory.GetLogger(Common.Flags.Merged ? SixMerged : type.FullName));
        }

        public virtual ILogger GetLoggerOrMerged(Type type, Type loggerType) {
            return
                new LoggerBase(Factory.GetLogger(Common.Flags.Merged ? SixMerged : type.FullName, loggerType));
        }
    }
}