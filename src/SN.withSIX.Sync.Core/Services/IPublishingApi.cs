// <copyright company="SIX Networks GmbH" file="IPublishingApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Api.Models.Publishing;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Services
{
    public interface IPublishingApi
    {
        Task<Guid> Publish(PublishModModel model, string registerKey);
        Task Signal(string registerKey);
        Task Deversion(SpecificVersion nextInline, string registerKey);
    }
}