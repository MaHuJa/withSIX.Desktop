// <copyright company="SIX Networks GmbH" file="ICopyProperties.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface ICopyProperties
    {
        string[] IgnoredProperties { get; }
    }
}