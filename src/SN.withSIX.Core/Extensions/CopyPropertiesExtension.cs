// <copyright company="SIX Networks GmbH" file="CopyPropertiesExtension.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using MoreLinq;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Extensions
{
    public static class CopyPropertiesExtension
    {
        public static void CopyProperties(this object src, object dest, string[] extraExclusions = null) {
            Contract.Requires<ArgumentNullException>(src != null);
            Contract.Requires<ArgumentNullException>(dest != null);

            if (ReferenceEquals(src, dest))
                return;

            var s = src as ICopyProperties;
            if (s == null) {
                src.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(item => item.CanRead && item.CanWrite
                                   && (extraExclusions == null || extraExclusions.None(x => x == item.Name)))
                    .ForEach(item => item.SetValue(dest, item.GetValue(src, null), null));
                return;
            }

            src.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(
                    item =>
                        item.CanRead && item.CanWrite &&
                        (s.IgnoredProperties == null ||
                         s.IgnoredProperties.None(x => x == item.Name))
                        && (extraExclusions == null || extraExclusions.None(x => x == item.Name)))
                .ForEach(item => item.SetValue(dest, item.GetValue(src, null), null));
        }

        public static void CopyFromProperties(this object dest, object src, string[] extraExclusions = null) {
            src.CopyProperties(dest, extraExclusions);
        }

        public static string PrintProperties(this object src) {
            var sb = new StringBuilder();
            src.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(
                    item =>
                        item.CanRead && item.CanWrite
                ).ForEach(x => {
                    var val = x.GetValue(src, null);
                    sb.AppendLine(x.Name + ": " + (val == null ? "<null>" : val.ToString()));
                });

            return sb.ToString();
        }
    }
}