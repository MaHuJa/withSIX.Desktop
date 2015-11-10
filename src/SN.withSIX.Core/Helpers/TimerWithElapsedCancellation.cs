// <copyright company="SIX Networks GmbH" file="TimerWithElapsedCancellation.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Timers;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core.Helpers
{
    public class TimerWithElapsedCancellation : Timer
    {
        volatile bool _disposed;
        protected Func<bool> OnElapsedFunc;

        public TimerWithElapsedCancellation(double time, Func<bool> onElapsed,
            Action<object, EventArgs> onDisposed = null,
            bool startImmediately = true)
            : base(time) {
            AutoReset = false;
            if (onDisposed != null)
                Disposed += (obj, args) => onDisposed(obj, args);
            OnElapsedFunc = onElapsed;
            Elapsed += OnElapsed;

            if (startImmediately)
                Start();
        }

        public TimerWithElapsedCancellation(TimeSpan time, Func<bool> onElapsed,
            Action<object, EventArgs> onDisposed = null, bool startImmediately = true)
            : this(time.TotalMilliseconds, onElapsed, onDisposed, startImmediately) {}

        void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (_disposed)
                return;
            Stop();

            var shouldContinue = TryShouldContinue();

            lock (this) {
                if (_disposed)
                    return;
                if (shouldContinue)
                    Start();
                else
                    Dispose();
            }
        }

        bool TryShouldContinue() {
            try {
                return OnElapsedFunc();
            } catch (Exception ex) {
                MainLog.Logger.WarnException("Unhandled exception in Timer!", ex);
                return false;
            }
        }

        protected override void Dispose(bool disposing) {
            lock (this) {
                if (_disposed)
                    return;
                _disposed = true;
            }

            if (disposing) {
                Elapsed -= OnElapsed;
                OnElapsedFunc = null;
            }

            base.Dispose(disposing);
        }
    }

    public class TimerWithElapsedCancellationOnExceptionOnly : Timer
    {
        volatile bool _disposed;
        protected Action OnElapsedFunc;

        public TimerWithElapsedCancellationOnExceptionOnly(double time, Action onElapsed,
            Action<object, EventArgs> onDisposed = null,
            bool startImmediately = true)
            : base(time) {
            AutoReset = false;
            if (onDisposed != null)
                Disposed += (obj, args) => onDisposed(obj, args);
            OnElapsedFunc = onElapsed;
            Elapsed += OnElapsed;

            if (startImmediately)
                Start();
        }

        public TimerWithElapsedCancellationOnExceptionOnly(TimeSpan time, Action onElapsed,
            Action<object, EventArgs> onDisposed = null, bool startImmediately = true)
            : this(time.TotalMilliseconds, onElapsed, onDisposed, startImmediately) {}

        void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (_disposed)
                return;
            Stop();

            var shouldContinue = TryShouldContinue();

            lock (this) {
                if (_disposed)
                    return;

                if (shouldContinue)
                    Start();
                else
                    Dispose();
            }
        }

        bool TryShouldContinue() {
            try {
                OnElapsedFunc();
                return true;
            } catch (Exception ex) {
                MainLog.Logger.WarnException("Unhandled exception in Timer!", ex);
                return false;
            }
        }

        protected override void Dispose(bool disposing) {
            lock (this) {
                if (_disposed)
                    return;
                _disposed = true;
            }

            if (disposing) {
                Elapsed -= OnElapsed;
                OnElapsedFunc = null;
            }

            base.Dispose(disposing);
        }
    }

    public class TimerWithoutOverlap : Timer
    {
        volatile bool _disposed;
        protected Action OnElapsedFunc;

        public TimerWithoutOverlap(double time, Action onElapsed,
            Action<object, EventArgs> onDisposed = null,
            bool startImmediately = true)
            : base(time) {
            AutoReset = false;
            if (onDisposed != null)
                Disposed += (obj, args) => onDisposed(obj, args);
            OnElapsedFunc = onElapsed;
            Elapsed += OnElapsed;

            if (startImmediately)
                Start();
        }

        public TimerWithoutOverlap(TimeSpan time, Action onElapsed,
            Action<object, EventArgs> onDisposed = null, bool startImmediately = true)
            : this(time.TotalMilliseconds, onElapsed, onDisposed, startImmediately) {}

        void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (_disposed)
                return;
            Stop();

            try {
                OnElapsedFunc();
            } finally {
                lock (this) {
                    if (!_disposed)
                        Start();
                }
            }
        }

        protected override void Dispose(bool disposing) {
            lock (this) {
                if (_disposed)
                    return;
                _disposed = true;
            }

            if (disposing) {
                Elapsed -= OnElapsed;
                OnElapsedFunc = null;
            }

            base.Dispose(disposing);
        }
    }


    public class TimerWithElapsedCancellationAsync : Timer
    {
        volatile bool _disposed;
        protected Func<Task<bool>> OnElapsedFunc;

        public TimerWithElapsedCancellationAsync(double time, Func<Task<bool>> onElapsed,
            Func<object, EventArgs, Task> onDisposed = null,
            bool startImmediately = true)
            : base(time) {
            AutoReset = false;
            if (onDisposed != null)
                Disposed += (obj, args) => onDisposed(obj, args).WaitAndUnwrapException();

            OnElapsedFunc = onElapsed;

            Elapsed += OnElapsed;

            if (startImmediately)
                Start();
        }

        async void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (_disposed)
                return;
            Stop();

            var shouldContinue = await TryShouldContinue().ConfigureAwait(false);

            lock (this) {
                if (_disposed)
                    return;
                if (shouldContinue)
                    Start();
                else
                    Dispose();
            }
        }

        async Task<bool> TryShouldContinue() {
            try {
                return await OnElapsedFunc().ConfigureAwait(false);
            } catch (Exception ex) {
                MainLog.Logger.WarnException("Unhandled exception in Timer!", ex);
                return false;
            }
        }

        protected override void Dispose(bool disposing) {
            lock (this) {
                if (_disposed)
                    return;
                _disposed = true;
            }
            if (disposing) {
                Elapsed -= OnElapsed;
                OnElapsedFunc = null;
            }

            base.Dispose(disposing);
        }
    }
}