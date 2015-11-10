// <copyright company="SIX Networks GmbH" file="Transfer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SN.withSIX.Core.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SN.withSIX.Core
{
    public static partial class Tools
    {
        public static TransferTools Transfer = new TransferTools();

        #region Nested type: Transfer

        public class TransferTools : IEnableLogging
        {
            static readonly char[] qsSplit = {'?', '&'};
            static readonly char[] splitQsParam = {'='};
            static readonly string[] encodedSchemes = {"http", "https", "zsync", "zsyncs", "ftp"};

            public virtual Dictionary<string, string> GetDictionaryFromQueryString(string qs) {
                Contract.Requires<ArgumentNullException>(qs != null);

                var parts = qs.Split(qsSplit);
                var properties = parts.Skip(1);
                return properties.Select(p => p.Split(splitQsParam, 2))
                    .ToDictionary(ps => ps[0], ps => Uri.UnescapeDataString(ps[1]));
            }

            public string EncodePathIfRequired(Uri uri, string path) {
                return EncodingRequired(uri) ? UrlEncodeRemoteFilePath(path) : path;
            }

            static string UrlEncodeRemoteFilePath(string path) {
                return !path.Contains("/")
                    ? Encode(path)
                    : string.Join("/", path.Split('/').Select(Encode));
            }

            /// <summary>
            ///     # is being ignored when we rebuild urls so we replace it with %23
            ///     sadly this does not help for rsync as we don't encode paths for rsync...
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            static string Encode(string path) {
                return HttpUtility.UrlPathEncode(path.Replace("#", "%23"));
            }

            static bool EncodingRequired(Uri uri) {
                return encodedSchemes.Contains(uri.Scheme);
            }

            // TODO: Missing serializersettings?
            public Task<T> GetJson<T>(Uri uri, string token = null) {
                return DownloaderExtensions.GetJson<T>(uri, token);
            }

            // TODO: Missing serializersettings?
            public Task<HttpResponseMessage> PostJson(object model, Uri uri, string token = null) {
                return DownloaderExtensions.PostJson(model, uri, token);
            }

            public Task<T> GetYaml<T>(Uri uri, string token = null) {
                return DownloaderExtensions.GetYaml<T>(uri, token);
            }

            public Task<HttpResponseMessage> PostYaml(object model, Uri uri, string token = null) {
                return DownloaderExtensions.PostYaml(model, uri, token);
            }

            public Uri JoinUri(Uri host, params object[] remotePaths) {
                Contract.Requires<ArgumentNullException>(host != null);
                Contract.Requires<ArgumentNullException>(remotePaths != null);
                Contract.Requires<ArgumentNullException>(remotePaths.Any());

                var remotePath = JoinPaths(remotePaths);
                if (!host.ToString().EndsWith("/"))
                    host = new Uri(host + "/");
                if (remotePath.StartsWith("/"))
                    remotePath = remotePath.Substring(1);
                return new Uri(host, remotePath);
            }

            public string JoinPaths(params object[] parts) {
                return string.Join("/", parts.Select(x => x == null ? null : x.ToString().TrimStart('/').TrimEnd('/')));
            }
        }

        #endregion
    }

    public static class DownloaderExtensions
    {
        static readonly DataAnnotationsValidator.DataAnnotationsValidator _validator =
            new DataAnnotationsValidator.DataAnnotationsValidator();

        public static async Task<T> GetJson<T>(Uri uri, string token = null) {
            using (var client = GetHttpClient()) {
                client.Setup(uri, token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var r = await client.GetStringAsync(uri).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(r);
            }
        }

        // TODO: Missing serializersettings?
        public static async Task<HttpResponseMessage> PostJson(object model, Uri uri, string token = null) {
            _validator.ValidateObject(model);
            using (var client = new HttpClient()) {
                client.Setup(uri, token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                        "application/json"))
                    return await client.PostAsync(uri, content).ConfigureAwait(false);
            }
        }

        public static async Task<T> GetYaml<T>(Uri uri, string token = null) {
            using (var client = GetHttpClient()) {
                client.Setup(uri, token);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/yaml"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept",
                    "text/html,application/xhtml+xml,application/xml,text/yaml,text/x-yaml,application/yaml,application/x-yaml");

                var r = await client.GetStringAsync(uri).ConfigureAwait(false);
                return
                    new Deserializer(ignoreUnmatched: true).Deserialize<T>(
                        new StringReader(r));
            }
        }

        public static async Task<HttpResponseMessage> PostYaml(object model, Uri uri, string token = null) {
            _validator.ValidateObject(model);
            using (var client = new HttpClient()) {
                client.Setup(uri, token);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/yaml"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept",
                    "text/html,application/xhtml+xml,application/xml,text/yaml,text/x-yaml,application/yaml,application/x-yaml");
                using (
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                        "text/yaml"))
                    return await client.PostAsync(uri, content).ConfigureAwait(false);
            }
        }

        static void Setup(this HttpClient client, Uri uri, string token) {
            HandleUserInfo(client, uri.UserInfo);
            if (token != null && CommonUrls.IsWithSixUrl(uri)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        static void HandleUserInfo(HttpClient client, string userInfo) {
            if (string.IsNullOrWhiteSpace(userInfo))
                return;
            var byteArray = Encoding.ASCII.GetBytes(userInfo);
            var authorizationString = Convert.ToBase64String(byteArray);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationString);
        }

        static HttpClient GetHttpClient() {
            var client = new HttpClient(
                new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip
                                             | DecompressionMethods.Deflate
                });
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            return client;
        }

        public sealed class CustomCamelCaseNamingConvention : INamingConvention
        {
            readonly CamelCaseNamingConvention convention = new CamelCaseNamingConvention();

            public string Apply(string value) {
                var s = value == null || !value.StartsWith(":") ? value : value.Substring(1);
                return convention.Apply(s);
            }
        }
    }
}