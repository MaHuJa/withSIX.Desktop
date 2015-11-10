// <copyright company="SIX Networks GmbH" file="PublishingApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SN.withSIX.Api.Models.Publishing;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Sync.Core.Services;

namespace SN.withSIX.Sync.Presentation.Console.Services
{
    public class PublishingApi : IPublishingApi, IInfrastructureService
    {
        public async Task<Guid> Publish(PublishModModel model, string registerKey) {
            using (var r =
                await
                    Tools.Transfer.PostJson(model,
                        Tools.Transfer.JoinUri(CommonUrls.PublishApiUrl,
                            "api/v2/publishing/mods?registerKey=" + Uri.EscapeDataString(registerKey)))
                        .ConfigureAwait(false)) {
                r.EnsureSuccessStatusCode();

                return
                    JsonConvert.DeserializeObject<Guid>(
                        await r.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
        }

        public async Task Signal(string registerKey) {
            using (var r =
                await
                    Tools.Transfer.PostJson(new {},
                        Tools.Transfer.JoinUri(CommonUrls.PublishApiUrl,
                            "api/v2/publishing/signal?registerKey=" + Uri.EscapeDataString(registerKey)))
                        .ConfigureAwait(false))
                r.EnsureSuccessStatusCode();
        }

        public async Task Deversion(SpecificVersion nextInline, string registerKey) {
            using (var r =
                await
                    Tools.Transfer.PostJson(new {nextInline.Name, nextInline.VersionData},
                        Tools.Transfer.JoinUri(CommonUrls.PublishApiUrl,
                            "api/v2/publishing/mods_deversion?registerKey=" + Uri.EscapeDataString(registerKey)))
                        .ConfigureAwait(false))
                r.EnsureSuccessStatusCode();
        }
    }
}