// <copyright company="SIX Networks GmbH" file="MessageBoxDialogParams.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Core.Applications.Services
{
    public class MessageBoxDialogParams
    {
        public MessageBoxDialogParams(string message) {
            Contract.Requires<ArgumentNullException>(message != null);
            Message = message;
            IgnoreContent = true;
        }

        public MessageBoxDialogParams(string message, string title)
            : this(message) {
            Contract.Requires<ArgumentNullException>(title != null);
            Title = title;
        }

        public MessageBoxDialogParams(string message, string title, SixMessageBoxButton buttons)
            : this(message, title) {
            Buttons = buttons;
        }

        public string Message { get; private set; }
        public string Title { get; private set; }
        public SixMessageBoxButton Buttons { get; private set; }
        public string GreenContent { get; set; }
        public string BlueContent { get; set; }
        public string RedContent { get; set; }
        public bool? RememberedState { get; set; }
        public object Owner { get; set; }
        public bool IgnoreContent { get; set; }
    }
}