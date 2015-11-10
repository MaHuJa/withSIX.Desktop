// <copyright company="SIX Networks GmbH" file="RestBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json;
using RestSharp;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Play.Infra.Api
{
    // TODO: Replace RestSharp with HttpClient
    abstract class RestBase : IEnableLogging
    {
        protected abstract IRestClient GetClient(Uri url);

        protected void HandleRequest(IRestRequest request, IRestClient client) {
#if DEBUG
            LogRequestInfo(request, client);
#endif
        }

        protected void HandleResponse<T>(IRestRequest request, IRestResponse<T> response,
            IRestClient client)
            where T : new() {
#if RESPONSE_LOGGING
            LogResponseInfo(response, client);
#endif
            HandleResponseException(response);
        }

        protected void HandleResponse(IRestRequest request, IRestResponse response,
            IRestClient client) {
#if RESPONSE_LOGGING
            LogResponseInfo(response, client);
#endif
            HandleResponseException(response);
        }

        protected static T Deserialize<T>(string data) {
            return JsonConvert.DeserializeObject<T>(data);
        }

        protected static T Deserialize<T>(string data, JsonSerializerSettings settings) {
            return JsonConvert.DeserializeObject<T>(data, settings);
        }

        #region RestExecute

        public IRestResponse<T> RestExecute<T>(IRestRequest request, Uri url) where T : new() {
            Contract.Requires<ArgumentNullException>(request != null);

            var client = GetClient(url);
            HandleRequest(request, client);
            var response = client.Execute<T>(request);
            HandleResponse(request, response, client);

            return response;
        }

        public IRestResponse RestExecute(IRestRequest request, Uri url) {
            Contract.Requires<ArgumentNullException>(request != null);

            var client = GetClient(url);
            HandleRequest(request, client);
            var response = client.Execute(request);
            HandleResponse(request, response, client);

            return response;
        }

        public async Task<IRestResponse> RestExecuteAsync(IRestRequest request, Uri url) {
            Contract.Requires<ArgumentNullException>(request != null);

            var client = GetClient(url);
            HandleRequest(request, client);
            var response = await client.ExecuteTaskAsync(request).ConfigureAwait(false);
            HandleResponse(request, response, client);

            return response;
        }

        public async Task<IRestResponse<T>> RestExecuteAsync<T>(IRestRequest request, Uri url = null)
            where T : new() {
            Contract.Requires<ArgumentNullException>(request != null);

            var client = GetClient(url);
            HandleRequest(request, client);
            var response = await client.ExecuteTaskAsync<T>(request).ConfigureAwait(false);
            HandleResponse(request, response, client);

            return response;
        }

        #endregion

        #region internal rest

        protected IRestRequest CreateGetRequestWithParameters(string path,
            IDictionary<string, object> pars = null) {
            return CreateRequestWithParameters(path, pars);
        }

        protected IRestRequest CreatePostRequestWithParameters(string path,
            IDictionary<string, object> pars = null) {
            var request = CreateRequestWithParameters(path, pars);
            request.Method = Method.POST;
            return request;
        }

        protected IRestRequest CreateDeleteRequestWithParameters(string path,
            IDictionary<string, object> pars = null) {
            var request = CreateRequestWithParameters(path, pars);
            request.Method = Method.DELETE;
            return request;
        }

        protected static IRestRequest CreateRequestWithParameters(string path, IDictionary<string, object> pars = null) {
            var request = new RestRequest(path);
            request.AddHeader("Accept-Encoding", "gzip,deflate");
            if (pars != null)
                pars.ForEach(pair => request.AddParameter(pair.Key, pair.Value));

            return request;
        }

        protected void LogRequestInfo(IRestRequest request, IRestClient client, string type = null) {
            Contract.Requires<ArgumentNullException>(request != null);

            this.Logger().Debug(GetRequestInfo(request, client, type));
        }

        protected void LogResponseInfo(IRestResponse response, IRestClient client,
            string type = null) {
            Contract.Requires<ArgumentNullException>(response != null);

            this.Logger().Debug(GetResponseInfo(response, client, type));
        }

        protected void LogResponseInfo<T>(IRestResponse<T> response, IRestClient client,
            string type = null) {
            Contract.Requires<ArgumentNullException>(response != null);

            this.Logger().Debug(GetResponseInfo(response, client, type));
        }

        protected static string GetRequestInfo(IRestRequest request, IRestClient client, string type = null) {
            return String.Format("RestRequest{0}. {1}, Host: {2} Path: {3}",
                type, request.Method, client.BaseUrl, request.Resource);
            // , GetParams(request)  // breaks when encoding messed
        }

        static string GetParams(IRestRequest request) {
            return request.Parameters == null
                ? String.Empty
                : String.Join("\n", request.Parameters);
        }

        protected static string GetResponseInfo(IRestResponse response, IRestClient client,
            string type = null) {
            Contract.Requires<ArgumentNullException>(response != null);

            return
                String.Format(
                    "RestResponse. Status: {2}, Desc: {3} Message: {1}, ErrorExc: {0}\nType: {4}, Encoding: {5}, Length: {6}, Headers: {7}\nFor: ",
                    response.ErrorException, response.ErrorMessage, response.StatusCode,
                    response.StatusDescription, response.ContentType, response.ContentEncoding,
                    response.ContentLength,
                    GetHeaders(response)) + GetRequestInfoWhenAvailable(response, client, type);
        }

        static string GetRequestInfoWhenAvailable(IRestResponse response, IRestClient client, string type) {
            return response.Request == null ? null : GetRequestInfo(response.Request, client, type);
        }

        protected static string GetResponseInfo<T>(IRestResponse<T> response, IRestClient client,
            string type = null) {
            Contract.Requires<ArgumentNullException>(response != null);

            return
                String.Format(
                    "RestResponse. Status: {2}, Desc: {3} Message: {1}, ErrorExc: {0}\nType: {4}, Encoding: {5}, Length: {6}, Headers: {7}, Data: {8}\nFor: ",
                    response.ErrorException, response.ErrorMessage, response.StatusCode,
                    response.StatusDescription, response.ContentType, response.ContentEncoding,
                    response.ContentLength,
                    GetHeaders(response), response.Data) + GetRequestInfoWhenAvailable(response, client, type);
        }

        static string GetHeaders(IRestResponse response) {
            return response.Headers == null
                ? String.Empty
                : String.Join("\n", response.Headers);
        }

        protected static void HandleResponseException(IRestResponse response, bool handleStatusCode = true) {
            Contract.Requires<ArgumentNullException>(response != null);

            var e = GetResponseException(response, handleStatusCode);
            if (e != null)
                throw e;
        }

        protected static Exception GetResponseException(IRestResponse response, bool handleStatusCode = true) {
            Contract.Requires<ArgumentNullException>(response != null);

            if (response.ErrorException != null) {
                return new RestResponseException(
                    String.Format(
                        "Received invalid response from rest request (EXCEPTION). Status: {0} ({1})\nException: {2} {3}\n{5}\nContentType {4}",
                        response.StatusCode, response.StatusDescription, response.ErrorMessage,
                        response.ErrorException, response.ContentType,
                        response.Request == null ? null : response.Request.Resource),
                    new Exception(string.Format("Content:\n{0}", response.Content), response.ErrorException));
            }


            if (handleStatusCode && (int) response.StatusCode > 399) {
                return new RestStatusException(
                    String.Format(
                        "Received invalid response from rest request (STATUS). Status: {0} ({1})\nException: {2} {3}\n{5}\nContentType {4}",
                        response.StatusCode, response.StatusDescription, response.ErrorMessage,
                        response.ErrorException, response.ContentType,
                        response.Request == null ? null : response.Request.Resource),
                    new Exception(string.Format("Content:\n{0}", response.Content), response.ErrorException));
            }

            return null;
        }

        #endregion
    }

    class RestParams : Dictionary<string, object> {}

    class RestContentInfo
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
    }
}