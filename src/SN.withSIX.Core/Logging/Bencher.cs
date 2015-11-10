// <copyright company="SIX Networks GmbH" file="Bencher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SN.withSIX.Core.Logging
{
    public static class BenchExtension
    {
        public static Bencher Bench(this object obj, string message = null, string startMessage = null,
            [CallerMemberName] string caller = null) {
            return new Bencher(message, obj.GetType().Name + "." + caller, startMessage);
        }
    }

    public class Bencher : IDisposable
    {
        readonly string _caller;
        readonly string _message;
        readonly Stopwatch _stopWatch;

        public Bencher(string message, string caller, string startInfo = null) {
            _message = message;
            _caller = caller;
            _stopWatch = Stopwatch.StartNew();
            MainLog.BenchLog($"START: [{_caller}] {_message}");
            if (startInfo != null)
                MainLog.BenchLog(startInfo);
        }

        public object AdditionalInfo { get; set; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            _stopWatch.Stop();
            MainLog.BenchLog(disposing
                ? $"END: {_stopWatch.Elapsed.ToString("mm':'ss':'fff")} [{_caller}] {_message} ({AdditionalInfo})"
                : $"END (WARN FINALIZED): {_stopWatch.Elapsed.ToString("mm':'ss':'fff")} [{_caller}] {_message} ({AdditionalInfo})");
        }

        ~Bencher() {
            Dispose(false);
        }
    }
}