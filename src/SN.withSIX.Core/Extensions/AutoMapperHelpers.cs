// <copyright company="SIX Networks GmbH" file="AutoMapperHelpers.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using AutoMapper;
using NDepend.Path;
using SN.withSIX.Api.Models;

namespace SN.withSIX.Core.Extensions
{
    public static class AutoMapperHelpers
    {
        public static void SetupConverters(this IConfiguration config) {
            config.SetupUriConverter();
            config.SetupDirConverter();
            config.SetupVersionConverter();
            config.SetupServerAddressConverter();
        }

        static void SetupUriConverter(this IConfiguration config) {
            config.CreateMap<Uri, string>().ConvertUsing<UriToStringConverter>();
            config.CreateMap<string, Uri>().ConvertUsing<StringToUriConverter>();
        }

        static void SetupServerAddressConverter(this IConfiguration config) {
            config.CreateMap<ServerAddress, string>().ConvertUsing<ServerAddressToStringConverter>();
            config.CreateMap<string, ServerAddress>().ConvertUsing<StringToServerAddressConverter>();
        }

        static void SetupVersionConverter(this IConfiguration config) {
            config.CreateMap<Version, string>().ConvertUsing<VersionToStringConverter>();
            config.CreateMap<string, Version>().ConvertUsing<StringToVersionConverter>();
        }

        static void SetupDirConverter(this IConfiguration config) {
            config.CreateMap<IAbsoluteDirectoryPath, string>().ConvertUsing<AbsoluteDirToStringConverter>();
            config.CreateMap<string, IAbsoluteDirectoryPath>().ConvertUsing<StringToAbsoluteDirConverter>();
            config.CreateMap<IAbsoluteFilePath, string>().ConvertUsing<AbsoluteFileToStringConverter>();
            config.CreateMap<string, IAbsoluteFilePath>().ConvertUsing<StringToAbsoluteFileConverter>();
        }

        class AbsoluteDirToStringConverter : ITypeConverter<IAbsoluteDirectoryPath, string>
        {
            public string Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : context.SourceValue.ToString();
            }
        }

        class AbsoluteFileToStringConverter : ITypeConverter<IAbsoluteFilePath, string>
        {
            public string Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : context.SourceValue.ToString();
            }
        }

        class StringToAbsoluteDirConverter : ITypeConverter<string, IAbsoluteDirectoryPath>
        {
            public IAbsoluteDirectoryPath Convert(ResolutionContext context) {
                return context.IsSourceValueNull
                    ? null
                    : ((string) context.SourceValue).ToAbsoluteDirectoryPathNullSafe();
            }
        }

        class StringToAbsoluteFileConverter : ITypeConverter<string, IAbsoluteFilePath>
        {
            public IAbsoluteFilePath Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : ((string) context.SourceValue).ToAbsoluteFilePathNullSafe();
            }
        }

        class StringToUriConverter : ITypeConverter<string, Uri>
        {
            public Uri Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : new Uri((string) context.SourceValue);
            }
        }

        class StringToServerAddressConverter : ITypeConverter<string, ServerAddress>
        {
            public ServerAddress Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : new ServerAddress((string) context.SourceValue);
            }
        }

        class StringToVersionConverter : ITypeConverter<string, Version>
        {
            public Version Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : Version.Parse((string) context.SourceValue);
            }
        }

        class UriToStringConverter : ITypeConverter<Uri, string>
        {
            public string Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : ((Uri) context.SourceValue).AbsoluteUri;
            }
        }

        class ServerAddressToStringConverter : ITypeConverter<ServerAddress, string>
        {
            public string Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : ((ServerAddress) context.SourceValue).ToString();
            }
        }

        class VersionToStringConverter : ITypeConverter<Version, string>
        {
            public string Convert(ResolutionContext context) {
                return context.IsSourceValueNull ? null : context.SourceValue.ToString();
            }
        }
    }
}