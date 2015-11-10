// <copyright company="SIX Networks GmbH" file="BusyStateHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public interface IBusyStateHandler
    {
        bool IsBusy { get; }
        bool IsAborted { get; set; }
        bool IsSuspended { get; }
        IDisposable StartSession();
        IDisposable StartSuspendedSession();
        event PropertyChangedEventHandler PropertyChanged;
        void Refresh();
    }

    public class BusyStateHandler : PropertyChangedBase, IEnableLogging, IDomainService, IBusyStateHandler
    {
        readonly object _busyLock = new Object();
        bool _isSuspended;
        public bool IsBusy
        {
            get
            {
                lock (_busyLock) {
                    return Common.AppCommon.IsBusy;
                }
            }
            private set
            {
                lock (_busyLock) {
                    if (Common.AppCommon.IsBusy == value)
                        return;
                    Common.AppCommon.IsBusy = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsAborted { get; set; }
        public bool IsSuspended
        {
            get { return _isSuspended; }
            private set { SetProperty(ref _isSuspended, value); }
        }

        public IDisposable StartSession() {
            if (!CheckAndSetBusy())
                throw new BusyException();
            return new BusySession(this);
        }

        public IDisposable StartSuspendedSession() {
            IsSuspended = true;
            return new SuspendedSession(this);
        }

        bool CheckAndSetBusy() {
            var value = CheckAndSetBusyInternal();
            return value;
        }

        bool CheckAndSetBusyInternal() {
            lock (_busyLock) {
                if (IsBusy)
                    return false;
                IsBusy = true;
                return true;
            }
        }

        [DoNotObfuscate]
        public class BusyException : UserException
        {
            public BusyException() : base("The application is in a busy state, please try again later") {}
        }

        class BusySession : IDisposable
        {
            readonly BusyStateHandler _handler;

            public BusySession(BusyStateHandler handler) {
                Contract.Requires<ArgumentNullException>(handler != null);
                _handler = handler;
            }

            public void Dispose() {
                _handler.IsBusy = false;
            }
        }

        class SuspendedSession : IDisposable
        {
            readonly BusyStateHandler _handler;

            public SuspendedSession(BusyStateHandler handler) {
                Contract.Requires<ArgumentNullException>(handler != null);
                _handler = handler;
            }

            public void Dispose() {
                _handler.IsSuspended = false;
            }
        }
    }
}