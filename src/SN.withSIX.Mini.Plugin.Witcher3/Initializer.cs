// <copyright company="SIX Networks GmbH" file="Initializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Mini.Plugin.Witcher3
{
    public class Initializer : IInitializer
    {
        public Task Initialize() {
            AutoMapperPluginWitcher3Config.Setup();
            // TODO: Register auto through container??

            return TaskExt.Default;
        }

        public async Task Deinitialize() {}
    }
}