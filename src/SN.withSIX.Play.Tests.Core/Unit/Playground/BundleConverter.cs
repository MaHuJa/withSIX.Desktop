// <copyright company="SIX Networks GmbH" file="BundleConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using AutoMapper;
using AutoMapper.Mappers;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground
{
    // TODO: Probably better to make this share-able through the website
    // reasons:
    // - Cloud-feel
    //   - Comfortable
    //   - No need to host it somewhere self
    //   - Scope: Public, Private Closed (specific users/groups), Open (sha1 url)
    // - More interaction with website
    // - Ads

    // - Probably good to also be able to host bundles elsewhere and import/use them, e.g from custom repositories
    //   but the primary form should probably be through the website..
    public class BundleConverter
    {
        readonly MappingEngine _mapper;

        public BundleConverter() {
            _mapper = CreateMapper();
        }

        public CustomCollection LoadFromBundle(IAbsoluteFilePath filePath) {
            var bundle = ReadBundleFromDisk(filePath);
            return CreateModSet(bundle);
        }

        public void SaveAsBundle(CustomCollection collection, IAbsoluteDirectoryPath destination) {
            WriteBundleToDisk(CreateBundle(collection), destination);
        }

        CustomCollection CreateModSet(Bundle bundle) {
            return _mapper.Map<CustomCollection>(bundle);
        }

        static Bundle ReadBundleFromDisk(IAbsoluteFilePath filePath) {
            return Repository.Load<BundleDto, Bundle>(filePath);
        }

        static void WriteBundleToDisk(Bundle bundle, IAbsoluteDirectoryPath destination) {
            Repository.SaveDto(CreateBundleDto(bundle),
                destination.GetChildFileWithName(GetBundleFileName(bundle)));
        }

        static string GetBundleFileName(Bundle bundle) {
            return bundle.GetFullName() + Repository.PackageFormat;
        }

        static BundleDto CreateBundleDto(Bundle bundle) {
            return Repository.MappingEngine.Map<BundleDto>(bundle);
        }

        Bundle CreateBundle(CustomCollection collection) {
            return _mapper.Map<Bundle>(collection);
        }

        static MappingEngine CreateMapper() {
            var mappingConfig = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            mappingConfig.SetupConverters();
            mappingConfig.CreateMap<Bundle, CustomCollection>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.GetFullName()))
                .ForMember(x => x.RequiredMods, opt => opt.MapFrom(src => src.GetRequiredClient()))
                .ForMember(x => x.OptionalMods, opt => opt.MapFrom(src => src.GetOptionalClient()));

            mappingConfig.CreateMap<CustomCollection, Bundle>()
                .ForMember(x => x.Name, opt => opt.ResolveUsing(src => PackageHelper.Packify(src.Name)))
                .ForMember(x => x.FullName, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(x => x.RequiredClients, opt => opt.ResolveUsing(src => src.RequiredMods))
                .ForMember(x => x.OptionalClients, opt => opt.ResolveUsing(src => src.OptionalMods));

            return new MappingEngine(mappingConfig);
        }
    }
}