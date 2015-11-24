// <copyright company="SIX Networks GmbH" file="ContentApiHandlerTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using NUnit.Framework;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Infra.Api.ContentApi;
using SN.withSIX.Play.Tests.Core.Support;

namespace SN.withSIX.Play.Tests.Core.Integration.Games
{
    [TestFixture, Ignore(""), Category("Integration")]
    public class ContentApiHandlerTest
    {
        ContentApiHandler _contentApi;

        [SetUp]
        public void SetUp() {
            SharedSupport.Init();
            _contentApi = new ContentApiHandler(new UserSettings(), new ApiLocalObjectCacheManager(new ApiLocalCache("LocalAppData/cache.db")));
        }

        [Test]
        public async Task LoadFromApi() {
            await _contentApi.LoadFromApi();
        }

        [Test]
        public async Task LoadFromDisk() {
            await _contentApi.LoadFromApi().ConfigureAwait(false);
            await _contentApi.LoadFromDisk().ConfigureAwait(false);
        }
    }
}