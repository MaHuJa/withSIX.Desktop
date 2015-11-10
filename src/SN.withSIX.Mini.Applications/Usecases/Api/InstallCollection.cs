// <copyright company="SIX Networks GmbH" file="InstallCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class InstallCollection : CompositeCommandBasicVoid
    {
        public InstallCollection(Guid gameId, ContentGuidSpec content)
            : base(new InstallContent(gameId, content), new SyncCollections(gameId, new List<ContentGuidSpec> {content})
                ) {}
    }
}