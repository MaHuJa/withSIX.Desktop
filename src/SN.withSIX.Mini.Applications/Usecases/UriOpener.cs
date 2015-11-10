// <copyright company="SIX Networks GmbH" file="UriOpener.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public class UriOpener
    {
        public static Task OpenUri(Uri baseUri, string path = null) {
            return Task.Run(() => Process.Start(path == null ? baseUri.ToString() : new Uri(baseUri, path).ToString()));
        }
    }

    class Urls
    {
        public static readonly Uri Play = new Uri("http://play.withsix.com");
        public static readonly Uri Connect = new Uri("https://connect.withsix.com");
        public static readonly Uri Main = new Uri("https://withsix.com");
    }
}