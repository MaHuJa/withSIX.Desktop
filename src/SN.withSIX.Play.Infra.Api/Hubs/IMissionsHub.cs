// <copyright company="SIX Networks GmbH" file="IMissionsHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SignalRNetClientProxyMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Api.Models.Content.Arma3;
using SN.withSIX.Api.Models.Shared;

namespace SN.withSIX.Play.Infra.Api.Hubs
{
    [DoNotObfuscateType]
    interface IMissionsHub : IClientHubProxyBase
    {
        Task<AWSUploadPolicy> RequestMissionUpload(RequestMissionUploadModel model);
        Task<MissionModel> MissionUploadCompleted(MissionUploadedModel model);
        Task<PageModel<MissionModel>> GetMyMissions(string type, int page);
    }
}