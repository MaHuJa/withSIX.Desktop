// <copyright company="SIX Networks GmbH" file="IConnectionManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Play.Infra.Api.Hubs;

namespace SN.withSIX.Play.Infra.Api
{
    [DoNotObfuscateType]
    interface IConnectionManager
    {
        IMessageBus MessageBus { get; }
        IChatHub ChatHub { get; }
        IAccountHub AccountHub { get; }
        IGroupHub GroupHub { get; }
        ICollectionsHub CollectionsHub { get; }
        IMissionsHub MissionsHub { get; }
        IApiHub ApiHub { get; }
        string ApiKey { get; }
        ContextModel Context();
        Task Start(string key = null);
        Task SetupContext();
        bool IsConnected();
        bool IsLoggedIn();
        Task Stop();
    }
}