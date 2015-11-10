// <copyright company="SIX Networks GmbH" file="UserErrorViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using ReactiveUI;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs
{
    public class UserErrorViewModel : RxViewModelBase
    {
        public UserErrorViewModel(UserError userError) {
            UserError = userError;

            this.WhenActivated(d => {
                foreach (var a in userError.RecoveryOptions.OfType<RecoveryCommand>().Where(x => !(x is IDontRecover)))
                    d(a.Subscribe(x => TryClose()));
            });
        }

        public UserError UserError { get; }
    }
}