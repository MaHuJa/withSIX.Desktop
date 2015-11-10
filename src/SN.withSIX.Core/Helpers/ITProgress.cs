// <copyright company="SIX Networks GmbH" file="ITProgress.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface ITProgress
    {
        long Speed { get; set; }
        double Progress { get; set; }
    }
}