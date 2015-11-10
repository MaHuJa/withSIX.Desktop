// <copyright company="SIX Networks GmbH" file="ExceptionDialogViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Security;
using SmartAssembly.SmartExceptionsCore;

namespace SN.withSIX.Core.Presentation.SA.ViewModels
{
    [DoNotObfuscate]
    public class ExceptionDialogViewModel
    {
        public ExceptionDialogViewModel(string moreInfo = null) {
#if DEBUG
            IsThrowEnabled = true;
#endif
            if (moreInfo != null)
                MoreInfo = SecurityElement.Escape(moreInfo);
        }

        public string Message { get; set; }
        public string MoreInfo { get; private set; }
        public string Title { get; set; }
        public bool Throw { get; set; }
        public bool IsThrowEnabled { get; set; }
        public bool Cancel { get; set; }
        public Exception Exception { get; set; }
    }
}