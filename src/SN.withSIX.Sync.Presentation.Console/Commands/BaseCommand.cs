// <copyright company="SIX Networks GmbH" file="BaseCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Threading.Tasks;
using ManyConsole;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    public abstract class BaseCommand : ConsoleCommand
    {
        protected Repository GetRepo(IAbsoluteDirectoryPath path,
            bool createWhenNotExisting = false) {
            var failedOnce = false;
            return Repository.Factory.OpenRepositoryWithRetry(path,
                createWhenNotExisting, () => {
                    if (failedOnce)
                        return;
                    System.Console.WriteLine("Repository seems locked, retrying indefinitely....");
                    failedOnce = true;
                });
        }

        protected SynqConfig GetConfig() {
            try {
                return SynqConfig.Load();
            } catch (FileNotFoundException) {
                return new SynqConfig();
            }
        }
    }

    public abstract class BaseCommandAsync : BaseCommand
    {
        public override sealed int Run(string[] remainingArguments) {
            return RunAsync(remainingArguments).WaitAndUnwrapException();
        }

        protected abstract Task<int> RunAsync(string[] remainingArguments);
    }
}