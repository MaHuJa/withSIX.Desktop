// <copyright company="SIX Networks GmbH" file="Initializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Infra.Cache;

namespace SN.withSIX.Core.Infra
{
    public class Initializer : IInitializer
    {
        readonly ICacheManager _cacheManager;

        public Initializer(ICacheManager cacheManager) {
            _cacheManager = cacheManager;
        }

        public async Task Initialize() {}

        public async Task Deinitialize() {
            await _cacheManager.Shutdown().ConfigureAwait(false);
        }
    }
}