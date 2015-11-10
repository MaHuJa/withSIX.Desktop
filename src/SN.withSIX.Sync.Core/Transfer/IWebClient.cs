// <copyright company="SIX Networks GmbH" file="IWebClient.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IWebClient : IDisposable
    {
        ICredentials Credentials { get; set; }
        void DownloadFile(string url, string fileName);
        void DownloadFile(Uri uri, string fileName);
        byte[] UploadFile(string url, string fileName);
        byte[] UploadFile(Uri uri, string fileName);
        Task DownloadFileTaskAsync(Uri uri, string fileName);
        Task DownloadFileTaskAsync(string url, string fileName);
        Task<byte[]> UploadFileTaskAsync(Uri uri, string fileName);
        Task<byte[]> UploadFileTaskAsync(string url, string fileName);
        void CancelAsync();
        event AsyncCompletedEventHandler DownloadFileCompleted;
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event UploadFileCompletedEventHandler UploadFileCompleted;
        event UploadProgressChangedEventHandler UploadProgressChanged;
        string DownloadString(string url);
        Task<string> DownloadStringTaskAsync(Uri uri);
        Task<string> DownloadStringTaskAsync(string url);
        string DownloadString(Uri uri);
        byte[] DownloadData(Uri uri);
        byte[] DownloadData(string url);
        Task<byte[]> DownloadDataTaskAsync(Uri uri);
        Task<byte[]> DownloadDataTaskAsync(string url);
        void SetAuthInfo(string userInfo);
        void SetAuthInfo(Uri uri);
    }

    public static class WebClientExtensions
    {
        public static CancellationTokenRegistration HandleCancellationToken(this IWebClient webClient, TransferSpec spec) {
            var cancellationToken = spec.CancellationToken;
            if (cancellationToken != null)
                return cancellationToken.Register(webClient.CancelAsync);
            return default(CancellationTokenRegistration);
        }
    }
}