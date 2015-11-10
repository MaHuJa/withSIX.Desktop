// <copyright company="SIX Networks GmbH" file="BrowserHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Connect.Events;

namespace SN.withSIX.Play.Core.Connect
{
    public static class BrowserHelper
    {
        public static bool TryOpenUrlIntegrated(string url) {
            if (String.IsNullOrWhiteSpace(url))
                return false;

            if (DomainEvilGlobal.Settings.AppOptions.PreferSystemBrowser)
                return Tools.Generic.TryOpenUrl(url);
            Common.App.PublishEvent(new RequestOpenBrowser(new Uri(url)));
            return true;
        }

        public static bool TryOpenUrlIntegrated(Uri uri) {
            if (String.IsNullOrWhiteSpace(uri.ToString()))
                return false;

            if (DomainEvilGlobal.Settings.AppOptions.PreferSystemBrowser)
                return Tools.Generic.TryOpenUrl(uri);
            Common.App.PublishEvent(new RequestOpenBrowser(uri));
            return true;
        }
    }
}