// <copyright company="SIX Networks GmbH" file="AutoMapperInfraDataConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Infra.Data.Services;

namespace SN.withSIX.Mini.Infra.Data
{
    public class AutoMapperInfraDataConfig
    {
        public static void Setup() {
            Cheat.MapperConfiguration.CreateMap<GameContextJsonImplementation, GameContextDto>();
        }
    }
}