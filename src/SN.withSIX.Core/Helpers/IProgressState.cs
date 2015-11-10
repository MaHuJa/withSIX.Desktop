// <copyright company="SIX Networks GmbH" file="IProgressState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface IProgressState
    {
        double Progress { get; set; }
        double Maximum { get; set; }
        bool Active { get; set; }
        bool IsIndeterminate { get; set; }
    }
}