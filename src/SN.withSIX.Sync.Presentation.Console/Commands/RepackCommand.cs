// <copyright company="SIX Networks GmbH" file="RepackCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Linq;
using ManyConsole;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.ExternalTools;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class RepackCommand : BaseCommand
    {
        readonly PboTools _pboTools;

        public RepackCommand(PboTools pboTools) {
            IsCommand("repack", "Repack a folder of pbos");
            _pboTools = pboTools;
            AllowsAnyAdditionalArguments(" <folder> (<folder>...)");
        }

        public override int Run(params string[] remainingArguments) {
            if (!remainingArguments.Any())
                throw new ConsoleHelpAsException("Please specify at least one folder to repack pbos of");

            foreach (
                var pbo in
                    remainingArguments.Select(dir => Repository.RepoTools.GetRootedPath(dir))
                        .SelectMany(path => Directory.EnumerateFiles(path.ToString(), "*.pbo"))
                        .Select(x => x.ToAbsoluteFilePath())
                )
                _pboTools.RepackPbo(pbo);

            return 0;
        }
    }
}