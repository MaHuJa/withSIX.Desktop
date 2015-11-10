// <copyright company="SIX Networks GmbH" file="WebClient.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Net;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class WebClient : System.Net.WebClient, IWebClient
    {
        public WebClient() : this(100*1000) {}

        public WebClient(int timeout) {
            Timeout = timeout;
        }

        public int Timeout { get; set; }

        public void SetAuthInfo(string userInfo) {
            var splitUserInfo = userInfo.Split(':');
            if (splitUserInfo.Length < 2)
                return;
            Credentials = new NetworkCredential(splitUserInfo[0], splitUserInfo[1]);
        }

        public void SetAuthInfo(Uri uri) {
            SetAuthInfo(uri.UserInfo);
        }

        protected override WebRequest GetWebRequest(Uri address) {
            var request = base.GetWebRequest(address);
            SetHttpKeepAlive(request);
            SetSynchronousTimeout(request);
            return request;
        }

        static void SetHttpKeepAlive(WebRequest request) {
            var httpRequest = request as HttpWebRequest;
            if (httpRequest != null)
                httpRequest.KeepAlive = false;
        }

        void SetSynchronousTimeout(WebRequest request) {
            if (request != null)
                request.Timeout = Timeout;
        }
    }
}