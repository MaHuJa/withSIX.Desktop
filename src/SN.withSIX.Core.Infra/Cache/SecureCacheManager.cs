// <copyright company="SIX Networks GmbH" file="SecureCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Core.Infra.Cache
{
    public class SecureCacheManager : ObjectCacheManager, IInfrastructureService, ISecureCacheManager
    {
        public SecureCacheManager(ISecureCache localCache) : base(localCache) {}
    }
}