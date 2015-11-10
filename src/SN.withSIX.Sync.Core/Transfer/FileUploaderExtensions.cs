// <copyright company="SIX Networks GmbH" file="FileUploaderExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer
{
    public static class FileUploaderExtensions
    {
        public static void Upload(this IFileUploader uploader, IAbsoluteFilePath localFile, string url) {
            uploader.Upload(new FileUploadSpec(localFile, url));
        }

        public static void Upload(this IFileUploader uploader, IAbsoluteFilePath localFile, Uri uri) {
            uploader.Upload(new FileUploadSpec(localFile, uri));
        }

        public static void Upload(this IFileUploader uploader, IAbsoluteFilePath localFile, string url,
            ITransferProgress progress) {
            uploader.Upload(new FileUploadSpec(localFile, url, progress));
        }

        public static void Upload(this IFileUploader uploader, IAbsoluteFilePath localFile, Uri uri,
            ITransferProgress progress) {
            uploader.Upload(new FileUploadSpec(localFile, uri, progress));
        }

        public static Task UploadAsync(this IFileUploader uploader, IAbsoluteFilePath localFile, string url) {
            return uploader.UploadAsync(new FileUploadSpec(localFile, url));
        }

        public static Task UploadAsync(this IFileUploader uploader, IAbsoluteFilePath localFile, Uri uri) {
            return uploader.UploadAsync(new FileUploadSpec(localFile, uri));
        }

        public static Task UploadAsync(this IFileUploader uploader, IAbsoluteFilePath localFile, string url,
            ITransferProgress progress) {
            return uploader.UploadAsync(new FileUploadSpec(localFile, url, progress));
        }

        public static Task UploadAsync(this IFileUploader uploader, IAbsoluteFilePath localFile, Uri uri,
            ITransferProgress progress) {
            return uploader.UploadAsync(new FileUploadSpec(localFile, uri, progress));
        }
    }
}