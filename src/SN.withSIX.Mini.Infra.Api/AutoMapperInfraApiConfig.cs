// <copyright company="SIX Networks GmbH" file="AutoMapperInfraApiConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Infra.Api.WebApi;

namespace SN.withSIX.Mini.Infra.Api
{
    public class AutoMapperInfraApiConfig
    {
        public static void Setup() {
            Cheat.MapperConfiguration.CreateMap<ContentDto, NetworkContent>()
                .ForMember(x => x.Dependencies, opt => opt.Ignore())
                .ForMember(x => x.Image,
                    opt =>
                        opt.MapFrom(
                            src => src.ImagePath == null ? null : new Uri(CommonUrls.UsercontentCdnProduction, src.ImagePath)))
                .ForMember(x => x.Aliases, opt => opt.ResolveUsing(ResolveAliases))
                .ForMember(x => x.RecentInfo, opt => opt.Ignore())
                .ForMember(x => x.IsFavorite, opt => opt.Ignore())
                .Include<ModDto, ModNetworkContent>()
                .Include<MissionDto, MissionNetworkContent>();

            // TODO: Why do the above includes not work??
            Cheat.MapperConfiguration.CreateMap<ModDto, ModNetworkContent>()
                .ForMember(x => x.Dependencies, opt => opt.Ignore())
                .ForMember(x => x.Aliases, opt => opt.ResolveUsing(ResolveAliases))
                .ForMember(x => x.RecentInfo, opt => opt.Ignore())
                .ForMember(x => x.IsFavorite, opt => opt.Ignore());
            Cheat.MapperConfiguration.CreateMap<MissionDto, MissionNetworkContent>()
                .ForMember(x => x.Aliases, opt => opt.ResolveUsing(ResolveAliases))
                .ForMember(x => x.Dependencies, opt => opt.Ignore())
                .ForMember(x => x.RecentInfo, opt => opt.Ignore())
                .ForMember(x => x.IsFavorite, opt => opt.Ignore());

            Cheat.MapperConfiguration.CreateMap<CollectionVersionModel, SubscribedCollection>()
                .ForMember(x => x.Dependencies, opt => opt.Ignore())
                .ForMember(x => x.Id, opt => opt.Ignore());
            Cheat.MapperConfiguration.CreateMap<CollectionModelWithLatestVersion, SubscribedCollection>()
                .ForMember(x => x.Image,
                    opt => opt.MapFrom(src => src.AvatarUrl == null ? null : ("https:" + src.AvatarUrl)))
                .AfterMap((src, dst) => src.LatestVersion.MapTo(dst));

            Cheat.MapperConfiguration.CreateMap<CollectionServer, CollectionVersionServerModel>();
            Cheat.MapperConfiguration.CreateMap<CollectionVersionServerModel, CollectionServer>();
        }

        static IEnumerable<string> ResolveAliases(ModDto arg) {
            if (arg.Aliases != null) {
                foreach (var e in arg.Aliases.Split(';'))
                    yield return e;
            }
            if (arg.CppName != null)
                yield return arg.CppName;
        }

        static IEnumerable<string> ResolveAliases(ContentDto arg) {
            if (arg.Aliases == null)
                yield break;
            foreach (var e in arg.Aliases.Split(';'))
                yield return e;
        }
    }
}