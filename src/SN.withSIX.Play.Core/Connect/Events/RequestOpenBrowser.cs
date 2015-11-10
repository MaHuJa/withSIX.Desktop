// <copyright company="SIX Networks GmbH" file="RequestOpenBrowser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public class RequestOpenBrowser : EventArgs
    {
        public RequestOpenBrowser(Uri url) {
            Contract.Requires<ArgumentNullException>(url != null);

            Url = url;
        }

        public RequestOpenBrowser(string url) {
            Contract.Requires<ArgumentNullException>(url != null);

            Url = new Uri(url);
        }

        public Uri Url { get; set; }
    }


    public class RequestOpenLogin {}
}