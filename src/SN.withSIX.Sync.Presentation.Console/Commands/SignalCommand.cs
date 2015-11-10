// <copyright company="SIX Networks GmbH" file="SignalCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class SignalCommand : BaseCommandAsync
    {
        readonly IPublishingApi _publishingApi;
        SynqConfig _config;

        public SignalCommand(IPublishingApi publishingApi) {
            _publishingApi = publishingApi;
            IsCommand("signal", "Signal API");
        }

        protected override async Task<int> RunAsync(string[] remainingArguments) {
            _config = GetConfig();
            if (_config.RegisterKey == null)
                throw new Exception("No key registered");
            await _publishingApi.Signal(_config.RegisterKey).ConfigureAwait(false);
            return 0;
        }
    }
}