// <copyright company="SIX Networks GmbH" file="HttpClientExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NDepend.Path;

namespace SN.withSIX.Sync.Core.Transfer
{
    public static class HttpClientExtensions
    {
        public static void SetAuthInfo(this HttpClient httpClient, string userInfo) {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", userInfo);
        }

        public static void SetAuthInfo(this HttpClient httpClient, Uri uri) {
            httpClient.SetAuthInfo(uri.UserInfo);
        }

        public static async Task DownloadAsync(this HttpClient httpClient, Uri requestUri, IAbsoluteFilePath filename) {
            if (filename == null)
                throw new ArgumentNullException("filename");

            /*            if (Proxy != null)
            {
                WebRequest.DefaultWebProxy = Proxy;
            }*/

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (
                var contentStream =
                    await
                        (await httpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync()
                            .ConfigureAwait(false))
            using (
                Stream stream = new FileStream(filename.ToString(), FileMode.Create, FileAccess.Write,
                    FileShare.None, 4096,
                    true))
                await contentStream.CopyToAsync(stream).ConfigureAwait(false);
        }
    }
}