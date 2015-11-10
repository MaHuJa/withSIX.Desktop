// <copyright company="SIX Networks GmbH" file="MirrorSelector.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Sync.Core.Transfer.MirrorSelectors
{
    public interface IMirrorSelector
    {
        Uri GetHost();
        void Failure(Uri host);
        void Success(Uri host);
    }
}