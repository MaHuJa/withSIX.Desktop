// <copyright company="SIX Networks GmbH" file="ContentRestApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Infra.Api.ContentApi
{
    // TODO: Replace RestSharp with HttpClient
    class ContentRestApi : RestBase
    {
        public static readonly JsonSerializerSettings JsonSettings = SerializationExtension.DefaultSettings;

        //public Task<T> GetJson<T>(string path) {
        //    return Tools.Transfer.GetJson<T>(new Uri(GetApiUrl(), path));
        //}

        public async Task<Tuple<T, string>> GetDataAsync<T>(string path, IDictionary<string, object> data = null) {
            var content =
                (await
                    RestExecuteAsync(CreateGetRequestWithParameters(path, data), GetApiUrl())
                        .ConfigureAwait(false))
                    .Content;
            return
                Tuple.Create(Deserialize<T>(content, JsonSettings), content);
        }

        static Uri GetApiUrl() {
            return Tools.Transfer.JoinUri(CommonUrls.ApiCdnUrl, "api", "v" + CommonUrls.ContentApiVersion);
        }

        protected override IRestClient GetClient(Uri url) {
            return new RestClient(url.ToString());
        }
    }
}