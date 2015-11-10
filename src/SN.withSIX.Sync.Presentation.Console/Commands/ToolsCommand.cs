// <copyright company="SIX Networks GmbH" file="ToolsCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Presentation.Console.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class ToolsCommand : BaseCommandAsync
    {
        readonly IToolsInstaller _toolsInstaller;

        public ToolsCommand(IToolsInstaller toolsInstaller) {
            _toolsInstaller = toolsInstaller;
            IsCommand("tools", "Download Six Tools through SynQ");
        }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            using (var statusRepo = new StatusRepo())
            using (new ConsoleProgress(statusRepo))
                await _toolsInstaller.DownloadAndInstallTools(statusRepo).ConfigureAwait(false);

            return 0;
        }
    }
}