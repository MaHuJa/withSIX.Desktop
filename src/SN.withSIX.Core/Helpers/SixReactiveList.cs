// <copyright company="SIX Networks GmbH" file="SixReactiveList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Disposables;
using ReactiveUI;

namespace SN.withSIX.Core.Helpers
{
    public class SixReactiveDisposableList<T> : ReactiveList<T>, IDisposable
    {
        CompositeDisposable _disposable;

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool b) {
            if (b) {
                if (_disposable != null)
                    _disposable.Dispose();
            }
        }

        public void SetDisposables(CompositeDisposable disposable) {
            _disposable = disposable;
        }
    }
}