// <copyright company="SIX Networks GmbH" file="Initializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using RpfGeneratorTool;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Mini.Plugin.GTA
{
    public class Initializer : IInitializer
    {
        public Task Initialize() {
            AutoMapperPluginGTAConfig.Setup();
            // TODO: Register auto through container??
            UiTaskHandler.RegisterHandler(new GTAExceptionHandler());
            var p = new Package();

            return TaskExt.Default;
        }

        public async Task Deinitialize() {}
    }
}