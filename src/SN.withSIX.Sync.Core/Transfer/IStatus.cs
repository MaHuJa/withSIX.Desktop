// <copyright company="SIX Networks GmbH" file="IStatus.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Sync.Core.Legacy.Status;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IStatus : ITransferStatus, IComparable<IStatus>
    {
        StatusRepo Repo { get; }
        string RealObject { get; set; }
    }
}