// <copyright company="SIX Networks GmbH" file="Initializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Infra.Data
{
    public class Initializer : IInitializer
    {
        public async Task Initialize() {
            AutoMapperInfraDataConfig.Setup();
        }

        public async Task Deinitialize() {}
    }
}