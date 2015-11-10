// <copyright company="SIX Networks GmbH" file="AssemblyRegistry.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SN.withSIX.Core.Presentation.Assemblies
{
    public class AssemblyRegistry
    {
        //static Assembly _assembly;
        readonly ReferencedAssemblyFinder _finder = new ReferencedAssemblyFinder();
        //readonly string _sharedDllPath = AssemblyHandler.GetSharedDllPath();
        //static Assembly ResourceAssembly => _assembly ?? (_assembly = Assembly.Load("SN.withSIX.Core.Presentation.Assemblies"));

/*        public void Register(string path, IEnumerable<string> files) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var dllFileHandler = new DllFileHandler();
            foreach (var f in files)
                dllFileHandler.HandleDll(path, f);

            SetupPath(path);
        }*/

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var requestedAssemblyName = new AssemblyName(args.Name);
/*            Assembly load;
            if (GetFromResources(requestedAssemblyName, out load))
                return load;*/

            var localFile = Path.Combine(CommonBase.AssemblyLoader.GetEntryPath(), requestedAssemblyName.Name + ".dll");
            if (File.Exists(localFile))
                return Assembly.LoadFile(localFile);

            // referenced doesnt include the references of other dlls, so e.g System.Web.Owin etc
            var assembly = _finder.FindReferencedMismatchedVersionAssembly(requestedAssemblyName);
            return assembly != null ? Assembly.Load(assembly) : null;
        }

/*        static bool GetFromResources(AssemblyName requestedAssemblyName, out Assembly load) {
            var fileName = GetFileName(requestedAssemblyName);
            var resourcePath = "SN.withSIX.Core.Presentation.Assemblies." + fileName;
            try {
                using (var a = ResourceAssembly.GetManifestResourceStream(resourcePath)) {
                    // This can actually end up in infinite loop, e.g if we have an assembly redirect to a version that we don't have, like Cors 5.2, while having 5.0
                    if (a != null) {
                        load = Assembly.Load(StreamToBytes(a));
                        return true;
                    }
                }
            } catch (FileNotFoundException) {}
            load = null;
            return false;
        }*/

        static string GetFileName(AssemblyName requestedAssemblyName) {
            return requestedAssemblyName.Name.ToLowerInvariant() + ".dll";
        }

/*        static Stream GetAssemblyStreamFromResource(string resourceName) {
            return
                ResourceAssembly.GetManifestResourceStream("SN.withSIX.Core.Presentation.Assemblies." +
                                                           resourceName.Replace("/", "."));
        }

        static byte[] StreamToBytes(Stream sourceStream) {
            using (var memoryStream = new MemoryStream()) {
                sourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }*/

        static void SetupPath(string path) {
            Environment.SetEnvironmentVariable("path",
                string.Join(";", Path.Combine(path, "x86"), path, Environment.GetEnvironmentVariable("path")));
        }

/*        class DllFileHandler
        {
            public void HandleDll(string path, string dllFileName) {
                var localDllPath = Path.Combine(path, dllFileName);
                if (ValidateFileOnDisk(dllFileName, localDllPath))
                    return;
                var directoryName = Path.GetDirectoryName(localDllPath);
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                WriteAssemblyFile(dllFileName, localDllPath);
            }

            static void WriteAssemblyFile(string dllFileName, string localDllPath) {
                using (var stream = GetAssemblyStreamFromResource(dllFileName))
                    File.WriteAllBytes(localDllPath, StreamToBytes(stream));
            }

            static bool ValidateFileOnDisk(string f, string file) {
                return File.Exists(file) && CompareStreamAndFileChecksum(f, file);
            }

            static bool CompareStreamAndFileChecksum(string f, string file) {
                using (var stream = HashTool.GetBufferedStream(GetAssemblyStreamFromResource(f)))
                    return HashTool.SHA1StreamHash(stream).Equals(HashTool.SHA1FileHash(file));
            }
        }*/

        class ReferencedAssemblyFinder
        {
            readonly AssemblyName[] _referencedAssemblies;

            public ReferencedAssemblyFinder() {
                var defaultVersion = new Version("0.0.0");
                _referencedAssemblies =
                    Assembly.GetExecutingAssembly()
                        .GetReferencedAssemblies()
                        .OrderByDescending(x => x.Version ?? defaultVersion)
                        .ToArray();
            }

            public AssemblyName FindReferencedMismatchedVersionAssembly(AssemblyName requestedAssemblyName) {
                return _referencedAssemblies.FirstOrDefault(assembly => Predicate(requestedAssemblyName, assembly));
            }

            static bool Predicate(AssemblyName requestedAssemblyName, AssemblyName assembly) {
                if (assembly.Version == null ||
                    !assembly.Name.Equals(requestedAssemblyName.Name, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                // Do not use >= or we end up in StackOverflow :)
                return requestedAssemblyName.Version == null
                    ? assembly.Version != null
                    : assembly.Version > requestedAssemblyName.Version;
            }
        }
    }
}