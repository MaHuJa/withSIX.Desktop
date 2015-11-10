// <copyright company="SIX Networks GmbH" file="ResourceService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Reflection;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Presentation.Resources;

namespace SN.withSIX.Core.Presentation.Services
{
    public class ResourceService : IResourceService, IPresentationService
    {
        const string SourceAssemblyName = "SN.withSIX.Core.Presentation.Resources";
        readonly Assembly _sourceAssembly = typeof (DummyClass).Assembly;

        static ResourceService() {
            ComponentPath = "/" + SourceAssemblyName + ";component";
            ResourcePath = "pack://application:,,," + ComponentPath;
        }

        public static string ComponentPath { get; }
        public static string ResourcePath { get; private set; }

        public Stream GetResource(string path) {
            return _sourceAssembly.GetManifestResourceStream(GetResourcePath(path));
        }

        static string GetResourcePath(string path)
        {
            return SourceAssemblyName + "." +
                   path.Replace("/", ".").Replace("\\", ".");
        }
    }
}