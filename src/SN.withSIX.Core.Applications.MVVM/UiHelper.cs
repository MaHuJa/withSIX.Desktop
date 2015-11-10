// <copyright company="SIX Networks GmbH" file="UiHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Caliburn.Micro;
using Action = System.Action;

namespace SN.withSIX.Core.Applications.MVVM
{
    public static class UiHelper
    {
        public static bool TryOnUiThread(Action action) {
            try {
                action.OnUIThread();
                return true;
            } catch (AggregateException e) {
                var rethrow = true;

                e.Handle(x => {
                    if (x is OperationCanceledException)
                        rethrow = false;

                    return true;
                });

                if (rethrow)
                    throw;
                return false;
            }
        }
    }
}