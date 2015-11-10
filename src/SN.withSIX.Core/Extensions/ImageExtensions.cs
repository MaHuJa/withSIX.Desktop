// <copyright company="SIX Networks GmbH" file="ImageExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Drawing;
using System.IO;

namespace SN.withSIX.Core.Extensions
{
    public static class ImageExtensions
    {
        public static string ToBase64(this Image image) {
            using (var ms = new MemoryStream()) {
                image.Save(ms, image.RawFormat);
                var imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static Image FromBase64(this string base64String) {
            var imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length)) {
                ms.Write(imageBytes, 0, imageBytes.Length);
                return Image.FromStream(ms, true);
            }
        }
    }
}