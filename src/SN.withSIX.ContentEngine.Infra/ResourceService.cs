// <copyright company="SIX Networks GmbH" file="ResourceService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Reflection;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.ContentEngine.Infra
{
    public interface ICEResourceService
    {
        Stream GetResource(string path);
        bool ResourceExists(string path);
    }

    public class CEResourceService : ICEResourceService, IInfrastructureService
    {
        const string SourceAssemblyName = "SN.withSIX.ContentEngine.Infra";
        readonly Assembly _sourceAssembly = typeof (ContentEngine).Assembly;

        static CEResourceService() {
            ComponentPath = "/" + SourceAssemblyName + ";component";
            ResourcePath = "pack://application:,,," + ComponentPath;
        }

        public static string ComponentPath { get; }
        public static string ResourcePath { get; private set; }

        public Stream GetResource(string path) {
            return
                _sourceAssembly.GetManifestResourceStream(GetResourcePath(path));
        }

        public bool ResourceExists(string path) {
            // GetManifestResourceNames cannot be used in conjunction with SmartAssembly compressed/encrypted resources!!!
            var manifestResourceNames = _sourceAssembly.GetManifestResourceNames();
            return manifestResourceNames.ContainsIgnoreCase(GetResourcePath(path));
        }

        static string GetResourcePath(string path) {
            return SourceAssemblyName + "." +
                   path.Replace("/", ".").Replace("\\", ".");
        }
    }
}